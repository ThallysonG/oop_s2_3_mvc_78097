using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly CourseService _courseService;

        public CoursesController(CourseService courseService)
        {
            _courseService = courseService;
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Index()
        {
            var items = await _courseService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _courseService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            await LoadBranchesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(Course course)
        {
            if (!ModelState.IsValid)
            {
                await LoadBranchesAsync();
                return View(course);
            }

            await _courseService.CreateAsync(course);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _courseService.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadBranchesAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadBranchesAsync();
                return View(course);
            }

            await _courseService.UpdateAsync(course);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _courseService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _courseService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBranchesAsync()
        {
            var branches = await _courseService.GetBranchesAsync();
            ViewBag.BranchId = new SelectList(branches, "Id", "Name");
        }
    }
}
