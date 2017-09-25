using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("MOFOS_")]
    sealed class MOFOS : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("MOFOS_", string.Empty);
            var url = string.Format("https://www.mofos.com/tour/search/?q={0}", num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = string.Format("/tour/scene/{0}", num);
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    var end = responseString.IndexOf("\"", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var videoUrl = responseString.Substring(start, end - start);
                        videoUrl = string.Format("https://www.mofos.com{0}", videoUrl);

                        responseString = await client.GetStringAsync(videoUrl);
                        search = "<h1 class=\"title\">";
                        start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                        if (start >= 0)
                        {
                            start = start + search.Length;
                            end = responseString.IndexOf("</h1>", start, StringComparison.OrdinalIgnoreCase);
                            var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                            search = "<h2>";
                            start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                            start += search.Length;
                            end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);

                            var namesStr = responseString.Substring(start, end - start);
                            search = "<a title=\"Watch ";
                            foreach (var nameStart in namesStr.AllIndexesOf(search))
                            {
                                var aStart = nameStart + search.Length;
                                var aEnd = namesStr.IndexOf(" Profile", nameStart, StringComparison.Ordinal);
                                var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                                ret.Actresses += string.Format("{0}, ", HttpUtility.HtmlDecode(actress));
                            }
                            if (ret.Actresses != null)
                                ret.Actresses = ret.Actresses.RemoveEnd(", ");

                            ret.Title = string.Format("[MOFOS] {0} - {1}", title, ret.Actresses);
                        }
                    }
                }
            }
            return ret;
        }
    }
}
