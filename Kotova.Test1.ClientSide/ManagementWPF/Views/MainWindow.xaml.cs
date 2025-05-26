using System.Windows;
using Kotova.Test1.ClientSide.ManagementWPF.ViewModels;

namespace Kotova.Test1.ClientSide.ManagementWPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}