using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("TUSHY_")]
    sealed class TUSHY : GeneralMediaSystems
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("TUSHY_", "TUSHY", "https://www.tushy.com", id);
        }
    }

    [WesternInfo("VIXEN_")]
    sealed class VIXEN : GeneralMediaSystems
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("VIXEN_", "VIXEN", "https://www.vixen.com", id);
        }
    }

    [WesternInfo("BLACKED_")]
    sealed class BLACKED : GeneralMediaSystems
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("BLACKED_", "BLACKED", "https://www.blacked.com", id);
        }
    }

    abstract class GeneralMediaSystems : IVideoInfo
    {
        public abstract Task<VideoInfo> GetInfo(string id);

        protected async Task<VideoInfo> GetInfo(string prefix, string brand, string brandUrl, string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace(prefix, string.Empty);
            var url = string.Format("{0}/{1}", brandUrl, num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h1 class=\"caption-title\"><a href=\"#\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf("</a>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        start = responseString.IndexOf("Featuring", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("&amp;", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "class=\"ajaxable\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[{0}] {1} - {2}", brand, ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
