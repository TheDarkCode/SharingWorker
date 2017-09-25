using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("CherryPimps_")]
    sealed class CherryPimps : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("CherryPimps_", string.Empty);
            var url = string.Format("http://www.cherrypimps.com/scenes/{0}_highres.html", num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<h2>";
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf("</h2>", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "<span class=\"update_models\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", HttpUtility.HtmlDecode(actress));
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[CherryPimps] {0} - {1}", title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
