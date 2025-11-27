using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleTrader.WPF.Commands;
using SimpleTrader.WPF.State.Navigators;
using SimpleTrader.WPF.ViewModels;
using System;

namespace SimpleTrader.WPF.HostBuilders
{
    public static class AddViewModelsHostBuilderExtensions
    {
        public static IHostBuilder AddViewModels(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddSingleton<INavigator, Navigator>();

                services.AddTransient<LoginViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<RegisterViewModel>();
                services.AddSingleton<MainViewModel>();

                services.AddTransient<LogoutCommand>();

                services.AddSingleton<Func<LoginViewModel>>(serviceProvider => () => serviceProvider.GetRequiredService<LoginViewModel>());
                services.AddSingleton<Func<HomeViewModel>>(serviceProvider => () => serviceProvider.GetRequiredService<HomeViewModel>());
                services.AddSingleton<Func<RegisterViewModel>>(serviceProvider => () => serviceProvider.GetRequiredService<RegisterViewModel>());

                services.AddSingleton<ViewModelDelegateRenavigator<LoginViewModel>>();
                services.AddSingleton<ViewModelDelegateRenavigator<HomeViewModel>>();
                services.AddSingleton<ViewModelDelegateRenavigator<RegisterViewModel>>();
            });

            return host;
        }
    }
}
