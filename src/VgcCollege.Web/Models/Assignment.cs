using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        [Required]
        public string Title { get; set; } = "";

        public int MaxScore { get; set; }
        public DateTime DueDate { get; set; }

        public List<AssignmentResult> Results { get; set; } = new();
    }
}
