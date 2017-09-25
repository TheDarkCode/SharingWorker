using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("Nubiles-Porn_")]
    class NubilesPorn : NPNetwork
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("Nubiles-Porn_", "Nubiles-Porn", "http://nubiles-porn.com", id);
        }
    }

    abstract class NPNetwork : IVideoInfo
    {
        public abstract Task<VideoInfo> GetInfo(string id);

        protected async Task<VideoInfo> GetInfo(string prefix, string brand, string brandUrl, string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace(prefix, string.Empty);
            var url = string.Format("{0}/video/watch/{1}", brandUrl, num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<span class=\"wp-title videotitle\">";
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf("</span>", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "<span class=\"featuring-modelname model\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd(',', ' ');
                            ret.Actresses += string.Format("{0}, ", HttpUtility.HtmlDecode(actress));
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[{0}] {1} - {2}", brand, title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
