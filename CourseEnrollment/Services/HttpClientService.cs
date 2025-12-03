using Blazored.LocalStorage;
using CourseEnrollment.Interfaces;
using CourseEnrollment.ViewModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CourseEnrollment.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private string _refreshToken;
        private readonly ILocalStorageService _sessionStorage;


        private const string AccessTokenKey = "accessToken";
        private const string RefreshTokenKey = "refreshToken";
        private const string UsernameKey = "username";
        private const string PasswordKey = "password";

        public HttpClientService(IHttpClientFactory clientFactory, ILocalStorageService sessionStorage)
        {
            _httpClient = clientFactory.CreateClient("api");
            _sessionStorage = sessionStorage;
        }

        private async Task<T> SendRequestWithRetry<T>(Func<Task<HttpResponseMessage>> requestFunc)
        {
            _accessToken = await _sessionStorage.GetItemAsync<string>(AccessTokenKey);
            _refreshToken = await _sessionStorage.GetItemAsync<string>(RefreshTokenKey);

            SetAuthorizationHeader(_accessToken);

            HttpResponseMessage response = await requestFunc();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && await RefreshAccessTokenAsync())
            {
                SetAuthorizationHeader(_accessToken);
                response = await requestFunc();
            }

            return await HandleResponse<T>(response);
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        private async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_refreshToken)) return false;

                var payload = new { RefreshToken = _refreshToken };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/auth/refresh", content);

                if (response.IsSuccessStatusCode)
                {
                    var token = await DeserializeToken(response);
                    if (token != null)
                    {
                        _accessToken = token.AccessToken;
                        _refreshToken = token.RefreshToken;

                        await _sessionStorage.SetItemAsync(AccessTokenKey, _accessToken);
                        await _sessionStorage.SetItemAsync(RefreshTokenKey, _refreshToken);

                        return true;
                    }
                }
                else
                {
                    return await Reauthentication();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token refresh failed: {ex.Message}");
            }

            return false;
        }

        private async Task<bool> Reauthentication()
        {
            string username = await _sessionStorage.GetItemAsync<string>(UsernameKey);
            string password = await _sessionStorage.GetItemAsync<string>(PasswordKey);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            var login = new UserLogin { Email = username, Password = password };
            return await LoginAsync(login);
        }

        public async Task<bool> LoginAsync(UserLogin payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var token = await DeserializeToken(response);

                if (token != null)
                {
                    await _sessionStorage.SetItemAsync(UsernameKey, payload.Email);
                    await _sessionStorage.SetItemAsync(PasswordKey, payload.Password);
                    await _sessionStorage.SetItemAsync(AccessTokenKey, token.AccessToken);
                    await _sessionStorage.SetItemAsync(RefreshTokenKey, token.RefreshToken);

                    return true;
                }
            }

            throw new HttpRequestException("Invalid login details.");
        }

        private static async Task<Token> DeserializeToken(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Token>(json);
        }

        private static async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(json);

                if (apiResponse == null || !apiResponse.Success)
                {
                    throw new HttpRequestException(apiResponse?.Message ?? "Unknown error occurred.");
                }
                return apiResponse.Data!;
            }

            try
            {
                var apiError = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                throw new HttpRequestException(apiError?.Message ?? json);
            }
            catch
            {
                throw new HttpRequestException(json);
            }
        }

        public async Task DeleteAsync(string endpoint)
        {
            await SendRequestWithRetry<object>(() => _httpClient.DeleteAsync(endpoint));
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            return await SendRequestWithRetry<T>(() => _httpClient.GetAsync(endpoint));
        }

        public async Task<T> PostAsync<T>(string endpoint, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await SendRequestWithRetry<T>(() => _httpClient.PostAsync(endpoint, content));
        }

        public async Task<T> PutAsync<T>(string endpoint, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await SendRequestWithRetry<T>(() => _httpClient.PutAsync(endpoint, content));
        }
    }





    public class Token
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
