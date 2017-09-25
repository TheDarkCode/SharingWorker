using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharingWorker.Video.Western
{
    [WesternInfo("KINK_")]
    sealed class KINK : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("KINK_", string.Empty);

            url = string.Format("https://www.kink.com/shoot/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h1 class=\"shoot-title\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start).TrimEnd();
                        search = "<span class=\"names\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start = start + search.Length + 3;
                        end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);
                        var namesStr = responseString.Substring(start, end - start);
                        foreach (var nameStart in namesStr.AllIndexesOf("\">"))
                        {
                            var aStart = nameStart + 2;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[KINK] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
