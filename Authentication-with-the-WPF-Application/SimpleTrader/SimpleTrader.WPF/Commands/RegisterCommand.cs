using SimpleTrader.WPF.State.Authenticators;
using SimpleTrader.WPF.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SimpleTrader.WPF.Commands
{
    public class RegisterCommand : AsyncCommandBase
    {
        private readonly RegisterViewModel _registerViewModel;
        private readonly IAuthenticator _authenticator;

        public RegisterCommand(RegisterViewModel registerViewModel, IAuthenticator authenticator)
        {
            _registerViewModel = registerViewModel;
            _authenticator = authenticator;

            _registerViewModel.PropertyChanged += RegisterViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            return _registerViewModel.CanRegister && base.CanExecute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            _registerViewModel.ErrorMessage = string.Empty;
            _registerViewModel.StatusMessage = string.Empty;

            try
            {
                var result = await _authenticator.Register(
                    _registerViewModel.Name,
                    _registerViewModel.Email,
                    _registerViewModel.Password,
                    _registerViewModel.ConfirmPassword);

                if (result.IsSuccess)
                {
                    _registerViewModel.StatusMessage = result.Message;
                }
                else
                {
                    _registerViewModel.ErrorMessage = result.Message;
                }
            }
            catch (Exception)
            {
                _registerViewModel.ErrorMessage = "Registration failed. Please try again.";
            }
        }

        private void RegisterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegisterViewModel.CanRegister))
            {
                OnCanExecuteChanged();
            }
        }
    }
}