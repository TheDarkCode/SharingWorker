using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("EA_")]
    sealed class EvilAngel : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("EA_", string.Empty);
            var url = string.Format("http://www.evilangel.com/en/search/{0}?query={0}", num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<div class=\"tlcTitle\">";
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += search.Length;
                    search = "\">";
                    start = responseString.IndexOf(search, start, StringComparison.OrdinalIgnoreCase);
                    start += search.Length;

                    var end = responseString.IndexOf("</a>", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "<div class=\"tlcActors\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        search = "</span>";
                        start = responseString.IndexOf(search, start, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</div>", start, StringComparison.Ordinal);

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

                        ret.Title = string.Format("[EvilAngel] {0} - {1}", title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
