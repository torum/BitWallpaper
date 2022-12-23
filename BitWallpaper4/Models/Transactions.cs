using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitWallpaper4.ViewModels;

namespace BitWallpaper4.Models;

public class Transactions : ViewModelBase
{
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

        }
    }

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

        }
    }

    public Transactions()
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

    public List<Transactions> Trans;

    public TransactionsResult()
    {
        Trans = new List<Transactions>();
    }
}

