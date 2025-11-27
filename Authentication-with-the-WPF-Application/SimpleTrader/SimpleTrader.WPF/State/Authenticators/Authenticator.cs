using SimpleTrader.WPF.Services;
using System;
using System.Threading.Tasks;

namespace SimpleTrader.WPF.State.Authenticators
{
    public class Authenticator : IAuthenticator
    {
        private readonly LaravelAuthService _authenticationService;
        private readonly TokenStore _tokenStore;

        public Authenticator(LaravelAuthService authenticationService, TokenStore tokenStore)
        {
            _authenticationService = authenticationService;
            _tokenStore = tokenStore;
        }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public LaravelAuthUser CurrentUser { get; private set; }
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
            CurrentUser = null;
            _tokenStore.DeleteSession();

            if (wasLoggedIn)
            {
                StateChanged?.Invoke();
            }
        }

        public void RestoreSession(PersistedAuthSession session)
        {
            if (session == null || string.IsNullOrWhiteSpace(session.AccessToken))
            {
                return;
            }

            AccessToken = session.AccessToken;
            RefreshToken = session.RefreshToken;
            CurrentUser = session.UserId.HasValue ||
                          !string.IsNullOrWhiteSpace(session.UserName) ||
                          !string.IsNullOrWhiteSpace(session.UserEmail)
                ? new LaravelAuthUser
                {
                    Id = session.UserId,
                    Name = session.UserName,
                    Email = session.UserEmail
                }
                : null;

            _tokenStore.SaveSession(session);
            StateChanged?.Invoke();
        }

        private void UpdateTokens(LaravelAuthResult result)
        {
            if (result.IsSuccess && result.Tokens != null)
            {
                AccessToken = result.Tokens.AccessToken;
                RefreshToken = result.Tokens.RefreshToken;
                CurrentUser = result.User;

                if (!string.IsNullOrWhiteSpace(AccessToken))
                {
                    var session = PersistedAuthSession.From(result.Tokens, result.User);
                    _tokenStore.SaveSession(session);
                }

                StateChanged?.Invoke();
            }
        }
    }
}
