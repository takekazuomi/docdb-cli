using System.Collections.Specialized;
using System.Linq;

namespace DocDB
{
    internal static class Extentions
    {
        public static string ToJoinedString(this NameValueCollection nvc, string separator, string valueMark)
        {
            return string.Join(separator, nvc.Cast<string>().Select(s => string.Format("{0}{1}{2}", s, valueMark, nvc[s])).ToArray());
        }
    }
}
