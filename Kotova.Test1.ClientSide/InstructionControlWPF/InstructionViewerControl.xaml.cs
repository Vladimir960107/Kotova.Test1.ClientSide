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
        if (DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.OpenNormativeDocument();
        }
    }

    private void RelatedFilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.OpenRelatedFile();
        }
    }

    // Dependency Properties for configuration
    public static readonly DependencyProperty AllowCompletionProperty =
        DependencyProperty.Register("AllowCompletion", typeof(bool), typeof(InstructionViewerControl),
            new PropertyMetadata(false, OnAllowCompletionChanged));

    public bool AllowCompletion
    {
        get { return (bool)GetValue(AllowCompletionProperty); }
        set { SetValue(AllowCompletionProperty, value); }
    }

    private static void OnAllowCompletionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InstructionViewerControl control && control.DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.AllowCompletion = (bool)e.NewValue;
        }
    }

    public static readonly DependencyProperty IsChiefProperty =
        DependencyProperty.Register("IsChief", typeof(bool), typeof(InstructionViewerControl),
            new PropertyMetadata(false, OnIsChiefChanged));

    public bool IsChief
    {
        get { return (bool)GetValue(IsChiefProperty); }
        set { SetValue(IsChiefProperty, value); }
    }

    private static void OnIsChiefChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InstructionViewerControl control && control.DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.IsChief = (bool)e.NewValue;
        }
    }

    public static readonly DependencyProperty JwtTokenProperty =
        DependencyProperty.Register("JwtToken", typeof(string), typeof(InstructionViewerControl),
            new PropertyMetadata(string.Empty, OnJwtTokenChanged));

    public string JwtToken
    {
        get { return (string)GetValue(JwtTokenProperty); }
        set { SetValue(JwtTokenProperty, value); }
    }

    private static void OnJwtTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InstructionViewerControl control && control.DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.JwtToken = (string)e.NewValue;
        }
    }

    public static readonly DependencyProperty UserNameProperty =
        DependencyProperty.Register("UserName", typeof(string), typeof(InstructionViewerControl),
            new PropertyMetadata(string.Empty, OnUserNameChanged));

    public string UserName
    {
        get { return (string)GetValue(UserNameProperty); }
        set { SetValue(UserNameProperty, value); }
    }

    private static void OnUserNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InstructionViewerControl control && control.DataContext is InstructionViewerViewModel viewModel)
        {
            viewModel.UserName = (string)e.NewValue;
        }
    }

    // Method to initialize the control with parameters
    public void Initialize(string jwtToken, string userName, bool isChief = false, bool allowCompletion = false)
    {
        var viewModel = new InstructionViewerViewModel(jwtToken, userName, isChief, allowCompletion);
        DataContext = viewModel;

        // Set dependency properties
        JwtToken = jwtToken;
        UserName = userName;
        IsChief = isChief;
        AllowCompletion = allowCompletion;
    }
}