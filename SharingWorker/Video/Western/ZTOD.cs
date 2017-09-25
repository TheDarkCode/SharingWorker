using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("ZTOD_")]
    sealed class ZTOD : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("ZTOD_", string.Empty);
            var url = string.Format("http://www.ztod.com/videos/{0}", num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<title>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);

                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf(" Featuring", start, StringComparison.Ordinal);

                    if (end < 0)
                    {
                        response = await client.GetByteArrayAsync(url);
                        responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);
                        start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start = start + search.Length;
                            end = responseString.IndexOf(" Featuring", start, StringComparison.Ordinal);
                        }
                    }

                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        search = "Pornstars:<span class=\"txt-highlight\">";
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

                        ret.Title = string.Format("[ZTOD] {0}", ret.Title);
                    }
                }
            }
            return ret;
        }
    }
}
