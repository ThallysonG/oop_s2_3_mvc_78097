using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class ExamResultService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExamResultService> _logger;

        public ExamResultService(ApplicationDbContext context, ILogger<ExamResultService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ExamResult>> GetAllAsync()
        {
            return await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e!.Course)
                        .ThenInclude(c => c!.Branch)
                .Include(r => r.StudentProfile)
                    .ThenInclude(s => s!.User)
                .OrderBy(r => r.StudentProfile!.Name)
                .ThenBy(r => r.Exam!.Title)
                .ToListAsync();
        }

        public async Task<List<ExamResult>> GetVisibleByStudentEmailAsync(string email)
        {
            return await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e!.Course)
                        .ThenInclude(c => c!.Branch)
                .Include(r => r.StudentProfile)
                    .ThenInclude(s => s!.User)
                .Where(r => r.StudentProfile != null &&
                            r.StudentProfile.User != null &&
                            r.StudentProfile.User.Email == email &&
                            r.Exam != null &&
                            r.Exam.ResultsReleased)
                .OrderBy(r => r.Exam!.Course!.Name)
                .ThenBy(r => r.Exam!.Title)
                .ToListAsync();
        }

        public async Task<ExamResult?> GetByIdAsync(int id)
        {
            return await _context.ExamResults
                .Include(r => r.Exam)
                    .ThenInclude(e => e!.Course)
                        .ThenInclude(c => c!.Branch)
                .Include(r => r.StudentProfile)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Exam>> GetExamsAsync()
        {
            return await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Branch)
                .OrderBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<List<StudentProfile>> GetStudentsAsync()
        {
            return await _context.StudentProfiles
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task CreateAsync(ExamResult examResult)
        {
            var duplicateExists = await _context.ExamResults.AnyAsync(r =>
                r.ExamId == examResult.ExamId &&
                r.StudentProfileId == examResult.StudentProfileId);

            if (duplicateExists)
                throw new Exception("An exam result for this student already exists.");

            _context.ExamResults.Add(examResult);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exam result created. ExamResultId: {ExamResultId}", examResult.Id);
        }

        public async Task UpdateAsync(ExamResult examResult)
        {
            var duplicateExists = await _context.ExamResults.AnyAsync(r =>
                r.Id != examResult.Id &&
                r.ExamId == examResult.ExamId &&
                r.StudentProfileId == examResult.StudentProfileId);

            if (duplicateExists)
                throw new Exception("An exam result for this student already exists.");

            _context.ExamResults.Update(examResult);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exam result updated. ExamResultId: {ExamResultId}", examResult.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var examResult = await _context.ExamResults.FindAsync(id);
            if (examResult == null)
            {
                _logger.LogWarning("Attempt to delete non-existing ExamResult. ExamResultId: {ExamResultId}", id);
                return;
            }

            _context.ExamResults.Remove(examResult);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exam result deleted. ExamResultId: {ExamResultId}", id);
        }
    }
}
