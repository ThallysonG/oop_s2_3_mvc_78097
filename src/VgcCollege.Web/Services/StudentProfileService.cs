using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class StudentProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentProfileService> _logger;

        public StudentProfileService(ApplicationDbContext context, ILogger<StudentProfileService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<StudentProfile>> GetAllAsync()
        {
            return await _context.StudentProfiles
                .Include(s => s.User)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<StudentProfile?> GetByIdAsync(int id)
        {
            return await _context.StudentProfiles
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<ApplicationUser>> GetStudentUsersAsync()
        {
            var studentUserIds = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == "Student"
                select user
            ).ToListAsync();

            return studentUserIds;
        }

        public async Task CreateAsync(StudentProfile studentProfile)
        {
            _context.StudentProfiles.Add(studentProfile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student profile created. StudentProfileId: {StudentProfileId}, Name: {Name}",
                studentProfile.Id, studentProfile.Name);
        }

        public async Task UpdateAsync(StudentProfile studentProfile)
        {
            _context.StudentProfiles.Update(studentProfile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student profile updated. StudentProfileId: {StudentProfileId}, Name: {Name}",
                studentProfile.Id, studentProfile.Name);
        }

        public async Task DeleteAsync(int id)
        {
            var studentProfile = await _context.StudentProfiles.FindAsync(id);
            if (studentProfile == null)
            {
                _logger.LogWarning("Attempt to delete non-existing StudentProfile. StudentProfileId: {StudentProfileId}", id);
                return;
            }

            _context.StudentProfiles.Remove(studentProfile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student profile deleted. StudentProfileId: {StudentProfileId}", id);
        }
    }
}
