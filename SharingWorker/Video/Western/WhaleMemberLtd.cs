using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("phd_")]
    sealed class PassionHD : WhaleMemberLtd
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("phd_", "passion-hd", "https://passion-hd.com", id);
        }
    }

    [WesternInfo("HOLED_")]
    sealed class HOLED : WhaleMemberLtd
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("HOLED_", "HOLED", "https://holed.com", id);
        }
    }

    [WesternInfo("SPYFAM_")]
    sealed class SPYFAM : WhaleMemberLtd
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("SPYFAM_", "SPYFAM", "https://spyfam.com", id);
        }
    }

    [WesternInfo("LUBED_")]
    sealed class LUBED : WhaleMemberLtd
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("LUBED_", "LUBED", "https://lubed.com", id);
        }
    }

    [WesternInfo("POVD_")]
    sealed class POVD : WhaleMemberLtd
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("POVD_", "POVD", "https://povd.com", id);
        }
    }

    abstract class WhaleMemberLtd : IVideoInfo
    {
        public abstract Task<VideoInfo> GetInfo(string id);

        protected async Task<VideoInfo> GetInfo(string prefix, string brand, string brandUrl, string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace(prefix, string.Empty);
            var url = string.Format("{0}/video/{1}", brandUrl, num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<h3 class=\"text-primary\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h3>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        start = responseString.IndexOf("<p>", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("</p>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
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
