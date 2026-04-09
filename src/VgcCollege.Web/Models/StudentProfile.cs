using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }

        public string IdentityUserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DOB { get; set; }

        public List<CourseEnrolment> Enrolments { get; set; } = new();
    }
}
