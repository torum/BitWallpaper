
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BitWallpaper.Models;

public partial class Depth : ObservableObject
{
    private string _priceFormat = "";
    public string PriceFormat
    {
        get => _priceFormat;
        set
        {
            if (_priceFormat == value)
            {
                return;
            }

            _priceFormat = value;
            OnPropertyChanged(nameof(PriceFormat));
            OnPropertyChanged(nameof(DepthPriceFormatted));
        }
    }

    private decimal _depthBid;
    public decimal DepthBid
    {
        get => _depthBid;
        set
        {
            if (_depthBid == value)
            {
                return;
            }

            _depthBid = value;
            OnPropertyChanged(nameof(DepthBid));
            OnPropertyChanged(nameof(DepthBidFormatted));
        }
    }
    public string DepthBidFormatted
    {
        get
        {
            if (_depthBid == 0)
            {
                return "";
            }
            else
            {
                return _depthBid.ToString();
            }
        }
    }

    private decimal _depthPrice;
    public decimal DepthPrice
    {
        get => _depthPrice;
        set
        {
            if (_depthPrice == value)
            {
                return;
            }

            _depthPrice = value;
            OnPropertyChanged(nameof(DepthPrice));
            OnPropertyChanged(nameof(DepthPriceFormatted));
        }
    }
    public string DepthPriceFormatted => string.Format(_priceFormat, _depthPrice);//_depthPrice.ToString(_priceFormat);

    private decimal _depthAsk;
    public decimal DepthAsk
    {
        get => _depthAsk;
        set
        {
            if (_depthAsk == value)
            {
                return;
            }

            _depthAsk = value;
            OnPropertyChanged(nameof(DepthAsk));
            OnPropertyChanged(nameof(DepthAskFormatted));
        }
    }
    public string DepthAskFormatted
    {
        get
        {
            if (_depthAsk == 0)
            {
                return "";
            }
            else
            {
                return _depthAsk.ToString();
            }
        }
    }

    private bool _isLTP;
    public bool IsLTP
    {
        get => _isLTP;
        set
        {
            if (_isLTP == value)
            {
                return;
            }

            _isLTP = value;
            OnPropertyChanged(nameof(IsLTP));

        }
    }

    private bool _isAskBest;
    public bool IsAskBest
    {
        get => _isAskBest;
        set
        {
            if (_isAskBest == value)
            {
                return;
            }

            _isAskBest = value;

            OnPropertyChanged(nameof(IsAskBest));
        }
    }
    private bool _isBidBest;
    public bool IsBidBest
    {
        get => _isBidBest;
        set
        {
            if (_isBidBest == value)
            {
                return;
            }

            _isBidBest = value;
            OnPropertyChanged(nameof(IsBidBest));

        }
    }

    public Depth(string priceFormat)
    {
        _priceFormat = priceFormat;
    }

    public Depth()
    {
    }
}

public partial class DepthListItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? LtpTemplate { get; set; }
    public DataTemplate? BidOrAskTemplate { get; set; }
    protected override DataTemplate SelectTemplateCore(object item)
    {
        return base.SelectTemplateCore(item, null);
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        var explorerItem = (Depth)item;
        return explorerItem.IsLTP ? LtpTemplate : BidOrAskTemplate;

        //return base.SelectTemplateCore(item, null); 
    }
}

public class DepthResult
{
    public bool IsSuccess
    {
        get; set;
    }
    public int ErrorCode
    {
        get; set;
    }

    public List<Depth> DepthAskList;
    public List<Depth> DepthBidList;

    public DepthResult()
    {
        DepthAskList = [];
        DepthBidList = [];
    }
}
