using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitWallpaper.Models
{
    public class ShowBalloonEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }
}
