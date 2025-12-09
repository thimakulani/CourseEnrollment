using System.ComponentModel.DataAnnotations;

namespace CourseEnrollment.Models
{
    public class Student
    {
        public Guid Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string LastName { get; set; } 
        [Required]
        public string PasswordHash { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
    }
}
