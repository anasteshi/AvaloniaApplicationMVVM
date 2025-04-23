using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaApplication.ViewModels;

namespace AvaloniaApplication.Views;

public partial class MainWindow : Window
{
    #region Private Members

    private Control mChannelConfigurationButton;
    private Control mChannelConfigurationPopup;
    private Control mMainGrid;
    

    #endregion

    #region Constructor

    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <exception cref="Exception"></exception>
    

    #endregion


    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModelBase();
        Title = "Avalonia Loudness Meter";
        mChannelConfigurationButton = this.FindControl<Button>("ChannelConfigButton") ??
                                      throw new Exception("Cannot find Channel Configuration Button by name");
        mChannelConfigurationPopup = this.FindControl<Control>("ChannelConfigPopup") ??
                                     throw new Exception("Cannot find Channel Config Popup by name");
        mMainGrid = this.FindControl<Control>("MainGrid") ??
                    throw new Exception("Cannot find Main Grid by name");
        this.Loaded += MainWindow_Loaded;
        this.DataContextChanged += MainWindow_DataContextChanged;
    }
    
    private async void MainWindow_DataContextChanged(object? sender, EventArgs e)
    {
        // Check if the new DataContext is the type you expect
        if (this.DataContext is ViewModelBase viewModel && viewModel.LoadSettingsCommand != null)
        {
            try
            {
                // Now it's much safer to access the ViewModel and its command
                await viewModel.LoadSettingsCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                // Handle potential exceptions from the command execution
                Console.WriteLine($"Error executing LoadSettingsCommand: {ex}");
                // Maybe show an error message to the user
            }
        }
    }

// You might not need OnInitialized anymore, or just keep the base call
    protected override void OnInitialized()
    {
        base.OnInitialized();
        // Don't call the command here anymore
    }

    private void MainWindow_Loaded(object sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            //Get relative position of button, in relation to main grid
            var position = mChannelConfigurationButton.TranslatePoint(new Point(), mMainGrid) ??
                           throw new Exception("Cannot get Translate Point from Configuration Button");
        
            //Set margin of popup so it appears bottom left of button
            mChannelConfigurationPopup.Margin = new Thickness(
                position.X,
                0,
                0,
                MainGrid.Bounds.Height - position.Y - mChannelConfigurationButton.Bounds.Height);
        });
        
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var viewModel = this.DataContext as ViewModelBase; // Correct cast
        viewModel?.ChannelConfigButtonPressedCommand.Execute(null);
    }
}