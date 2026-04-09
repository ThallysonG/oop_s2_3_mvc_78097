using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class ExamResultsController : Controller
    {
        private readonly ExamResultService _examResultService;

        public ExamResultsController(ExamResultService examResultService)
        {
            _examResultService = examResultService;
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Index()
        {
            var items = await _examResultService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyResults()
        {
            var currentEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentEmail))
                return Forbid();

            var items = await _examResultService.GetVisibleByStudentEmailAsync(currentEmail);
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _examResultService.GetByIdAsync(id);
            if (item == null) return NotFound();

            if (User.IsInRole("Student"))
            {
                var currentEmail = User.Identity?.Name;

                if (!string.Equals(item.StudentProfile?.User?.Email, currentEmail, StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                if (item.Exam != null && !item.Exam.ResultsReleased)
                    return Forbid();
            }

            return View(item);
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Create(ExamResult examResult)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(examResult);
            }

            try
            {
                await _examResultService.CreateAsync(examResult);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownsAsync();
                return View(examResult);
            }
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _examResultService.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadDropdownsAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id, ExamResult examResult)
        {
            if (id != examResult.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(examResult);
            }

            try
            {
                await _examResultService.UpdateAsync(examResult);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownsAsync();
                return View(examResult);
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _examResultService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examResultService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            var exams = await _examResultService.GetExamsAsync();
            var students = await _examResultService.GetStudentsAsync();

            ViewBag.ExamId = new SelectList(
                exams.Select(e => new
                {
                    e.Id,
                    Description = $"{e.Title} - {e.Course!.Name}"
                }),
                "Id",
                "Description");

            ViewBag.StudentProfileId = new SelectList(students, "Id", "Name");
        }
    }
}
