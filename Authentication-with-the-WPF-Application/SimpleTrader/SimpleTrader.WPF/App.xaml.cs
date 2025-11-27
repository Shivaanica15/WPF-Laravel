using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleTrader.WPF.Commands;
using SimpleTrader.WPF.Services;
using SimpleTrader.WPF.State.Authenticators;
using SimpleTrader.WPF.State.Messages;
using SimpleTrader.WPF.State.Navigators;
using SimpleTrader.WPF.ViewModels;
using System;
using System.Windows;

namespace SimpleTrader.WPF
{
    public partial class App : Application
    {
        private IHost _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => ConfigureServices(context, services))
                .Build();

            _host.Start();

            InitializeRootView();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }

        private void InitializeRootView()
        {
            var navigator = _host.Services.GetRequiredService<INavigator>();
            var authenticator = _host.Services.GetRequiredService<IAuthenticator>();
            var tokenStore = _host.Services.GetRequiredService<TokenStore>();
            var successMessageStore = _host.Services.GetRequiredService<SuccessMessageStore>();

            PersistedAuthSession savedSession = tokenStore.LoadSession();

            if (savedSession != null && !string.IsNullOrWhiteSpace(savedSession.AccessToken))
            {
                authenticator.RestoreSession(savedSession);

                if (successMessageStore != null)
                {
                    successMessageStore.Message = string.IsNullOrWhiteSpace(savedSession.UserName)
                        ? HomeViewModel.DefaultSuccessMessage
                        : $"Welcome back, {savedSession.UserName}!";
                }

                // FORCE OPEN HOME
                navigator.CurrentViewModel = _host.Services.GetRequiredService<HomeViewModel>();
            }
            else
            {
                // FORCE OPEN LOGIN
                navigator.CurrentViewModel = _host.Services.GetRequiredService<LoginViewModel>();
            }

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHttpClient<LaravelAuthService>(client =>
            {
                string baseUrl = context.Configuration.GetValue<string>("Api:BaseUrl");
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new InvalidOperationException("Api:BaseUrl configuration is missing.");
                }

                client.BaseAddress = new Uri(baseUrl);
            });

            services.AddSingleton<TokenStore>();
            services.AddSingleton<IAuthenticator, Authenticator>();
            services.AddSingleton<SuccessMessageStore>();

            services.AddSingleton<INavigator, Navigator>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddSingleton<MainViewModel>();

            services.AddTransient<LogoutCommand>();

            services.AddSingleton<Func<LoginViewModel>>(sp => () => sp.GetRequiredService<LoginViewModel>());
            services.AddSingleton<Func<HomeViewModel>>(sp => () => sp.GetRequiredService<HomeViewModel>());
            services.AddSingleton<Func<RegisterViewModel>>(sp => () => sp.GetRequiredService<RegisterViewModel>());

            services.AddSingleton<ViewModelDelegateRenavigator<LoginViewModel>>();
            services.AddSingleton<ViewModelDelegateRenavigator<HomeViewModel>>();
            services.AddSingleton<ViewModelDelegateRenavigator<RegisterViewModel>>();

            services.AddSingleton<MainWindow>(sp => new MainWindow(sp.GetRequiredService<MainViewModel>()));
        }
    }
}
