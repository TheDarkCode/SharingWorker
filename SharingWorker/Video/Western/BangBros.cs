using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("BB_")]
    class BangBros : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("BB_", string.Empty);

            url = string.Format("https://bangbrothers.com/video{0}/a", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<div class=\"ps-vdoHdd\"><h1>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "Cast:";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</div>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        foreach (var nameStart in namesStr.AllIndexesOf("\">"))
                        {
                            var aStart = nameStart + 2;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[BangBros] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
