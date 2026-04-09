using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
        public DbSet<FacultyProfile> FacultyProfiles => Set<FacultyProfile>();
        public DbSet<CourseEnrolment> CourseEnrolments => Set<CourseEnrolment>();
        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<AssignmentResult> AssignmentResults => Set<AssignmentResult>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<ExamResult> ExamResults => Set<ExamResult>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Course>()
                .HasOne(c => c.Branch)
                .WithMany(b => b.Courses)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseEnrolment>()
                .HasOne(e => e.StudentProfile)
                .WithMany(s => s.Enrolments)
                .HasForeignKey(e => e.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseEnrolment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.CourseEnrolment)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(a => a.CourseEnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AssignmentResult>()
                .HasOne(ar => ar.Assignment)
                .WithMany(a => a.Results)
                .HasForeignKey(ar => ar.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AssignmentResult>()
                .HasOne(ar => ar.StudentProfile)
                .WithMany()
                .HasForeignKey(ar => ar.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Exam>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExamResult>()
                .HasOne(er => er.Exam)
                .WithMany(e => e.Results)
                .HasForeignKey(er => er.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExamResult>()
                .HasOne(er => er.StudentProfile)
                .WithMany()
                .HasForeignKey(er => er.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentProfile>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.IdentityUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FacultyProfile>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.IdentityUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
