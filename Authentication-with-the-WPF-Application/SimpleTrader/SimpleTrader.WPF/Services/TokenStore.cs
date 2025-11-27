using System;
using System.IO;
using System.Text.Json;

namespace SimpleTrader.WPF.Services
{
    public class TokenStore
    {
        private const string DirectoryName = "SimpleTrader";
        private const string TokenFileName = "token.txt";
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _tokenFilePath;

        public TokenStore()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string directory = Path.Combine(appData, DirectoryName);
            _tokenFilePath = Path.Combine(directory, TokenFileName);
        }

        public string GetTokenFilePath() => _tokenFilePath;

        public PersistedAuthSession LoadSession()
        {
            if (!File.Exists(_tokenFilePath))
            {
                return null;
            }

            string content = File.ReadAllText(_tokenFilePath);
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            try
            {
                PersistedAuthSession session = JsonSerializer.Deserialize<PersistedAuthSession>(content, SerializerOptions);
                if (session == null || string.IsNullOrWhiteSpace(session.AccessToken))
                {
                    return null;
                }

                return session;
            }
            catch (JsonException)
            {
                string token = content.Trim();
                return string.IsNullOrWhiteSpace(token)
                    ? null
                    : new PersistedAuthSession { AccessToken = token };
            }
        }

        public void SaveSession(PersistedAuthSession session)
        {
            if (session == null || string.IsNullOrWhiteSpace(session.AccessToken))
            {
                DeleteSession();
                return;
            }

            string directory = Path.GetDirectoryName(_tokenFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string content = JsonSerializer.Serialize(session, SerializerOptions);
            File.WriteAllText(_tokenFilePath, content);
        }

        public void DeleteSession()
        {
            if (File.Exists(_tokenFilePath))
            {
                File.Delete(_tokenFilePath);
            }
        }
    }
}