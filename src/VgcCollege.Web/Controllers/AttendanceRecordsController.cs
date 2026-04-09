using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class AttendanceRecordsController : Controller
    {
        private readonly AttendanceRecordService _attendanceRecordService;

        public AttendanceRecordsController(AttendanceRecordService attendanceRecordService)
        {
            _attendanceRecordService = attendanceRecordService;
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Index()
        {
            var items = await _attendanceRecordService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _attendanceRecordService.GetByIdAsync(id);
            if (item == null) return NotFound();

            if (User.IsInRole("Student"))
            {
                var currentEmail = User.Identity?.Name;
                if (!string.Equals(item.CourseEnrolment?.StudentProfile?.User?.Email, currentEmail, StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }

            return View(item);
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyAttendance()
        {
            var currentEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(currentEmail))
                return Forbid();

            var items = await _attendanceRecordService.GetByStudentEmailAsync(currentEmail);
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Create()
        {
            await LoadEnrolmentsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Create(AttendanceRecord attendanceRecord)
        {
            if (!ModelState.IsValid)
            {
                await LoadEnrolmentsAsync();
                return View(attendanceRecord);
            }

            try
            {
                await _attendanceRecordService.CreateAsync(attendanceRecord);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadEnrolmentsAsync();
                return View(attendanceRecord);
            }
        }

        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _attendanceRecordService.GetByIdAsync(id);
            if (item == null) return NotFound();

            await LoadEnrolmentsAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Faculty")]
        public async Task<IActionResult> Edit(int id, AttendanceRecord attendanceRecord)
        {
            if (id != attendanceRecord.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadEnrolmentsAsync();
                return View(attendanceRecord);
            }

            try
            {
                await _attendanceRecordService.UpdateAsync(attendanceRecord);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadEnrolmentsAsync();
                return View(attendanceRecord);
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _attendanceRecordService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _attendanceRecordService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadEnrolmentsAsync()
        {
            var enrolments = await _attendanceRecordService.GetEnrolmentsAsync();

            ViewBag.CourseEnrolmentId = new SelectList(
                enrolments.Select(e => new
                {
                    e.Id,
                    Description = $"{e.StudentProfile!.Name} - {e.Course!.Name} ({e.Course.Branch!.Name})"
                }),
                "Id",
                "Description");
        }
    }
}
