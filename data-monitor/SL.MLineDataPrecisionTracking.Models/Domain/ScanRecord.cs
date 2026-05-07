using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class ScanRecord : ObservableObject
    {
        public string MarkingNo { get; set; }

        private bool _isHave;
        public bool IsHave
        {
            get => _isHave;
            set => SetProperty(ref _isHave, value);
        }
    }
}
