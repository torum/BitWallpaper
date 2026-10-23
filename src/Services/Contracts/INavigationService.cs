using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace BitWallpaper.Services.Contracts;

public interface INavigationService
{
    bool NavigateTo(object? selectedPage, object? parameter, SlideNavigationTransitionEffect effect);
    void Initialize(Frame frame, Dictionary<string, Type> pages);
    void GoBack();
    bool CanGoBack();
}
