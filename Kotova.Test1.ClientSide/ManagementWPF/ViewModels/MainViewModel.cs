using Kotova.Test1.ClientSide.ManagementWPF.Helpers;
using Kotova.Test1.ClientSide.ManagementWPF.Services;
using Kotova.Test1.ClientSide.ManagementWPF.Views;
using Kotova.Test1.ClientSideManagementWPF.ViewModels;
using System;
using System.Windows;

namespace Kotova.Test1.ClientSide.ManagementWPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private string _currentUser;
        private string _statusMessage;
        private string _lastUpdateTime;
        private Login_Russian _loginForm;

        public MainViewModel(IApiService apiService, string currentUser, Login_Russian loginForm = null)
        {
            _apiService = apiService;
            _currentUser = currentUser;
            _loginForm = loginForm;

            // Create the UnplannedInstructionViewModel
            UnplannedInstructionViewModel = new UnplannedInstructionViewModel(_apiService);

            // Create commands
            SignOutCommand = new RelayCommand(SignOut);

            StatusMessage = "Готово к работе";
            LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
        }

        public UnplannedInstructionViewModel UnplannedInstructionViewModel { get; }

        public string CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string LastUpdateTime
        {
            get => _lastUpdateTime;
            set => SetProperty(ref _lastUpdateTime, value);
        }

        public RelayCommand SignOutCommand { get; }

        private void SignOut()
        {
            var result = System.Windows.MessageBox.Show(
                "Вы уверены, что хотите выйти из учётной записи?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Delete JWT token
                    Decryption_stuff.DeleteJWTToken();

                    // Close current window
                    var currentWindow = System.Windows.Application.Current.MainWindow;

                    // Show login form if available
                    if (_loginForm != null)
                    {
                        // Use dispatcher to ensure proper thread handling
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {

                            // SET FORCE CLOSE FLAG BEFORE CLOSING
                            if (currentWindow is MainWindow window)
                            {
                                _loginForm.activeWpfWindow = null;
                                window.ForceClose(); 
                            }

                            _loginForm.Show();
                            _loginForm.BringToFront();
                        });
                    }
                    else
                    {
                        // Fallback: shutdown application if no login form reference
                        System.Windows.Application.Current.Shutdown();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Ошибка при выходе: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    // Force shutdown if error occurs
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }

        // Method to update the last update time
        public void UpdateLastUpdateTime()
        {
            LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}