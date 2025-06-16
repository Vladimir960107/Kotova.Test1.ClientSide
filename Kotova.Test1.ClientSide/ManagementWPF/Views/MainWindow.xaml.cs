using System;
using System.ComponentModel;
using System.Windows;
using Kotova.Test1.ClientSide.ManagementWPF.ViewModels;

namespace Kotova.Test1.ClientSide.ManagementWPF.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _forceClose = false;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_forceClose)
            {
                // Ask user if they want to sign out or just minimize
                var result = System.Windows.MessageBox.Show(
                    "Хотите выйти из учётной записи или свернуть окно?\n\nДа - Выйти из аккаунта\nНет - Свернуть окно\nОтмена - Остаться",
                    "Закрытие окна",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // Sign out
                        _viewModel.SignOutCommand.Execute(null);
                        e.Cancel = true; // Cancel the close, let SignOut handle it
                        break;
                    case MessageBoxResult.No:
                        // Minimize to taskbar
                        e.Cancel = true;
                        this.WindowState = WindowState.Minimized;
                        break;
                    case MessageBoxResult.Cancel:
                        // Stay open
                        e.Cancel = true;
                        break;
                }
            }
            else
            {
                base.OnClosing(e);
            }
        }

        // Method to properly close the window when needed
        public void ForceClose()
        {
            _forceClose = true;
            this.Close();
        }

        // Event to notify when window is hidden (for integration with Login_Russian)
        public event Action OnWindowHidden;

        private void NotifyWindowHidden()
        {
            OnWindowHidden?.Invoke();
        }
    }
}