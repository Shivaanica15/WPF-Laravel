namespace SimpleTrader.WPF.Services
{
    public class PersistedAuthSession
    {
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public static PersistedAuthSession From(LaravelAuthTokens tokens, LaravelAuthUser user)
        {
            if (tokens == null || string.IsNullOrWhiteSpace(tokens.AccessToken))
            {
                return null;
            }

            return new PersistedAuthSession
            {
                UserId = user?.Id,
                UserName = user?.Name,
                UserEmail = user?.Email,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            };
        }
    }
}
