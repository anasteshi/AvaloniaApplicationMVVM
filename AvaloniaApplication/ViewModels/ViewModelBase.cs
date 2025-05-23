﻿using Avalonia.Threading;
using AvaloniaApplication.DataModels;
using AvaloniaApplication.Services;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    #region Private Members

    /// <summary>
    /// The audio capture service
    /// </summary>
    private IAudioCaptureService mAudioCaptureService;

    /// <summary>
    /// A slow tick counter to update the text slower than the graphs and bars
    /// </summary>
    private int mUpdatecounter;
    
    #endregion

    #region Public Properties

    [ObservableProperty] private string _boldTitle = "AVALONIA";

    [ObservableProperty] private string _regularTitle = "LOUDNESS METER";

    [ObservableProperty] private string _shortTermLoudness = "0 LUFS";

    [ObservableProperty] private string _integratedLoudness = "0 LUFS";

    [ObservableProperty] private string _loudnessRange = "0 LU";

    [ObservableProperty] private string _realtimeDynamics = "0 LU";

    [ObservableProperty] private string _averageDynamics = "0 LU";

    [ObservableProperty] private string _momentaryMaxLoudness = "0 LUFS";

    [ObservableProperty] private string _shortTermMaxLoudness = "0 LUFS";

    [ObservableProperty] private string _truePeakMax = "0 dB";

    [ObservableProperty] private bool _channelConfigurationListIsOpen;

    [ObservableProperty] private double _volumePercentPosition;

    [ObservableProperty] private double _volumeContainerHeight;

    [ObservableProperty] private double _volumeBarHeight;

    [ObservableProperty] private double _volumeBarMaskHeight;

    [ObservableProperty]
    private ObservableGroupedCollection<string, ChannelConfigurationItem> _channelConfigurations = default!;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ChannelConfigurationButtonText))]
    private ChannelConfigurationItem? _selectedChannelConfiguration;

    public string ChannelConfigurationButtonText => SelectedChannelConfiguration?.ShortText ?? "Select Channel";

    public ObservableCollection<ObservableValue> MainChartValues = new ObservableCollection<ObservableValue>();

    public ISeries[] Series { get; set; }

    public List<Axis> YAxis { get; set; } = 
        new List<Axis>
        {
            new Axis
            {
                MinStep = 1,
                ForceStepToMin = true,
                MinLimit = 0,
                MaxLimit = 60,
                Labeler = (val) => (Math.Min(60, Math.Max(0, val)) - 60).ToString(),
                IsVisible = false
                //IsInverted = true
            }
        };

    #endregion

    #region Public Commands

    [RelayCommand]
    private void ChannelConfigurationButtonPressed() => ChannelConfigurationListIsOpen ^= true;

    [RelayCommand]
    private void ChannelConfigurationItemPressed(ChannelConfigurationItem item)
    {
        // Update the selected item
        SelectedChannelConfiguration = item;

        // Close the menu
        ChannelConfigurationListIsOpen = false;
    }

    /// <summary>
    /// Do initial loading of data and settings up services
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        // Get the channel configuration data
        var channelConfigurations = await mAudioCaptureService.GetChannelConfigurationsAsync();

        // Create a grouping from the flat data
        ChannelConfigurations =
            new ObservableGroupedCollection<string, ChannelConfigurationItem>(
                channelConfigurations.GroupBy(item => item.Group));
        
        StartCapture(deviceId: 1);
    }
    
    #endregion

    #region Constructor

    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <param name="audioInterfaceService">The audio interface service</param>
    public ViewModelBase(IAudioCaptureService audioInterfaceService)
    {
        mAudioCaptureService = audioInterfaceService;
        
        Initialize();
    }

    /// <summary>
    ///     Design-time constructor
    /// </summary>
    public ViewModelBase()
    {
        mAudioCaptureService = new BassAudioCaptureService();
        
        Initialize();
    }
    
    #endregion

    private void Initialize()
    {
        // Chart width 170 values
        MainChartValues.AddRange(Enumerable.Range(0, 170).Select(f => new ObservableValue(0)));
        
        // Set up series
        Series = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = MainChartValues,
                GeometrySize = 0,
                GeometryStroke = null,
                Fill = new SolidColorPaint(new SKColor(63, 77, 99)),
                Stroke = new SolidColorPaint(new SKColor(120, 152, 203)) { StrokeThickness = 3 },
            }
        };
    }
    
    /// <summary>
    /// Starts capturing audio from the specified device
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    private void StartCapture(int deviceId)
    {
        // Initialize capturing on specific device
        mAudioCaptureService.InitCapture(deviceId);
        
        // Listen out for chunks of information
        mAudioCaptureService.AudioChunkAvailable += audioChuckData =>
        {
            ProcessAudioChunk(audioChuckData);
        };
        
        // Start capturing
        mAudioCaptureService.Start();
    }

    private void ProcessAudioChunk(AudioChunkData audioChuckData)
    {
        // Counter between 0-1-2
        mUpdatecounter = (mUpdatecounter+ 1) % 3;
        
        // Every time counter is at 0...
        if (mUpdatecounter == 0)
        {
            ShortTermLoudness = $"{Math.Max(-60, audioChuckData.ShortTermLUFS):0.0} LUFS";
            IntegratedLoudness = $"{Math.Max(-60, audioChuckData.IntegratedLUFS):0.0} LUFS";
            LoudnessRange = $"{Math.Max(-60, audioChuckData.LoudnessRange):0.0} LU";
            RealtimeDynamics = $"{Math.Max(-60, audioChuckData.RealtimeDynamics):0.0} LU";
            AverageDynamics = $"{Math.Max(-60, audioChuckData.AverageRealtimeDynamics):0.0} LU";
            MomentaryMaxLoudness = $"{Math.Max(-60, audioChuckData.MomentaryMaxLUFS):0.0} LUFS";
            ShortTermMaxLoudness = $"{Math.Max(-60, audioChuckData.ShortTermMaxLUFS):0.0} LUFS";
            TruePeakMax = $"{Math.Max(-60, audioChuckData.TruePeakMax):0.0} dB";
            
            // Update charge on UI thread
            Dispatcher.UIThread.Invoke(() =>
            {
                MainChartValues.RemoveAt(0);
                MainChartValues.Add( new (Math.Max(0, 60 + audioChuckData.ShortTermLUFS)));
            });
        }

        // Set volume bar height
        VolumeBarMaskHeight = Math.Min(_volumeBarHeight, _volumeBarHeight / 60 * -audioChuckData.Loudness);
        
        // Set Volume Arrow height
        VolumePercentPosition = Math.Min(_volumeContainerHeight, _volumeContainerHeight / 60 * -audioChuckData.ShortTermLUFS);
    }
}