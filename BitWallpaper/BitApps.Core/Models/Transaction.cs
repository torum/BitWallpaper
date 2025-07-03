namespace BitApps.Core.Models;

public partial class Transaction : ViewModelBase
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
            NotifyPropertyChanged(nameof(PriceFormat));
            NotifyPropertyChanged(nameof(PriceFormatted));
        }
    }

    private long _transactionId;
    public long TransactionId
    {
        get => _transactionId;
        set
        {
            if (_transactionId == value)
            {
                return;
            }

            _transactionId = value;
            NotifyPropertyChanged(nameof(TransactionId));
        }
    }

    private string _side = "";
    public string Side
    {
        get => _side;
        set
        {
            if (_side == value)
            {
                return;
            }

            _side = value;
            NotifyPropertyChanged(nameof(Side));
        }
    }

    public decimal _price;
    public decimal Price
    {
        get => _price;
        set
        {
            if (_price == value)
            {
                return;
            }

            _price = value;
            NotifyPropertyChanged(nameof(Price));
            NotifyPropertyChanged(nameof(PriceFormatted));
        }
    }

    public string PriceFormatted => string.Format(_priceFormat, _price);
    //PriceFormated

    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set
        {
            if (_amount == value)
            {
                return;
            }

            _amount = value;
            NotifyPropertyChanged(nameof(Amount));
        }
    }

    private DateTime _executedAt;
    public DateTime ExecutedAt
    {
        get => _executedAt;
        set
        {
            if (_executedAt == value)
            {
                return;
            }

            _executedAt = value;
            NotifyPropertyChanged(nameof(ExecutedAt));
            NotifyPropertyChanged(nameof(ExecutedAtFormatted));
        }
    }

    public string ExecutedAtFormatted => _executedAt.ToString("HH:mm:ss");

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
        Trans = [];
    }
}

