using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class StudentProfilesController : Controller
    {
        private readonly StudentProfileService _studentProfileService;

        public StudentProfilesController(StudentProfileService studentProfileService)
        {
            _studentProfileService = studentProfileService;
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Index()
        {
            var items = await _studentProfileService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _studentProfileService.GetByIdAsync(id);
            if (item == null) return NotFound();

            if (User.IsInRole("Student"))
            {
                var currentEmail = User.Identity?.Name;
                if (!string.Equals(item.User?.Email, currentEmail, StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }

            return View(item);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            await LoadStudentUsersAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(StudentProfile studentProfile)
        {
            if (!ModelState.IsValid)
            {
                await LoadStudentUsersAsync();
                return View(studentProfile);
            }

            await _studentProfileService.CreateAsync(studentProfile);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _studentProfileService.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadStudentUsersAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, StudentProfile studentProfile)
        {
            if (id != studentProfile.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadStudentUsersAsync();
                return View(studentProfile);
            }

            await _studentProfileService.UpdateAsync(studentProfile);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _studentProfileService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyProfile()
        {
            var currentEmail = User.Identity?.Name;

            var items = await _studentProfileService.GetAllAsync();
            var profile = items.FirstOrDefault(s =>
                s.User != null &&
                s.User.Email != null &&
                s.User.Email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
                return NotFound();

            return View("Details", profile);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentProfileService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadStudentUsersAsync()
        {
            var users = await _studentProfileService.GetStudentUsersAsync();
            ViewBag.IdentityUserId = new SelectList(users, "Id", "Email");
        }
    }
}
