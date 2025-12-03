
using CourseEnrollment.ViewModels;

namespace CourseEnrollment.Interfaces
{
    public interface IHttpClientService
    {
        Task DeleteAsync(string endpoint);
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object payload);
        Task<T> PutAsync<T>(string endpoint, object payload);
        Task<bool> LoginAsync(UserLogin loginDTO);
    }
}
