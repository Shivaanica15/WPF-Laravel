using SimpleTrader.WPF.ViewModels;
using System;

namespace SimpleTrader.WPF.State.Navigators
{
    public interface INavigator
    {
        ViewModelBase CurrentViewModel { get; set; }
        event Action StateChanged;
    }
}