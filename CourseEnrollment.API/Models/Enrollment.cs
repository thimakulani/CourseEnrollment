using System.ComponentModel.DataAnnotations;

namespace CourseEnrollment.API.Models
{
    public class Enrollment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
    }
}