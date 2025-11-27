using SimpleTrader.WPF.Services;
using System;
using System.Threading.Tasks;

namespace SimpleTrader.WPF.State.Authenticators
{
    public class Authenticator : IAuthenticator
    {
        private readonly LaravelAuthService _authenticationService;

        public Authenticator(LaravelAuthService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

        public event Action StateChanged;

        public async Task<LaravelAuthResult> Login(string email, string password)
        {
            LaravelAuthResult result = await _authenticationService.Login(email, password);
            UpdateTokens(result);
            return result;
        }

        public async Task<LaravelAuthResult> Register(string name, string email, string password, string confirmPassword)
        {
            LaravelAuthResult result = await _authenticationService.Register(name, email, password, confirmPassword);
            UpdateTokens(result);
            return result;
        }

        public void Logout()
        {
            bool wasLoggedIn = IsLoggedIn;
            AccessToken = null;
            RefreshToken = null;

            if (wasLoggedIn)
            {
                StateChanged?.Invoke();
            }
        }

        private void UpdateTokens(LaravelAuthResult result)
        {
            if (result.IsSuccess && result.Tokens != null)
            {
                AccessToken = result.Tokens.AccessToken;
                RefreshToken = result.Tokens.RefreshToken;
                StateChanged?.Invoke();
            }
        }
    }
}