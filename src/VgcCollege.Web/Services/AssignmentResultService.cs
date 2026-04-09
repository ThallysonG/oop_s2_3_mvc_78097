using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class AssignmentResultService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssignmentResultService> _logger;

        public AssignmentResultService(ApplicationDbContext context, ILogger<AssignmentResultService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AssignmentResult>> GetAllAsync()
        {
            return await _context.AssignmentResults
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Course)
                .Include(r => r.StudentProfile)
                    .ThenInclude(s => s!.User)
                .OrderBy(r => r.StudentProfile!.Name)
                .ThenBy(r => r.Assignment!.Title)
                .ToListAsync();
        }

        public async Task<List<AssignmentResult>> GetByStudentEmailAsync(string email)
        {
            return await _context.AssignmentResults
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Course)
                        .ThenInclude(c => c!.Branch)
                .Include(r => r.StudentProfile)
                    .ThenInclude(s => s!.User)
                .Where(r => r.StudentProfile != null &&
                            r.StudentProfile.User != null &&
                            r.StudentProfile.User.Email == email)
                .OrderBy(r => r.Assignment!.Course!.Name)
                .ThenBy(r => r.Assignment!.Title)
                .ToListAsync();
        }

        public async Task<AssignmentResult?> GetByIdAsync(int id)
        {
            return await _context.AssignmentResults
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Course)
                        .ThenInclude(c => c!.Branch)
                .Include(r => r.StudentProfile)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Assignment>> GetAssignmentsAsync()
        {
            return await _context.Assignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Branch)
                .OrderBy(a => a.Title)
                .ToListAsync();
        }

        public async Task<List<StudentProfile>> GetStudentsAsync()
        {
            return await _context.StudentProfiles
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task CreateAsync(AssignmentResult assignmentResult)
        {
            var duplicateExists = await _context.AssignmentResults.AnyAsync(r =>
                r.AssignmentId == assignmentResult.AssignmentId &&
                r.StudentProfileId == assignmentResult.StudentProfileId);

            if (duplicateExists)
                throw new Exception("A result for this student and assignment already exists.");

            _context.AssignmentResults.Add(assignmentResult);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment result created. AssignmentResultId: {AssignmentResultId}", assignmentResult.Id);
        }

        public async Task UpdateAsync(AssignmentResult assignmentResult)
        {
            var duplicateExists = await _context.AssignmentResults.AnyAsync(r =>
                r.Id != assignmentResult.Id &&
                r.AssignmentId == assignmentResult.AssignmentId &&
                r.StudentProfileId == assignmentResult.StudentProfileId);

            if (duplicateExists)
                throw new Exception("A result for this student and assignment already exists.");

            _context.AssignmentResults.Update(assignmentResult);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment result updated. AssignmentResultId: {AssignmentResultId}", assignmentResult.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var assignmentResult = await _context.AssignmentResults.FindAsync(id);
            if (assignmentResult == null)
            {
                _logger.LogWarning("Attempt to delete non-existing AssignmentResult. AssignmentResultId: {AssignmentResultId}", id);
                return;
            }

            _context.AssignmentResults.Remove(assignmentResult);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment result deleted. AssignmentResultId: {AssignmentResultId}", id);
        }
    }
}
