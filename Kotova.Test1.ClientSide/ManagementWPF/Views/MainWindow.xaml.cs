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
                // Just minimize instead of closing
                e.Cancel = true;
                this.Hide();

                // Optional: Hide from taskbar and show in system tray
                this.ShowInTaskbar = false;
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