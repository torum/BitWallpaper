using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BitWallpaper.Views;

public sealed partial class GridViewPage : Page
{
    public GridViewPage()
    {
        ViewModel = App.GetService<MainViewModel>();

        InitializeComponent();

        ContentArea.SizeChanged += (s, e) =>
        {
            int totalItems = ViewModel.EnabledPairs.Count;
            if (totalItems == 0) return;

            int columns = MyUniformGrid.MaximumRowsOrColumns;
            int rows = (int)Math.Ceiling((double)totalItems / columns);

            Debug.WriteLine($"Total Items: {totalItems}, Columns: {columns}, Rows: {rows}");

            // Calculate exact available spacing
            double totalHorizontalSpacing = MyUniformGrid.MinColumnSpacing * (columns - 1);
            double totalVerticalSpacing = MyUniformGrid.MinRowSpacing * (rows - 1);

            // Calculate maximum theoretical dimensions
            double maxPossibleWidth = (e.NewSize.Width - totalHorizontalSpacing) / columns;
            double maxPossibleHeight = (e.NewSize.Height - totalVerticalSpacing) / rows;

            // FIX: Subtract a larger buffer (e.g., 10-15px) to trick the layout engine into 
            // wrapping into 2 columns. ItemsStretch="Fill" will perfectly stretch it out.
            MyUniformGrid.MinItemWidth = Math.Max(0, maxPossibleWidth - 10);
            MyUniformGrid.MinItemHeight = Math.Max(0, maxPossibleHeight - 2);

            MyItemsRepeater.Height = e.NewSize.Height;
            MyItemsRepeater.InvalidateMeasure();
        };
    }

    public MainViewModel? ViewModel
    {
        get; private set;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        //base.OnNavigatedTo(e);

        if (e.Parameter is MainViewModel mvm)
        {
            ViewModel = mvm;

            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
