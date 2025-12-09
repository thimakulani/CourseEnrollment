using CourseEnrollment.API.Data;
using CourseEnrollment.API.Models;
using CourseEnrollment.API.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CourseEnrollment.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<Student> _passwordHasher = new();
        private readonly IJwtTokenService _jwt;

        public AuthService(AppDbContext db, IJwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        
        public async Task<(bool Success, string Error)> RegisterAsync(AuthDTO request)
        {
            var existing = await _db.Students
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (existing != null)
                return (false, "Email already exists.");

            var student = new Student
            {
                Email = request.Email,
                Name = request.Name,
                LastName = request.LastName,
            };

            student.PasswordHash = _passwordHasher.HashPassword(student, request.Password);

            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            return (true, null);
        }


        public async Task<(bool Success, LoginResponseDTO Data, string Error)> LoginAsync(LoginDTO request)
        {
            var student = await _db.Students
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (student == null)
                return (false, null, "Invalid email or password.");

            var result = _passwordHasher.VerifyHashedPassword(
                student, student.PasswordHash, request.Password
            );

            if (result == PasswordVerificationResult.Failed)
                return (false, null, "Invalid email or password.");

            var tokenPair = await _jwt.CreateTokenPairAsync(student);

            var studentDto = new StudentDTO
            {
                Id = student.Id,
                Name = student.Name,
                Surname = student.LastName,
                Email = student.Email
            };

            var loginResponse = new LoginResponseDTO
            {
                AccessToken = tokenPair.AccessToken,
                RefreshToken = tokenPair.RefreshToken,
                Student = studentDto
            };

            return (true, loginResponse, null);
        }

        public async Task<(bool Success, TokenPair Tokens, string Error)> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var tokens = await _jwt.RefreshAsync(refreshToken, ipAddress);
                return (true, tokens, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }

    public interface IAuthService
    {
        Task<(bool Success, string Error)> RegisterAsync(AuthDTO request);
        Task<(bool Success, LoginResponseDTO Data, string Error)> LoginAsync(LoginDTO request);

        Task<(bool Success, TokenPair Tokens, string Error)> RefreshTokenAsync(string refreshToken, string ipAddress);
    }
}
