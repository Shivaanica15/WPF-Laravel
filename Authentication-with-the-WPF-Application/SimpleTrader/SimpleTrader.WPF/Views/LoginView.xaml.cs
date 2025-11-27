using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleTrader.WPF.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        // Dependency property for LoginCommand
        public static readonly DependencyProperty LoginCommandProperty =
            DependencyProperty.Register(
                nameof(LoginCommand),
                typeof(ICommand),
                typeof(LoginView),
                new PropertyMetadata(null));

        public ICommand LoginCommand
        {
            get => (ICommand)GetValue(LoginCommandProperty);
            set => SetValue(LoginCommandProperty, value);
        }
    }
}
