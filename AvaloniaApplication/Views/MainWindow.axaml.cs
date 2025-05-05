using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaApplication.Services;
using AvaloniaApplication.ViewModels;
using ManagedBass;
using NWaves.Signals;
using NWaves.Utils;

namespace AvaloniaApplication.Views;

public partial class MainWindow : Window
{
    #region Private Members

    private ViewModelBase mViewModel => (ViewModelBase)DataContext;
    private AudioCaptureService  mCaptureDevice ;
    private int mCaptureFrequency = 44100;
    private Queue<double> mLufs = new Queue<double>();

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
    }
    #endregion

    private void UpdateSizes()
    {
        mViewModel.VolumeContainerSize = mVolumeContainer.Bounds.Height;;
    }
    
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        await mViewModel.LoadSettingsCommand.ExecuteAsync(null);
 
        StartCapture(1);
             
        base.OnLoaded(e);
    }

    private void StartCapture(int deviceId)
    {
        mCaptureDevice = new AudioCaptureService(deviceId, mCaptureFrequency);

        mCaptureDevice.DataAvailable += (buffer, length) => { CalculateValues(buffer); };

        mCaptureDevice.Start();
    }

    private void CalculateValues(byte[] buffer)
    {
        // Console.WriteLine(BitConverter.ToString(buffer));

        // Get total PCM16 samples in this buffer (16 bits per sample)
        var sampleCount = buffer.Length / 2;
        
        // Create our Discrete Signal ready to be filled with information
        var signal = new DiscreteSignal(mCaptureFrequency, sampleCount);
        
        // Loop all bytes and extract the 16 bits into signal floats
        using var reader = new BinaryReader(new MemoryStream(buffer));
        for (var i = 0; i < sampleCount; i++)
            signal[i] = reader.ReadInt16() / 32768f; 
        
        // Calculate the LUFS
        var lufs = Scale.ToDecibel(signal.Rms() * 1.2);
        mLufs.Enqueue(lufs);
        
        // Keep the list to 10 samples
        if (mLufs.Count > 10)
            mLufs.Dequeue();

        var averageLufs = mLufs.Average();
     
        Dispatcher.UIThread.InvokeAsync(() => mViewModel.ShortTermLoudness = $"{averageLufs:0.0} LUFS");
    }

    // You might not need OnInitialized anymore, or just keep the base call
    protected override void OnInitialized()
    {
        base.OnInitialized();
        // Don't call the command here anymore
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

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
        mViewModel.ChannelConfigButtonPressedCommand.Execute(null);
    }
}