using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("NubileFilms_")]
    sealed class NubileFilms : NFNetwork
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("NubileFilms_", "NubileFilms", "http://nubilefilms.com", id);
        }
    }

    [WesternInfo("NFBusty_")]
    sealed class NFBusty : NFNetwork
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("NFBusty_", "NF Busty", "http://nfbusty.com", id);
        }
    }

    abstract class NFNetwork : IVideoInfo
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
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        ret.Title = Regex.Replace(ret.Title, " - S[\\d]+:E[\\d]+", string.Empty);
                        
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
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0} ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.TrimEnd();

                        ret.Title = string.Format("[{0}] {1} - {2}", brand, ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
