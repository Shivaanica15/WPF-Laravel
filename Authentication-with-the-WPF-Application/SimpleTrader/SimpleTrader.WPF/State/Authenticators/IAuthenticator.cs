using SimpleTrader.WPF.Services;
using System;
using System.Threading.Tasks;

namespace SimpleTrader.WPF.State.Authenticators
{
    public interface IAuthenticator
    {
        bool IsLoggedIn { get; }
        string AccessToken { get; }
        string RefreshToken { get; }
        LaravelAuthUser CurrentUser { get; }

        event Action StateChanged;

        Task<LaravelAuthResult> Register(string name, string email, string password, string confirmPassword);
        Task<LaravelAuthResult> Login(string email, string password);
        void RestoreSession(PersistedAuthSession session);
        void Logout();
    }
}
