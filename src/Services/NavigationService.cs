using BitWallpaper.Services.Contracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WinRT;

namespace BitWallpaper.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;
    private Dictionary<string, Type>? _pageMap;

    public void Initialize(Frame frame, Dictionary<string, Type> pages)
    {
        _frame = frame;
        _pageMap = pages;
    }

    public bool NavigateTo(object? selectedPage, object? parameter, SlideNavigationTransitionEffect effect)
    {
        try
        {
            if (_frame is null) return false;
            if (_pageMap is null) return false;

            string? tag = null;
            if (selectedPage is NavigationViewItem navItem)
            {
                tag = navItem.Tag.ToString();
            }
            else if (selectedPage is string str)
            {
                tag = str;
            }

            if (tag != null && _pageMap.TryGetValue(tag, out var pageType) && _frame.CurrentSourcePageType != pageType)
            {
                return _frame.Navigate(pageType, parameter, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());//new SuppressNavigationTransitionInfo() //new SlideNavigationTransitionInfo() { Effect = effect }
            }
            else
            {
                Debug.WriteLine("NavigationService.NavigateTo: No valid page found (or navigated to the same page) for " + tag);
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"NavigationService.NavigateTo: Exception occurred - {ex.Message}");
            return false;
        }   
    }

    public void GoBack()
    {
        if (_frame != null && _frame.CanGoBack)
        {
            _frame.GoBack();
        }
    }

    public bool CanGoBack()
    {
        return _frame != null && _frame.CanGoBack;
    }
}
