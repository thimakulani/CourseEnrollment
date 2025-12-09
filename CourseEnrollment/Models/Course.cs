using System.ComponentModel.DataAnnotations;

namespace CourseEnrollment.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public int Duration { get; internal set; }
    }
}
