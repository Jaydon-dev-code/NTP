using System.ComponentModel;
using System.Runtime.CompilerServices;
using SqlSugar;

namespace SL.ScanData.Models;

[SugarTable("scan_record")]
[SugarIndex("unique_scan_record_Sn", nameof(ScanRecord.Sn), OrderByType.Desc, true)]
public class ScanRecord : INotifyPropertyChanged
{
    private int _displayIndex;

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 200)]
    public string Sn { get; set; } = string.Empty;

    public DateTime ScanTime { get; set; }

    [SugarColumn(IsIgnore = true)]
    public int DisplayIndex
    {
        get => _displayIndex;
        set
        {
            if (_displayIndex == value) return;
            _displayIndex = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
