using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class CourseEnrolmentsController : Controller
    {
        private readonly CourseEnrolmentService _courseEnrolmentService;

        public CourseEnrolmentsController(CourseEnrolmentService courseEnrolmentService)
        {
            _courseEnrolmentService = courseEnrolmentService;
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Index()
        {
            var items = await _courseEnrolmentService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _courseEnrolmentService.GetByIdAsync(id);
            if (item == null) return NotFound();

            if (User.IsInRole("Student"))
            {
                var currentEmail = User.Identity?.Name;
                if (!string.Equals(item.StudentProfile?.User?.Email, currentEmail, StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }

            return View(item);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyEnrolments()
        {
            var currentEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(currentEmail))
                return Forbid();

            var items = await _courseEnrolmentService.GetByStudentEmailAsync(currentEmail);
            return View(items);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(CourseEnrolment enrolment)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(enrolment);
            }

            try
            {
                await _courseEnrolmentService.CreateAsync(enrolment);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownsAsync();
                return View(enrolment);
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _courseEnrolmentService.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadDropdownsAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, CourseEnrolment enrolment)
        {
            if (id != enrolment.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(enrolment);
            }

            try
            {
                await _courseEnrolmentService.UpdateAsync(enrolment);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownsAsync();
                return View(enrolment);
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _courseEnrolmentService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _courseEnrolmentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            var students = await _courseEnrolmentService.GetStudentsAsync();
            var courses = await _courseEnrolmentService.GetCoursesAsync();

            ViewBag.StudentProfileId = new SelectList(students, "Id", "Name");
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
