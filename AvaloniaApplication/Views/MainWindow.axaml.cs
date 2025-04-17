using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
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
    }

    private void MainWindow_Loaded(object sender, EventArgs e)
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
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var viewModel = this.DataContext as ViewModelBase; // Correct cast
        viewModel?.ChannelConfigButtonPressedCommand.Execute(null);
    }
}