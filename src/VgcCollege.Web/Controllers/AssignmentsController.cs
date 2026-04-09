using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class AssignmentsController : Controller
    {
        private readonly AssignmentService _assignmentService;

        public AssignmentsController(AssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Index()
        {
            var items = await _assignmentService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _assignmentService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Create()
        {
            await LoadCoursesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Create(Assignment assignment)
        {
            if (!ModelState.IsValid)
            {
                await LoadCoursesAsync();
                return View(assignment);
            }

            await _assignmentService.CreateAsync(assignment);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _assignmentService.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadCoursesAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id, Assignment assignment)
        {
            if (id != assignment.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadCoursesAsync();
                return View(assignment);
            }

            await _assignmentService.UpdateAsync(assignment);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _assignmentService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _assignmentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCoursesAsync()
        {
            var courses = await _assignmentService.GetCoursesAsync();
            ViewBag.CourseId = new SelectList(
                courses.Select(c => new
                {
                    c.Id,
                    Description = $"{c.Name} - {c.Branch!.Name}"
                }),
                "Id",
                "Description");
        }
    }
}
