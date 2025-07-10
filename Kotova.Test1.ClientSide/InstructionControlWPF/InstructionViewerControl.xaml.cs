using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ListBox = System.Windows.Controls.ListBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Kotova.Test1.ClientSide.InstructionControlWPF;

public partial class InstructionViewerControl : UserControl
{
    public InstructionViewerControl()
    {
        InitializeComponent();
        // Add BooleanToVisibilityConverter to resources if not already there
        if (!Resources.Contains("BoolToVisConverter"))
        {
            Resources.Add("BoolToVisConverter", new BooleanToVisibilityConverter());
        }
    }

    // Event handlers that delegate to the ViewModel
    private void NormativeInstructionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is InstructionViewerViewModel viewModel && sender is ListBox listBox)
        {
            if (listBox.SelectedItem is InstructionViewerViewModel.NormativeInstructionItem selectedItem)
            {
                // Use the new method that handles the specific item
                viewModel.OpenNormativeDocumentByItem(selectedItem);
            }
        }
    }

    private void RelatedFilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.OpenRelatedFile();
        }
    }

    // Selection changed handlers
    private void NormativeInstructionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is InstructionViewerViewModel viewModel && sender is ListBox listBox)
        {
            if (listBox.SelectedItem is InstructionViewerViewModel.NormativeInstructionItem selectedItem)
            {
                // Mark the selected item as checked for tracking
                selectedItem.IsChecked = true;

                // Uncheck previously selected items
                foreach (var item in viewModel.NormativeInstructions)
                {
                    if (item != selectedItem)
                    {
                        item.IsChecked = false;
                    }
                }
            }
        }
    }

    private void RelatedFilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Handle related files selection if needed
        if (DataContext is InstructionViewerViewModel viewModel && sender is ListBox listBox)
        {
            // Can add specific handling for related files selection here
        }
    }

    // Button click handlers
    private void PassInstructionButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.PassInstructionCommand?.Execute(null);
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.RefreshInstructions();
        }
    }

    // Dependency Properties for configuration
    public static readonly DependencyProperty AllowCompletionProperty =
        DependencyProperty.Register(
            nameof(AllowCompletion),
            typeof(bool),
            typeof(InstructionViewerControl),
            new PropertyMetadata(false));

    public bool AllowCompletion
    {
        get => (bool)GetValue(AllowCompletionProperty);
        set => SetValue(AllowCompletionProperty, value);
    }

    public static readonly DependencyProperty JwtTokenProperty =
        DependencyProperty.Register(
            nameof(JwtToken),
            typeof(string),
            typeof(InstructionViewerControl),
            new PropertyMetadata(string.Empty));

    public string JwtToken
    {
        get => (string)GetValue(JwtTokenProperty);
        set => SetValue(JwtTokenProperty, value);
    }

    public static readonly DependencyProperty UserNameProperty =
        DependencyProperty.Register(
            nameof(UserName),
            typeof(string),
            typeof(InstructionViewerControl),
            new PropertyMetadata(string.Empty));

    public string UserName
    {
        get => (string)GetValue(UserNameProperty);
        set => SetValue(UserNameProperty, value);
    }

    public static readonly DependencyProperty IsChiefProperty =
        DependencyProperty.Register(
            nameof(IsChief),
            typeof(bool),
            typeof(InstructionViewerControl),
            new PropertyMetadata(false));

    public bool IsChief
    {
        get => (bool)GetValue(IsChiefProperty);
        set => SetValue(IsChiefProperty, value);
    }

    // Helper method to open URLs (can be used by both double-click handlers)
    private void OpenUrl(string url)
    {
        try
        {
            if (!string.IsNullOrEmpty(url))
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка при открытии URL: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}