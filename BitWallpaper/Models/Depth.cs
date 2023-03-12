using System.Collections.Generic;
using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace BitWallpaper.Models;

public class Depth : ViewModelBase
{
    private string _priceFormat = "";
    public string PriceFormat
    {
        get
        {
            return _priceFormat;
        }
        set
        {
            if (_priceFormat == value)
                return;

            _priceFormat = value;
            NotifyPropertyChanged(nameof(PriceFormat));
            NotifyPropertyChanged(nameof(DepthPriceFormatted));
        }
    }

    private decimal _depthBid;
    public decimal DepthBid
    {
        get
        {
            return _depthBid;
        }
        set
        {
            if (_depthBid == value)
                return;

            _depthBid = value;
            NotifyPropertyChanged(nameof(DepthBid));
            NotifyPropertyChanged(nameof(DepthBidFormatted));
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
        get
        {
            return _depthPrice;
        }
        set
        {
            if (_depthPrice == value)
                return;

            _depthPrice = value;
            NotifyPropertyChanged(nameof(DepthPrice));
            NotifyPropertyChanged(nameof(DepthPriceFormatted));
        }
    }
    public string DepthPriceFormatted
    {
        get => string.Format(_priceFormat, _depthPrice);//_depthPrice.ToString(_priceFormat);
    }

    private decimal _depthAsk;
    public decimal DepthAsk
    {
        get
        {
            return _depthAsk;
        }
        set
        {
            if (_depthAsk == value)
                return;

            _depthAsk = value;
            NotifyPropertyChanged(nameof(DepthAsk));
            NotifyPropertyChanged(nameof(DepthAskFormatted));
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
        get
        {
            return _isLTP;
        }
        set
        {
            if (_isLTP == value)
                return;

            _isLTP = value;
            NotifyPropertyChanged(nameof(IsLTP));

        }
    }

    private bool _isAskBest;
    public bool IsAskBest
    {
        get
        {
            return _isAskBest;
        }
        set
        {
            if (_isAskBest == value)
                return;

            _isAskBest = value;

            NotifyPropertyChanged(nameof(IsAskBest));
        }
    }
    private bool _isBidBest;
    public bool IsBidBest
    {
        get
        {
            return _isBidBest;
        }
        set
        {
            if (_isBidBest == value)
                return;

            _isBidBest = value;
            NotifyPropertyChanged(nameof(IsBidBest));

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

public class DepthListItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? LtpTemplate { get; set; }
    public DataTemplate? BidOrAskTemplate { get; set; }
    protected override DataTemplate SelectTemplateCore(object item)
    {
        return base.SelectTemplateCore(item, null);
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        /*
        var explorerItem = (ExplorerItem)item;
        return explorerItem.Type == ExplorerItem.ExplorerItemType.Folder ? FolderTemplate : FileTemplate;
        */
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
        DepthAskList = new List<Depth>();
        DepthBidList = new List<Depth>();
    }
}
