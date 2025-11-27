using SimpleTrader.WPF.ViewModels;
using System;

namespace SimpleTrader.WPF.State.Navigators
{
    public class Navigator : INavigator
    {
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel == value)
                {
                    return;
                }

                _currentViewModel?.Dispose();
                _currentViewModel = value;
                StateChanged?.Invoke();
            }
        }

        public event Action StateChanged;
    }
}