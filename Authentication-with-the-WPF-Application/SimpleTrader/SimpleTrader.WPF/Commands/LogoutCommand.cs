using SimpleTrader.WPF.State.Authenticators;
using SimpleTrader.WPF.State.Messages;
using SimpleTrader.WPF.State.Navigators;
using SimpleTrader.WPF.ViewModels;
using System;
using System.Windows.Input;

namespace SimpleTrader.WPF.Commands
{
    public class LogoutCommand : ICommand
    {
        private readonly IAuthenticator _authenticator;
        private readonly SuccessMessageStore _successMessageStore;
        private readonly ViewModelDelegateRenavigator<LoginViewModel> _loginRenavigator;

        public LogoutCommand(
            IAuthenticator authenticator,
            SuccessMessageStore successMessageStore,
            ViewModelDelegateRenavigator<LoginViewModel> loginRenavigator)
        {
            _authenticator = authenticator;
            _successMessageStore = successMessageStore;
            _loginRenavigator = loginRenavigator;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _authenticator.Logout();
            _successMessageStore.Message = "Logged out successfully.";
            _loginRenavigator.Renavigate();
        }
    }
}
