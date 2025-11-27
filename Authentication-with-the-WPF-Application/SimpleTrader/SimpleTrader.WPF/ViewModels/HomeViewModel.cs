using SimpleTrader.WPF.State.Messages;

namespace SimpleTrader.WPF.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public const string DefaultSuccessMessage = "Login successful! Welcome to the Simple Trader home page.";

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

        public HomeViewModel(SuccessMessageStore successMessageStore)
        {
            SuccessMessage = string.IsNullOrWhiteSpace(successMessageStore?.Message)
                ? DefaultSuccessMessage
                : successMessageStore.Message;
        }
    }
}
