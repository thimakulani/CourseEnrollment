using System.ComponentModel.DataAnnotations;

namespace CourseEnrollment.API.Models
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public int Duration { get; internal set; }
    }
}
