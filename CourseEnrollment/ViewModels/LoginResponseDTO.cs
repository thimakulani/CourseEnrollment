namespace CourseEnrollment.ViewModels
{
  
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
}
