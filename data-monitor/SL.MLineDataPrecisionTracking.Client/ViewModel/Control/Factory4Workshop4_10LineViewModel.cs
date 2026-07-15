using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public class Factory4Workshop4_10LineViewModel : ObservableObject
    {
        private bool _isOnlieClearance;
        public bool IsOnlieClearance
        {
            get => _isOnlieClearance;
            set => SetProperty(ref _isOnlieClearance, value);
        }

        private bool _isOnlieRivetAndCrack;
        public bool IsOnlieRivetAndCrack
        {
            get => _isOnlieRivetAndCrack;
            set => SetProperty(ref _isOnlieRivetAndCrack, value);
        }

        private bool _isOnlieVib;
        public bool IsOnlieVib
        {
            get => _isOnlieVib;
            set => SetProperty(ref _isOnlieVib, value);
        }

        private bool _isOnlieABS;
        public bool IsOnlieABS
        {
            get => _isOnlieABS;
            set => SetProperty(ref _isOnlieABS, value);
        }

        public Factory4Workshop4_10LineViewModel(HubClien hubClien)
        {
            UpValue(hubClien);
        }

        void UpValue(HubClien hubClien)
        {
            // 判断是否在线
            hubClien.Start<bool>(nameof(IsOnlieClearance), x => IsOnlieClearance = x);
            hubClien.Start<bool>(nameof(IsOnlieRivetAndCrack), x => IsOnlieRivetAndCrack = x);
            hubClien.Start<bool>(nameof(IsOnlieVib), x => IsOnlieVib = x);
            hubClien.Start<bool>(nameof(IsOnlieABS), x => IsOnlieABS = x);
            //接收后台数据
            //hubClien.Start<bool>(nameof(IsOnlieClearance), x => IsOnlieClearance = x);
            //hubClien.Start<bool>(nameof(IsOnlieRivetAndCrack), x => IsOnlieRivetAndCrack = x);
            //hubClien.Start<bool>(nameof(IsOnlieVib), x => IsOnlieVib = x);
            //hubClien.Start<bool>(nameof(IsOnlieABS), x => IsOnlieABS = x);
        }
    }
}
