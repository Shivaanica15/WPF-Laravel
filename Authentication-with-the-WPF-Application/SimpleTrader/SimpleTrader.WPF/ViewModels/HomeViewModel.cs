using SimpleTrader.WPF.Commands;
using SimpleTrader.WPF.State.Messages;
using System.Windows.Input;

namespace SimpleTrader.WPF.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public const string DefaultSuccessMessage = "Welcome!";

        private string _successMessage;

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                if (_successMessage == value)
                {
                    return;
                }

                _successMessage = value;
                OnPropertyChanged(nameof(SuccessMessage));
            }
        }

        public ICommand LogoutCommand { get; }

        public HomeViewModel(SuccessMessageStore successMessageStore, LogoutCommand logoutCommand)
        {
            SuccessMessage = string.IsNullOrWhiteSpace(successMessageStore?.Message)
                ? DefaultSuccessMessage
                : successMessageStore.Message;
            LogoutCommand = logoutCommand;
        }
    }
}
