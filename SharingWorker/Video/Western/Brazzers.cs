using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("BZ_")]
    sealed class Brazzers : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("BZ_", string.Empty);

            url = string.Format("http://www.brazzers.com/scenes/view/id/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h1 class=\"scene-title\" itemprop=\"name\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("<span class", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "itemprop=\"name\" title=\"";
                        foreach (var searchStart in responseString.AllIndexesOf(search))
                        {
                            if (searchStart < 0) continue;
                            var aStart = searchStart + search.Length;
                            var aEnd = responseString.IndexOf("\"", aStart, StringComparison.Ordinal);
                            ret.Actresses += string.Format("{0}, ", responseString.Substring(aStart, aEnd - aStart));
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[BRAZZERS] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
