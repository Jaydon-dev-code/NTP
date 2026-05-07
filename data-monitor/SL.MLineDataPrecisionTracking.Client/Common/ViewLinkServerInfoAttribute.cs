using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.Common
{
    public class ViewLinkServerInfoAttribute : Attribute
    {
        public ViewLinkServerInfoAttribute(string[] serverName, string heard, string icon)
        {
            _serversName = serverName;
            _header = heard;
            _icon = icon;
        }

        public string[] _serversName { get; set; }

        public string[] ServiceType
        {
            get => _serversName;
        }

        public string _header { get; set; }
        public string Header
        {
            get => _header;
        }

        public string _icon { get; set; }
        public string Icon
        {
            get => _icon;
        }
    }
}
