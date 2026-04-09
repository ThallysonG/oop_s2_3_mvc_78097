using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<CourseEnrolment> Enrolments { get; set; } = new();
    }
}
