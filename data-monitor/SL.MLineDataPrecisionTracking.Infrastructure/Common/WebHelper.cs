using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Common
{
    public class WebHelper
    {
        public static bool IsHaveWebHtml() =>
            File.Exists(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StaticHtml", "index.html")
            );
    }
}
