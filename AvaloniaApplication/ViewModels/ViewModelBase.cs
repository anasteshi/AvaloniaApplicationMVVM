using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _boldTitle = "AVALONIA";
    [ObservableProperty]
    private string _regularTitle = "LOUDNESS METER";
    [ObservableProperty]
    private bool _channelConfigListIsOpen = false;

    [RelayCommand]
    private void ChannelConfigButtonPressed() => ChannelConfigListIsOpen ^= true;

}