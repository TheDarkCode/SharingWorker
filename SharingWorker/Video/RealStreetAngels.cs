using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharingWorker.Video
{
    static class RealStreetAngels
    {
        public static bool IsRealStreetAngels(string id)
        {
            if (!id.StartsWith("m") || !id.Contains("_") || id.Length < 4 
                || !char.IsDigit(id[1]) || !char.IsDigit(id[2]) || !char.IsDigit(id[3]))
            {
                return false;
            }
            return true;
        }

        public static async Task<VideoInfo> GetInfo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Split('_')[0];
            
            url = string.Format("http://real2.s-angels.com/teigaku/item.php?ID={0}", id);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<div id=\"header\"><h1>Real Street Angels ";
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h1>", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        ret.Actresses = responseString.Substring(start, end - start).TrimEnd();
                        ret.Title = string.Format("Real Street Angels {0} {1}", num, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
