using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("DDF_")]
    sealed class DDFNetwork : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("DDF_", string.Empty);
            var url = string.Format("https://ddfnetwork.com/scenes/view/{0}", num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<h1>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start < 0) return ret;
                start += search.Length;
                var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                if (end >= 0)
                {
                    ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                    search = "<h2 class=\"actors\">";
                    start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                    start += search.Length;
                    end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);

                    var namesStr = responseString.Substring(start, end - start);
                    search = "\">";
                    foreach (var nameStart in namesStr.AllIndexesOf(search))
                    {
                        var aStart = nameStart + search.Length;
                        var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                        var actress = namesStr.Substring(aStart, aEnd - aStart).Replace(". ", string.Empty);
                        ret.Actresses += string.Format("{0}, ", actress);
                    }
                    if (ret.Actresses != null)
                        ret.Actresses = ret.Actresses.RemoveEnd(", ");

                    ret.Title = string.Format("[DDFNetwork] {0} - {1}", ret.Title, ret.Actresses);
                }
            }
            return ret;
        }
    }
}
