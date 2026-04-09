using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models
{
    public class FacultyProfile
    {
        public int Id { get; set; }

        public string IdentityUserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
