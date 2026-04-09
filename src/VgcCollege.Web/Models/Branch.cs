using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models
{
    public class Branch
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string? Address { get; set; }

        public List<Course> Courses { get; set; } = new();
    }
}
