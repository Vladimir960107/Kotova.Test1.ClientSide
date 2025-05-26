using Kotova.Test1.ClientSide.ManagementWPF.Helpers;
using Kotova.Test1.ClientSide.ManagementWPF.Services;
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

        public MainViewModel(IApiService apiService, string currentUser)
        {
            _apiService = apiService;
            _currentUser = currentUser;

            UnplannedInstructionViewModel = new UnplannedInstructionViewModel(_apiService);
            LogoutCommand = new RelayCommand(Logout);

            StatusMessage = "Готово к работе";
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

        public RelayCommand LogoutCommand { get; }

        private void Logout()
        {
            var result = System.Windows.MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}