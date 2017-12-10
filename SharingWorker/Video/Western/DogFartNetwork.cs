using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("BlacksOnBlondes_")]
    sealed class BlacksOnBlondes : DogFartNetwork
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("BlacksOnBlondes_", id);
        }
    }

    [WesternInfo("GloryHole_")]
    sealed class GloryHole : DogFartNetwork
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("GloryHole_", id);
        }
    }

    abstract class DogFartNetwork : IVideoInfo
    {
        public abstract Task<VideoInfo> GetInfo(string id);

        protected async Task<VideoInfo> GetInfo(string prefix, string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };

            var brand = prefix.TrimEnd('_');
            var num = id.Replace(prefix, string.Empty);
            var url = $"https://www.dogfartnetwork.com/tour/sites/{brand}/{num}/";

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "class=\"description-title\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "<h4 class=\"more-scenes\">See All Scenes With:";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</h4>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "'>";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[{0}] {1}", brand, ret.Title);
                    }
                }
            }
            return ret;
        }
    }
}
