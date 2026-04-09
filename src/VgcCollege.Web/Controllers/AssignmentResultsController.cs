using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class AssignmentResultsController : Controller
    {
        private readonly AssignmentResultService _assignmentResultService;

        public AssignmentResultsController(AssignmentResultService assignmentResultService)
        {
            _assignmentResultService = assignmentResultService;
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Index()
        {
            var items = await _assignmentResultService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyResults()
        {
            var currentEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentEmail))
                return Forbid();

            var items = await _assignmentResultService.GetByStudentEmailAsync(currentEmail);
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _assignmentResultService.GetByIdAsync(id);
            if (item == null) return NotFound();

            if (User.IsInRole("Student"))
            {
                var currentEmail = User.Identity?.Name;
                if (!string.Equals(item.StudentProfile?.User?.Email, currentEmail, StringComparison.OrdinalIgnoreCase))
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
        public async Task<IActionResult> Create(AssignmentResult assignmentResult)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(assignmentResult);
            }

            try
            {
                await _assignmentResultService.CreateAsync(assignmentResult);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownsAsync();
                return View(assignmentResult);
            }
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _assignmentResultService.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadDropdownsAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id, AssignmentResult assignmentResult)
        {
            if (id != assignmentResult.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(assignmentResult);
            }

            try
            {
                await _assignmentResultService.UpdateAsync(assignmentResult);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownsAsync();
                return View(assignmentResult);
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _assignmentResultService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _assignmentResultService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            var assignments = await _assignmentResultService.GetAssignmentsAsync();
            var students = await _assignmentResultService.GetStudentsAsync();

            ViewBag.AssignmentId = new SelectList(
                assignments.Select(a => new
                {
                    a.Id,
                    Description = $"{a.Title} - {a.Course!.Name}"
                }),
                "Id",
                "Description");

            ViewBag.StudentProfileId = new SelectList(students, "Id", "Name");
        }
    }
}