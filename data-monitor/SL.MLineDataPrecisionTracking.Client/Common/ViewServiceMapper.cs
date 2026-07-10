using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SL.MLineDataPrecisionTracking.Client.Common
{
    public static class ViewServiceMapper
    {
        public static string[] GetServerNames(Type viewType)
        {
            var attr = viewType.GetCustomAttribute<ViewLinkServerInfoAttribute>();
            return attr?.ServerNames ?? Array.Empty<string>();
        }

        public static ViewMatchMode GetMatchMode(Type viewType)
        {
            var attr = viewType.GetCustomAttribute<ViewLinkServerInfoAttribute>();
            return attr?.MatchMode ?? ViewMatchMode.Any;
        }

        public static bool ShouldShow(Type viewType, ISet<string> enabledServiceTypes)
        {
            var serverNames = GetServerNames(viewType);
            if (serverNames.Length == 0)
                return true;

            var matchMode = GetMatchMode(viewType);

            if (matchMode == ViewMatchMode.All)
                return serverNames.All(s => enabledServiceTypes.Any(e => e == s));
            return serverNames.Any(s => enabledServiceTypes.Any(e => e == s));
        }
    }
}
