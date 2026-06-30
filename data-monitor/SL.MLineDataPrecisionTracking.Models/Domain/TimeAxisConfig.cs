using CommunityToolkit.Mvvm.ComponentModel;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class TimeAxisConfig : ObservableObject
    {
        private double _time;
        public double Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private double _lowerLimit;
        public double LowerLimit
        {
            get => _lowerLimit;
            set => SetProperty(ref _lowerLimit, value);
        }

        private double _upperLimit;
        public double UpperLimit
        {
            get => _upperLimit;
            set => SetProperty(ref _upperLimit, value);
        }
    }
}
