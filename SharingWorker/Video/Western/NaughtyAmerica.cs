using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("NA_")]
    sealed class NaughtyAmerica : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("NA_", string.Empty);

            url = string.Format("http://tour.naughtyamerica.com/scene/{0}", num);
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
                    search = " in ";
                    var end = responseString.IndexOf(search, start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        start = end + search.Length;
                        end = responseString.IndexOf(" -", start, StringComparison.Ordinal);
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        start = responseString.IndexOf("<h1>", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);

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


                        ret.Title = string.Format("[NaughtyAmerica] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
