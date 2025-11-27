using SimpleTrader.WPF.Commands;
using SimpleTrader.WPF.State.Authenticators;
using SimpleTrader.WPF.State.Messages;
using SimpleTrader.WPF.State.Navigators;
using System.Windows.Input;

namespace SimpleTrader.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly SuccessMessageStore _successMessageStore;

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(CanLogin));
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
                OnPropertyChanged(nameof(CanLogin));
            }
        }

        public bool CanLogin => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

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

        public ICommand LoginCommand { get; }
        public ICommand NavigateRegisterCommand { get; }

        public LoginViewModel(
            IAuthenticator authenticator,
            ViewModelDelegateRenavigator<RegisterViewModel> registerRenavigator,
            ViewModelDelegateRenavigator<HomeViewModel> homeRenavigator,
            SuccessMessageStore successMessageStore)
        {
            _successMessageStore = successMessageStore;
            ErrorMessageViewModel = new MessageViewModel();
            StatusMessageViewModel = new MessageViewModel();

            LoginCommand = new LoginCommand(this, authenticator, successMessageStore, homeRenavigator);
            NavigateRegisterCommand = new RenavigateCommand(registerRenavigator);

            LoadPersistedStatusMessage();
        }

        public override void Dispose()
        {
            ErrorMessageViewModel.Dispose();
            StatusMessageViewModel.Dispose();
            base.Dispose();
        }

        private void LoadPersistedStatusMessage()
        {
            if (string.IsNullOrWhiteSpace(_successMessageStore?.Message))
            {
                return;
            }

            StatusMessage = _successMessageStore.Message;
            _successMessageStore.Message = string.Empty;
        }
    }
}
