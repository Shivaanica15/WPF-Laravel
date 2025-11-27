using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

                if (success?.Tokens == null)
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

                return LaravelAuthResult.Success(successMessage, tokens);
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
            public AuthTokensDto Tokens { get; set; }
        }

        private sealed class AuthTokensDto
        {
            public string TokenType { get; set; }
            public int ExpiresIn { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }

        private sealed class ErrorResponseDto
        {
            public string Message { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }

    public class LaravelAuthResult
    {
        private LaravelAuthResult(bool isSuccess, string message, LaravelAuthTokens tokens)
        {
            IsSuccess = isSuccess;
            Message = message;
            Tokens = tokens;
        }

        public bool IsSuccess { get; }
        public string Message { get; }
        public LaravelAuthTokens Tokens { get; }

        public static LaravelAuthResult Success(string message, LaravelAuthTokens tokens) =>
            new LaravelAuthResult(true, message, tokens);

        public static LaravelAuthResult Fail(string message) =>
            new LaravelAuthResult(false, message, null);
    }

    public class LaravelAuthTokens
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
}
