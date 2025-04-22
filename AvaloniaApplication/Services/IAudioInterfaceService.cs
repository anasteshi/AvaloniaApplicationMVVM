using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaApplication.DataModels;

namespace AvaloniaApplication.Services;

public interface IAudioInterfaceService
{
    /// <summary>
    /// Fetch the channel configurations
    /// </summary>
    /// <returns></returns>
    Task<List<ChannelConfigurationItem>> GetChannelConfigurationsAsync();
}