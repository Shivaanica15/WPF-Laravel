using SimpleTrader.WPF.Commands;
using SimpleTrader.WPF.State.Authenticators;
using SimpleTrader.WPF.State.Navigators;
using System.Windows.Input;

namespace SimpleTrader.WPF.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        public bool CanRegister =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(ConfirmPassword);

        public MessageViewModel ErrorMessageViewModel { get; }
        public MessageViewModel StatusMessageViewModel { get; }

        public string ErrorMessage
        {
            set => ErrorMessageViewModel.Message = value;
        }

        public string StatusMessage
        {
            set => StatusMessageViewModel.Message = value;
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateLoginCommand { get; }

        public RegisterViewModel(IAuthenticator authenticator, ViewModelDelegateRenavigator<LoginViewModel> loginRenavigator)
        {
            ErrorMessageViewModel = new MessageViewModel();
            StatusMessageViewModel = new MessageViewModel();

            RegisterCommand = new RegisterCommand(this, authenticator);
            NavigateLoginCommand = new RenavigateCommand(loginRenavigator);
        }

        public override void Dispose()
        {
            ErrorMessageViewModel.Dispose();
            StatusMessageViewModel.Dispose();
            base.Dispose();
        }
    }
}