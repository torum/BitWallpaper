using BitWallpaper.Models;
using BitWallpaper.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BitWallpaper.Services.Contracts;

public interface IConfigurationService
{
    void Initialize(string configFilePath);
    Task<Models.Configuration> LoadConfigurationAsync();
    Task SaveConfigurationAsync(ObservableCollection<PairViewModel> pairs);
    Models.Configuration CurrentConfiguration { get; }
    string ConfigFilePath { get; }
    void GetPairs(ObservableCollection<PairViewModel> pairs);
}