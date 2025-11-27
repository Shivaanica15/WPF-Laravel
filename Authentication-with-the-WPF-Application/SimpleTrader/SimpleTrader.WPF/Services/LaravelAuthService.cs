using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimpleTrader.WPF.Services
{
    public class LaravelAuthService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public LaravelAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<LaravelAuthResult> Login(string email, string password)
        {
            var payload = new
            {
                email,
                password
            };

            return PostAsync("login", payload, "Login successful.");
        }

        public Task<LaravelAuthResult> Register(string name, string email, string password, string confirmPassword)
        {
            var payload = new
            {
                name,
                email,
                password,
                password_confirmation = confirmPassword
            };

            return PostAsync("register", payload, "Registration successful.");
        }

        private async Task<LaravelAuthResult> PostAsync(string relativeUrl, object payload, string successMessage)
        {
            string json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(relativeUrl, content);
            }
            catch (HttpRequestException ex)
            {
                return LaravelAuthResult.Fail($"Unable to reach authentication server. {ex.Message}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                AuthSuccessDto success = JsonSerializer.Deserialize<AuthSuccessDto>(responseBody, SerializerOptions);

                if (success?.Tokens == null || string.IsNullOrWhiteSpace(success.Tokens.AccessToken))
                {
                    return LaravelAuthResult.Fail("Authentication service returned an unexpected response.");
                }

                var tokens = new LaravelAuthTokens
                {
                    AccessToken = success.Tokens.AccessToken,
                    RefreshToken = success.Tokens.RefreshToken,
                    TokenType = success.Tokens.TokenType,
                    ExpiresIn = success.Tokens.ExpiresIn
                };

                var user = success.User == null
                    ? null
                    : new LaravelAuthUser
                    {
                        Id = success.User.Id,
                        Name = success.User.Name,
                        Email = success.User.Email
                    };

                return LaravelAuthResult.Success(successMessage, tokens, user);
            }

            string errorMessage = TryParseErrors(responseBody) ??
                                  $"Request failed with status code {(int)response.StatusCode}.";

            return LaravelAuthResult.Fail(errorMessage);
        }

        private static string TryParseErrors(string content)
        {
            try
            {
                ErrorResponseDto error = JsonSerializer.Deserialize<ErrorResponseDto>(content, SerializerOptions);
                if (error == null)
                {
                    return null;
                }

                var messages = new List<string>();

                if (!string.IsNullOrWhiteSpace(error.Message))
                {
                    messages.Add(error.Message);
                }

                if (error.Errors != null)
                {
                    foreach (KeyValuePair<string, string[]> entry in error.Errors)
                    {
                        if (entry.Value == null)
                        {
                            continue;
                        }

                        foreach (string message in entry.Value)
                        {
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                messages.Add(message);
                            }
                        }
                    }
                }

                return messages.Count > 0 ? string.Join(Environment.NewLine, messages) : null;
            }
            catch
            {
                return null;
            }
        }

        private sealed class AuthSuccessDto
        {
            [JsonPropertyName("user")]
            public UserDto User { get; set; }

            [JsonPropertyName("tokens")]
            public AuthTokensDto Tokens { get; set; }
        }

        private sealed class AuthTokensDto
        {
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }
        }

        private sealed class UserDto
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }
        }

        private sealed class ErrorResponseDto
        {
            public string Message { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }

    public class LaravelAuthResult
    {
        private LaravelAuthResult(bool isSuccess, string message, LaravelAuthTokens tokens, LaravelAuthUser user)
        {
            IsSuccess = isSuccess;
            Message = message;
            Tokens = tokens;
            User = user;
        }

        public bool IsSuccess { get; }
        public string Message { get; }
        public LaravelAuthTokens Tokens { get; }
        public LaravelAuthUser User { get; }

        public static LaravelAuthResult Success(string message, LaravelAuthTokens tokens, LaravelAuthUser user) =>
            new LaravelAuthResult(true, message, tokens, user);

        public static LaravelAuthResult Fail(string message) =>
            new LaravelAuthResult(false, message, null, null);
    }

    public class LaravelAuthTokens
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }

    public class LaravelAuthUser
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}



