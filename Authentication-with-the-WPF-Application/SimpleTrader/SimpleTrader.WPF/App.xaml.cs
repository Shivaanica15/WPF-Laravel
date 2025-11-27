using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleTrader.WPF.HostBuilders;
using SimpleTrader.WPF.Services;
using SimpleTrader.WPF.State.Authenticators;
using SimpleTrader.WPF.State.Messages;
using System;
using System.Windows;

namespace SimpleTrader.WPF
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = CreateHostBuilder().Build();
        }

        public static IHostBuilder CreateHostBuilder(string[] args = null)
        {
            return Host.CreateDefaultBuilder(args)
                .AddConfiguration()
                .ConfigureServices((context, services) =>
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

                    services.AddSingleton<IAuthenticator, Authenticator>();
                    services.AddSingleton<SuccessMessageStore>();
                })
                .AddViewModels()
                .AddViews();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _host.Start();

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();

            base.OnExit(e);
        }
    }
}
