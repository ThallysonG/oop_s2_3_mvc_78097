using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class ExamsController : Controller
    {
        private readonly ExamService _examService;

        public ExamsController(ExamService examService)
        {
            _examService = examService;
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Index()
        {
            var items = await _examService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _examService.GetByIdAsync(id);
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
        public async Task<IActionResult> Create(Exam exam)
        {
            if (!ModelState.IsValid)
            {
                await LoadCoursesAsync();
                return View(exam);
            }

            await _examService.CreateAsync(exam);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _examService.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadCoursesAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id, Exam exam)
        {
            if (id != exam.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadCoursesAsync();
                return View(exam);
            }

            await _examService.UpdateAsync(exam);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _examService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCoursesAsync()
        {
            var courses = await _examService.GetCoursesAsync();
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
