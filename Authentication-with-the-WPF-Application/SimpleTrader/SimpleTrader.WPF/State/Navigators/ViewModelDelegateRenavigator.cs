using SimpleTrader.WPF.ViewModels;
using System;

namespace SimpleTrader.WPF.State.Navigators
{
    public class ViewModelDelegateRenavigator<TViewModel> : IRenavigator where TViewModel : ViewModelBase
    {
        private readonly INavigator _navigator;
        private readonly Func<TViewModel> _createViewModel;

        public ViewModelDelegateRenavigator(INavigator navigator, Func<TViewModel> createViewModel)
        {
            _navigator = navigator;
            _createViewModel = createViewModel;
        }

        public void Renavigate()
        {
            _navigator.CurrentViewModel = _createViewModel();
        }
    }
}