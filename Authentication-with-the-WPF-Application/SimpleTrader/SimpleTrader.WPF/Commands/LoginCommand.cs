using SimpleTrader.WPF.State.Authenticators;
using SimpleTrader.WPF.State.Messages;
using SimpleTrader.WPF.State.Navigators;
using SimpleTrader.WPF.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SimpleTrader.WPF.Commands
{
    public class LoginCommand : AsyncCommandBase
    {
        private readonly LoginViewModel _loginViewModel;
        private readonly IAuthenticator _authenticator;
        private readonly SuccessMessageStore _successMessageStore;
        private readonly ViewModelDelegateRenavigator<HomeViewModel> _homeRenavigator;

        public LoginCommand(
            LoginViewModel loginViewModel,
            IAuthenticator authenticator,
            SuccessMessageStore successMessageStore,
            ViewModelDelegateRenavigator<HomeViewModel> homeRenavigator)
        {
            _loginViewModel = loginViewModel;
            _authenticator = authenticator;
            _successMessageStore = successMessageStore;
            _homeRenavigator = homeRenavigator;

            _loginViewModel.PropertyChanged += LoginViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            return _loginViewModel.CanLogin && base.CanExecute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            _loginViewModel.ErrorMessage = string.Empty;
            _loginViewModel.StatusMessage = string.Empty;

            try
            {
                var result = await _authenticator.Login(_loginViewModel.Email, _loginViewModel.Password);

                if (result.IsSuccess)
                {
                    _loginViewModel.StatusMessage = result.Message;
                    _successMessageStore.Message = string.IsNullOrWhiteSpace(result.Message)
                        ? HomeViewModel.DefaultSuccessMessage
                        : result.Message;
                    _homeRenavigator.Renavigate();
                }
                else
                {
                    _loginViewModel.ErrorMessage = result.Message;
                }
            }
            catch (Exception)
            {
                _loginViewModel.ErrorMessage = "Login failed. Please try again.";
            }
        }

        private void LoginViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginViewModel.CanLogin))
            {
                OnCanExecuteChanged();
            }
        }
    }
}
