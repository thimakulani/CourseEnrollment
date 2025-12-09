using Blazored.LocalStorage;
using CourseEnrollment.Interfaces;
using CourseEnrollment.ViewModels;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace CourseEnrollment.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient httpClient;
        private readonly ILocalStorageService _sessionStorage;
        public UserService(IHttpClientFactory _httpClient, ILocalStorageService sessionStorage)
        {
            httpClient = _httpClient.CreateClient("api");
            _sessionStorage = sessionStorage;
        }
        public async Task<LoginResponseDTO> Login(UserLogin login)
        {
            var json = JsonConvert.SerializeObject(login);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/account/login", content);
            if (response.IsSuccessStatusCode)
            {
                var str_data = await response.Content.ReadAsStringAsync();

                var tkn = JsonConvert.DeserializeObject<ApiResponse<LoginResponseDTO>>(str_data);
                await _sessionStorage.SetItemAsync("accessToken", tkn.Data.AccessToken);
                await _sessionStorage.SetItemAsync("refreshToken", tkn.Data.RefreshToken);
                await _sessionStorage.SetItemAsync("username", login.Email);
                await _sessionStorage.SetItemAsync("password", login.Password);
                return tkn.Data;
            }
            else
            {
                throw new HttpRequestException($"Invalid Email or Password");
            }
        }
        public async Task<StudentDTO> GetUserInfo(string _accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await httpClient.GetAsync("api/account/profile");
            if (response.IsSuccessStatusCode)
            {
                string str = await response.Content.ReadAsStringAsync();
                var vm = JsonConvert.DeserializeObject<ApiResponse<StudentDTO>>(str);

                return vm.Data;
            }
            else
            {
                string str = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{response.StatusCode} {str}");
            }
        }

        public async Task<StudentDTO> SessionLogin()
        {
            try
            {
                var username = await _sessionStorage.GetItemAsync<string>("username");
                var password = await _sessionStorage.GetItemAsync<string>("password");
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    UserLogin userLogin = new()
                    {
                        Email = username,
                        Password = password
                    };
                    var results = await Login(userLogin);
                    Console.WriteLine("AccessToken " + results.AccessToken);
                    return await GetUserInfo(results.AccessToken);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async void LogoutAsync()
        {
            await _sessionStorage.RemoveItemAsync("Username");
            await _sessionStorage.RemoveItemAsync("Password");
        }
    }
}
