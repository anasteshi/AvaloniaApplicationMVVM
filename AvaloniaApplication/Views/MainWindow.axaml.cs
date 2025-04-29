using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaApplication.Services;
using AvaloniaApplication.ViewModels;
using ManagedBass;

namespace AvaloniaApplication.Views;

public partial class MainWindow : Window
{
    #region Private Members

    private Control mChannelConfigurationButton;
    private Control mChannelConfigurationPopup;
    private Control mMainGrid;
    private Control mVolumeContainer;
    
    /// <summary>
    /// The timeout timer to detect when auto-sizing has finished firing
    /// </summary>
    private Timer mSizingTimer;
    

    #endregion

    #region Constructor

    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <exception cref="Exception"></exception>

    public MainWindow()
    {
        mSizingTimer = new Timer((t) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                //Update desired size
                UpdateSizes();
            });
        });
        
        InitializeComponent();
        DataContext = new ViewModelBase();
        Title = "Avalonia Loudness Meter";
        mChannelConfigurationButton = this.FindControl<Button>("ChannelConfigButton") ??
                                      throw new Exception("Cannot find Channel Configuration Button by name");
        mChannelConfigurationPopup = this.FindControl<Control>("ChannelConfigPopup") ??
                                     throw new Exception("Cannot find Channel Config Popup by name");
        mMainGrid = this.FindControl<Control>("MainGrid") ??
                    throw new Exception("Cannot find Main Grid by name");
        mVolumeContainer = this.FindControl<Control>("VolumeContainer") ??
        throw new Exception("Cannot find Volume Container by name");

            this.Loaded += MainWindow_Loaded;
        this.DataContextChanged += MainWindow_DataContextChanged;
    }
    #endregion

    private void UpdateSizes()
    {
        ((ViewModelBase)DataContext).VolumeContainerSize = mVolumeContainer.Bounds.Height;;
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
                Task.Run(async () =>
                {
                    // Output all devices, then select one
                    foreach (var device in RecordingDevice.Enumerate())
                        Console.WriteLine($"{device?.Index}: {device?.Name}");
 
                    var outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MBass");
                    Directory.CreateDirectory(outputPath);
                    var filePath = Path.Combine(outputPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".wav");
                    using var writer = new WaveFileWriter(new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read), new WaveFormat());
 
                    using var mCaptureDevice = new AudioCaptureService(1);
 
                    mCaptureDevice.DataAvailable += (buffer, length) =>
                    {
                        writer.Write(buffer, length);
                     
                        Console.WriteLine(BitConverter.ToString(buffer));
                    };
             
                    mCaptureDevice.Start();
 
                    await Task.Delay(3000);
                 
                    mCaptureDevice.Stop();
 
                    await Task.Delay(100);
                });

                
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
        
        mSizingTimer.Change(1, int.MaxValue);

        
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