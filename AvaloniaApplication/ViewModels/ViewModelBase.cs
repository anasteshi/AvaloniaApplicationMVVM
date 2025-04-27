using Avalonia.Threading;
using AvaloniaApplication.DataModels;
using AvaloniaApplication.Services;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaApplication.DataModels;
using AvaloniaApplication.Services;

namespace AvaloniaApplication.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    #region Private Members
    
    private IAudioInterfaceService mAudioInterfaceService;
    #endregion
    
    #region Public Properties

    [ObservableProperty] private double _volumeContainerSize;
    
    [ObservableProperty] private double _volumePercentPosition;
    
    [ObservableProperty] private string _boldTitle = "AVALONIA";
    
    [ObservableProperty] private string _regularTitle = "LOUDNESS METER";
    
    [ObservableProperty] private bool _channelConfigListIsOpen = false;
    
    [ObservableProperty]
    private ObservableGroupedCollection<string, ChannelConfigurationItem> _channelConfigurations = default!;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChannelConfigurationButtonText))]
    private ChannelConfigurationItem? _selectedChannelConfiguration;

    public string ChannelConfigurationButtonText => SelectedChannelConfiguration?.ShortText ??
                                                    "Select Channel";
    #endregion

    #region Public Commands
    
    [RelayCommand]
    private void ChannelConfigButtonPressed() => ChannelConfigListIsOpen ^= true;

    [RelayCommand]
    public void ChannelConfigurationItemPressed(ChannelConfigurationItem item)
    {
        //Update the selected item
        SelectedChannelConfiguration = item; 
        
        //Close the menu
        ChannelConfigListIsOpen = false;
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        //Get the channel configuration data
        var channelConfigurations = await mAudioInterfaceService.GetChannelConfigurationsAsync();

        //Creating a grouping from the flat data
        ChannelConfigurations =
            new ObservableGroupedCollection<string, ChannelConfigurationItem>(
                channelConfigurations.GroupBy(item => item.Group));
    }
    

    #endregion

    #region Constructor
    
    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <param name="audioInterfaceService"> The audio interface service </param>
    public ViewModelBase(IAudioInterfaceService audioInterfaceService)
    {
        mAudioInterfaceService = audioInterfaceService;
        Initialize();
    }

    /// <summary>
    /// Design time constructor
    /// </summary>
    public ViewModelBase()
    {
        mAudioInterfaceService = new DummyAudioInterfaceService();
        
        Initialize();
    }

    private void Initialize()
    {
        //Temporary code to move volume position
        var tick = 0;
        var input = 0.0;

        var tempTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1/60.0)
        };

        tempTimer.Tick += (s, e) =>
        {
            tick++;

            //Slow down ticks
            input = tick / 20f;
            
            //Scale value
            var scale = VolumeContainerSize / 2f;

            VolumePercentPosition = (Math.Sin(input) + 1) * scale;
        };
        
        tempTimer.Start();
    }
    
    #endregion
    


}