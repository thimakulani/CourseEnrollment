using CourseEnrollment.Models;
using CourseEnrollment.ViewModels;

namespace CourseEnrollment.Interfaces
{
    public interface IUserService
    {
        Task<StudentDTO> GetUserInfo(string _accessToken);
        Task<LoginResponseDTO> Login(UserLogin login);
        Task<StudentDTO> SessionLogin();
        void LogoutAsync();
        //Task SignInWithGoogleAsync();
    }
}
