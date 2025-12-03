using Blazored.LocalStorage;
using CourseEnrollment.Interfaces;

namespace NewLife.Web.Services
{
    public class SessionService : ISessionService
    {
        private readonly ILocalStorageService _sessionStorage;
        public SessionService(ILocalStorageService sessionStorage)
        {
            this._sessionStorage = sessionStorage;
        }

        public async void Clear(string key)
        {
            await _sessionStorage.RemoveItemAsync(key);
        }

        public async Task<string> Get(string key)
        {
            return await _sessionStorage.GetItemAsync<string>(key);
        }

        public async Task Set(string key, string value)
        {
            await _sessionStorage.SetItemAsync(key, value);
        }
    }
}
