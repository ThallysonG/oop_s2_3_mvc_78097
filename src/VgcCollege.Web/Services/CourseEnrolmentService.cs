using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class CourseEnrolmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseEnrolmentService> _logger;

        public CourseEnrolmentService(ApplicationDbContext context, ILogger<CourseEnrolmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CourseEnrolment>> GetAllAsync()
        {
            return await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                    .ThenInclude(s => s!.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Branch)
                .OrderBy(e => e.StudentProfile!.Name)
                .ThenBy(e => e.Course!.Name)
                .ToListAsync();
        }

        public async Task<CourseEnrolment?> GetByIdAsync(int id)
        {
            return await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                    .ThenInclude(s => s!.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Branch)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<CourseEnrolment>> GetByStudentEmailAsync(string email)
        {
            return await _context.CourseEnrolments
                .Include(e => e.StudentProfile)
                    .ThenInclude(s => s!.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Branch)
                .Where(e => e.StudentProfile != null &&
                            e.StudentProfile.User != null &&
                            e.StudentProfile.User.Email == email)
                .OrderBy(e => e.Course!.Name)
                .ToListAsync();
        }

        public async Task<List<StudentProfile>> GetStudentsAsync()
        {
            return await _context.StudentProfiles
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<List<Course>> GetCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Branch)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task CreateAsync(CourseEnrolment enrolment)
        {
            var course = await _context.Courses.FindAsync(enrolment.CourseId);
            if (course == null)
                throw new Exception("Selected course not found.");

            var sameCourseExists = await _context.CourseEnrolments.AnyAsync(e =>
                e.StudentProfileId == enrolment.StudentProfileId &&
                e.CourseId == enrolment.CourseId);

            if (sameCourseExists)
                throw new Exception("This student is already enrolled in this course.");

            var overlappingEnrolmentExists = await _context.CourseEnrolments
                .Include(e => e.Course)
                .AnyAsync(e =>
                    e.StudentProfileId == enrolment.StudentProfileId &&
                    e.Status == "Active" &&
                    e.Course != null &&
                    course.StartDate <= e.Course.EndDate &&
                    course.EndDate >= e.Course.StartDate);

            if (overlappingEnrolmentExists)
                throw new Exception("This student already has an active enrolment in another course during the same period.");

            _context.CourseEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Course enrolment created. EnrolmentId: {EnrolmentId}, StudentProfileId: {StudentProfileId}, CourseId: {CourseId}",
                enrolment.Id, enrolment.StudentProfileId, enrolment.CourseId);
        }

        public async Task UpdateAsync(CourseEnrolment enrolment)
        {
            var course = await _context.Courses.FindAsync(enrolment.CourseId);
            if (course == null)
                throw new Exception("Selected course not found.");

            var sameCourseExists = await _context.CourseEnrolments.AnyAsync(e =>
                e.Id != enrolment.Id &&
                e.StudentProfileId == enrolment.StudentProfileId &&
                e.CourseId == enrolment.CourseId);

            if (sameCourseExists)
                throw new Exception("This student is already enrolled in this course.");

            var overlappingEnrolmentExists = await _context.CourseEnrolments
                .Include(e => e.Course)
                .AnyAsync(e =>
                    e.Id != enrolment.Id &&
                    e.StudentProfileId == enrolment.StudentProfileId &&
                    e.Status == "Active" &&
                    e.Course != null &&
                    course.StartDate <= e.Course.EndDate &&
                    course.EndDate >= e.Course.StartDate);

            if (overlappingEnrolmentExists)
                throw new Exception("This student already has an active enrolment in another course during the same period.");

            _context.CourseEnrolments.Update(enrolment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Course enrolment updated. EnrolmentId: {EnrolmentId}", enrolment.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var enrolment = await _context.CourseEnrolments.FindAsync(id);
            if (enrolment == null)
            {
                _logger.LogWarning("Attempt to delete non-existing enrolment. EnrolmentId: {EnrolmentId}", id);
                return;
            }

            _context.CourseEnrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Course enrolment deleted. EnrolmentId: {EnrolmentId}", id);
        }
    }
}
