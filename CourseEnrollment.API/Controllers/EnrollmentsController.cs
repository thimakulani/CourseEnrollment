using CourseEnrollment.API.Data;
using CourseEnrollment.API.Models;
using CourseEnrollment.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CourseEnrollment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EnrollmentsController(AppDbContext db)
        {
            _db = db;
        }

        private Guid? GetStudentId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromQuery] Guid courseId)
        {
            try
            {
                var studentId = GetStudentId();
                if (studentId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized student."
                    });
                }

                bool exists = await _db.StudentCourses.AnyAsync(x =>
                    x.StudentId == studentId && x.CourseId == courseId);

                if (exists)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student already enrolled in this course."
                    });
                }

                var enrollment = new StudentCourse
                {
                    StudentId = studentId.Value,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow
                };

                _db.StudentCourses.Add(enrollment);
                await _db.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Enrollment successful."
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An unexpected error occurred."
                });
            }
        }

        [HttpDelete("deregister")]
        public async Task<IActionResult> Deregister([FromQuery] Guid courseId)
        {
            try
            {
                var studentId = GetStudentId();
                if (studentId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized student."
                    });
                }

                var enrollment = await _db.StudentCourses.FirstOrDefaultAsync(x =>
                    x.StudentId == studentId && x.CourseId == courseId);

                if (enrollment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Enrollment not found."
                    });
                }

                _db.StudentCourses.Remove(enrollment);
                await _db.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Successfully deregistered."
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Data = ex.Message
                });
            }
        }

        [HttpGet("student/me")]
        public async Task<IActionResult> GetMyCourses()
        {
            try
            {
                var studentId = GetStudentId();
                if (studentId == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized student."
                    });
                }

                var courses = await _db.StudentCourses
                    .Where(x => x.StudentId == studentId)
                    .Include(x => x.Course)
                    .Select(x => x.Course)
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = courses,
                    Message = "Courses retrieved."
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Data = ex.Message
                    
                });
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetStudentsInCourse(Guid courseId)
        {
            try
            {
                var students = await _db.StudentCourses
                    .Where(x => x.CourseId == courseId)
                    .Include(x => x.Student)
                    .Select(x => x.Student)
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = students,
                    Message = "Course students retrieved."
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Data = ex.Message
                });
            }
        }
    }
}
