using CourseEnrollment.API.Models;
using CourseEnrollment.API.Services;
using CourseEnrollment.API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CourseEnrollment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IRepository<Course> repository;

        public CoursesController(IRepository<Course> repository)
        {
            this.repository = repository;
        }
        //add
        [HttpPost]
        public async Task<IActionResult> AddCourse([FromBody] Course course)
        {
            try
            {
                var result = await repository.AddAsync(course);

                return Ok(new ApiResponse<object>
                {
                    Data = result,
                    Message = "Course successfully created",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
        //get all
        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            try
            {
                var courses = await repository.GetListAsync();

                return Ok(new ApiResponse<object>
                {
                    Data = courses,
                    Message = "Courses retrieved successfully",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        //get by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            try
            {
                var course = await repository.GetByIdAsync(id);

                if (course == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Course not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Data = course,
                    Message = "Course retrieved successfully",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        //update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] Course course)
        {
            try
            {
                var existing = await repository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Course not found"
                    });
                }

                // Map updated values
                existing.Title = course.Title;
                existing.Description = course.Description;
                existing.Duration = course.Duration;

                 await repository.UpdateAsync(existing);

                return Ok(new ApiResponse<object>
                {
                    Data = existing,
                    Message = "Course successfully updated",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        //DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            try
            {
                var existing = await repository.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Course not found"
                    });
                }

                await repository.DeleteAsync(id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Course successfully deleted"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
