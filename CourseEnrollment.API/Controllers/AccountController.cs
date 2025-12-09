using CourseEnrollment.API.Data;
using CourseEnrollment.API.Models;
using CourseEnrollment.API.Services;
using CourseEnrollment.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace CourseEnrollment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAuthService authService;
        private ILogger<AccountController> _logger;
        private readonly IRepository<Student> _studentRepository;
        public AccountController(IAuthService authService, ILogger<AccountController> logger, AppDbContext context, IRepository<Student> studentRepository)
        {
            this.authService = authService;
            this._logger = logger;
            _studentRepository = studentRepository;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO auth)
        {
            try
            {
                var result = await authService.LoginAsync(auth);

                if (!result.Success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Error
                    });
                }

                var response = result.Data;

                return Ok(new ApiResponse<LoginResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Data = ex.Message,
                    Message = ex.ToString(),
                    Success = false
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthDTO auth)
        {
            try
            {
                var result = await authService.RegisterAsync(auth);

                if (!result.Success)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Error
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Data = ex.Message,
                    Message = ex.ToString(),
                    Success = false
                });
            }
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenPair request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var result = await authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                if (!result.Success)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Error
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        accessToken = result.Tokens.AccessToken,
                        refreshToken = result.Tokens.RefreshToken,
                        expiresAt = result.Tokens.ExpiresAtUtc
                    },
                    Message = "Token refreshed successfully"
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
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            try
            {
                var student = await _studentRepository
                    .GetFirstOrDefaultAsync(x => x.Id == id);

                if (student == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                await _studentRepository.DeleteAsync(student.Id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Profile deleted successfully"
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
        [Authorize]
        [HttpPut("update_student/{id}")]
        public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] Student model)
        {
            try
            {
                var student = await _studentRepository
                    .GetFirstOrDefaultAsync(x => x.Id == id);

                if (student == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                student.Name = model.Name;
                student.LastName = model.LastName;
                student.Email = model.Email;

                await _studentRepository.UpdateAsync(student);

                return Ok(new ApiResponse<Student>
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    Data = student
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
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized"
                    });
                }

                var student = await _studentRepository
                    .GetFirstOrDefaultAsync(x => x.Id.ToString() == userId);

                if (student == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                return Ok(new ApiResponse<Student>
                {
                    Success = true,
                    Message = "Student profile retrieved",
                    Data = student
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
