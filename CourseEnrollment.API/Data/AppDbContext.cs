using CourseEnrollment.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseEnrollment.API.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<RefreshTokenRecord> RefreshTokenRecords { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

    }
}