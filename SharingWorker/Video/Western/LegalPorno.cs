using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("lp_")]
    sealed class LegalPorno : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("lp_", string.Empty);

            url = string.Format("https://www.legalporno.com/search/?query={0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<title>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf(" -\n LegalPorno", start, StringComparison.Ordinal);
                    if (end < 0) return ret;

                    var title = responseString.Substring(start, end - start).TrimStart('\n').TrimEnd();
                    title = HttpUtility.HtmlDecode(title);
                    ret.Actresses = string.Empty;
                    ret.Title = string.Format("[LegalPorno]{0}", title);
                }
            }
            return ret;
        }
    }
}
