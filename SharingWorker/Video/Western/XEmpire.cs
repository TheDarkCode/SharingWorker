using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("HX_")]
    sealed class HardX : XEmpire
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("HX_", "HardX", "http://www.hardx.com", id);
        }
    }

    [WesternInfo("DX_")]
    sealed class DarkX : XEmpire
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("DX_", "DarkX", "http://www.darkx.com", id);
        }
    }

    abstract class XEmpire : IVideoInfo
    {
        public abstract Task<VideoInfo> GetInfo(string id);

        protected async Task<VideoInfo> GetInfo(string prefix, string brand, string brandUrl, string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace(prefix, string.Empty);

            url = string.Format("{0}/en/search/{1}?query={1}", brandUrl, num);
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

                        ret.Title = string.Format("[{0}] {1} - {2}",brand , title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
