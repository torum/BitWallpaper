using System.Threading.Tasks;
using BitWallpaper.Models;

namespace BitWallpaper.Services.Contracts;

public interface IConfigurationService
{
    void Initialize(string configFilePath);
    Task<Models.Configuration> LoadConfigurationAsync();
    Task SaveConfigurationAsync();
    Models.Configuration CurrentConfiguration { get; }
}