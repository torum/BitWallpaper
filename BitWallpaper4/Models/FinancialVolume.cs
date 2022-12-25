using BitWallpaper4.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitWallpaper4.Models
{
    internal class FinancialVolume : ViewModelBase
    {
        private double? _volume;

        public double? Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
                this.NotifyPropertyChanged(nameof(Volume));
            }
        }

        private DateTime? _date;

        public DateTime? Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                this.NotifyPropertyChanged(nameof(Date));
            }
        }
    }
}
