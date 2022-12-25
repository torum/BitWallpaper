using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitWallpaper4.ViewModels;

namespace BitWallpaper4.Models;

public class Transaction : ViewModelBase
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
            this.NotifyPropertyChanged("PriceFormat");
            this.NotifyPropertyChanged("PriceFormatted");
        }
    }

    private long _transactionId;
    public long TransactionId
    {
        get
        {
            return _transactionId;
        }
        set
        {
            if (_transactionId == value)
                return;

            _transactionId = value;
            this.NotifyPropertyChanged("TransactionId");

        }
    }

    private string _side;
    public string Side
    {
        get
        {
            return _side;
        }
        set
        {
            if (_side == value)
                return;

            _side = value;
            this.NotifyPropertyChanged("Side");

        }
    }

    public decimal _price;
    public decimal Price
    {
        get
        {
            return _price;
        }
        set
        {
            if (_price == value)
                return;

            _price = value;
            this.NotifyPropertyChanged("Price");
            this.NotifyPropertyChanged("PriceFormatted");
        }
    }

    public string PriceFormatted
    {
        get => String.Format(_priceFormat, _price);
    }
    //PriceFormated

    private decimal _amount;
    public decimal Amount
    {
        get
        {
            return _amount;
        }
        set
        {
            if (_amount == value)
                return;

            _amount = value;
            this.NotifyPropertyChanged("Amount");

        }
    }

    private DateTime _executedAt;
    public DateTime ExecutedAt
    {
        get
        {
            return _executedAt;
        }
        set
        {
            if (_executedAt == value)
                return;

            _executedAt = value;
            this.NotifyPropertyChanged("ExecutedAt");
            this.NotifyPropertyChanged("ExecutedAtFormatted");
        }
    }

    public string ExecutedAtFormatted
    {
        get => _executedAt.ToString("HH:mm:ss");
    }

    public Transaction(string priceFormat)
    {
        _priceFormat = priceFormat;
    }
    public Transaction()
    {
    }
}

public class TransactionsResult
{
    public bool IsSuccess
    {
        get; set;
    }
    public int ErrorCode
    {
        get; set;
    }

    public List<Transaction> Trans;

    public TransactionsResult()
    {
        Trans = new List<Transaction>();
    }
}

