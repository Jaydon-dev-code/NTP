using System;

namespace SL.MLineDataPrecisionTracking.Client.Common
{
    public enum ViewMatchMode
    {
        Any,
        All
    }

    public class ViewLinkServerInfoAttribute : Attribute
    {
        public string[] ServerNames { get; }
        public string Header { get; }
        public string Icon { get; }
        public ViewMatchMode MatchMode { get; set; } = ViewMatchMode.Any;

        public ViewLinkServerInfoAttribute(string[] serverNames, string header, string icon)
        {
            ServerNames = serverNames;
            Header = header;
            Icon = icon;
        }
    }
}
