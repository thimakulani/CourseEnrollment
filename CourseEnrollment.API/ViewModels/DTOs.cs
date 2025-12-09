namespace CourseEnrollment.API.ViewModels
{
    public class DTOs
    {
    }
    public class AuthDTO
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponseDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public StudentDTO Student { get; set; }
    }

    public class StudentDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; } 
        public string Email { get; set; }
    }
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
    }
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginDTO 
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
