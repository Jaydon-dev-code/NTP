using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class ChartMonitorItem : ObservableObject
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        private string _editingName;
        public string EditingName
        {
            get => _editingName;
            set => SetProperty(ref _editingName, value);
        }

        public ObservableCollection<TimeAxisConfig> Configs { get; set; } = new ObservableCollection<TimeAxisConfig>();
    }
}
