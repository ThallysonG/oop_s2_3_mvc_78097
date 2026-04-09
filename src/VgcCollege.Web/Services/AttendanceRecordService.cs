using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class AttendanceRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceRecordService> _logger;

        public AttendanceRecordService(ApplicationDbContext context, ILogger<AttendanceRecordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AttendanceRecord>> GetAllAsync()
        {
            return await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e!.StudentProfile)
                        .ThenInclude(s => s!.User)
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e!.Course)
                .OrderBy(a => a.CourseEnrolment!.StudentProfile!.Name)
                .ThenBy(a => a.WeekNumber)
                .ToListAsync();
        }

        public async Task<AttendanceRecord?> GetByIdAsync(int id)
        {
            return await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e!.StudentProfile)
                        .ThenInclude(s => s!.User)
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e!.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
        }


        public async Task<List<CourseEnrolment>> GetEnrolmentsAsync()
        {
            return await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Branch)
                .OrderBy(e => e.StudentProfile!.Name)
                .ToListAsync();
        }

        public async Task CreateAsync(AttendanceRecord attendanceRecord)
        {
            var duplicateExists = await _context.AttendanceRecords.AnyAsync(a =>
                a.CourseEnrolmentId == attendanceRecord.CourseEnrolmentId &&
                a.WeekNumber == attendanceRecord.WeekNumber);

            if (duplicateExists)
                throw new Exception("An attendance record already exists for this enrolment in the selected week.");

            _context.AttendanceRecords.Add(attendanceRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Attendance record created. AttendanceRecordId: {AttendanceRecordId}, CourseEnrolmentId: {CourseEnrolmentId}, WeekNumber: {WeekNumber}, Present: {Present}",
                attendanceRecord.Id, attendanceRecord.CourseEnrolmentId, attendanceRecord.WeekNumber, attendanceRecord.Present);
        }

        public async Task UpdateAsync(AttendanceRecord attendanceRecord)
        {
            var duplicateExists = await _context.AttendanceRecords.AnyAsync(a =>
                a.Id != attendanceRecord.Id &&
                a.CourseEnrolmentId == attendanceRecord.CourseEnrolmentId &&
                a.WeekNumber == attendanceRecord.WeekNumber);

            if (duplicateExists)
                throw new Exception("An attendance record already exists for this enrolment in the selected week.");

            _context.AttendanceRecords.Update(attendanceRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Attendance record updated. AttendanceRecordId: {AttendanceRecordId}, WeekNumber: {WeekNumber}, Present: {Present}",
                attendanceRecord.Id, attendanceRecord.WeekNumber, attendanceRecord.Present);
        }

        public async Task DeleteAsync(int id)
        {
            var attendanceRecord = await _context.AttendanceRecords.FindAsync(id);
            if (attendanceRecord == null)
            {
                _logger.LogWarning("Attempt to delete non-existing attendance record. AttendanceRecordId: {AttendanceRecordId}", id);
                return;
            }

            _context.AttendanceRecords.Remove(attendanceRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attendance record deleted. AttendanceRecordId: {AttendanceRecordId}", id);
        }
        public async Task<List<AttendanceRecord>> GetByStudentEmailAsync(string email)
        {
            return await _context.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e!.StudentProfile)
                        .ThenInclude(s => s!.User)
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e!.Course)
                        .ThenInclude(c => c!.Branch)
                .Where(a => a.CourseEnrolment != null &&
                            a.CourseEnrolment.StudentProfile != null &&
                            a.CourseEnrolment.StudentProfile.User != null &&
                            a.CourseEnrolment.StudentProfile.User.Email == email)
                .OrderBy(a => a.CourseEnrolment!.Course!.Name)
                .ThenBy(a => a.WeekNumber)
                .ToListAsync();
        }

    }
}
