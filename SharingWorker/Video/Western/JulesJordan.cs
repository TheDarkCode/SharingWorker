using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("JJ_")]
    sealed class JulesJordan : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("JJ_", string.Empty);
            var queryStr = new StringBuilder();
            for (var i = 0; i < num.Length; i++)
            {
                if (i > 0 && char.IsUpper(num[i])) queryStr.Append(" ");
                queryStr.Append(num[i]);
            }

            url = string.Format("https://www.julesjordan.com/trial/search.php?query={0}", HttpUtility.UrlEncode(queryStr.ToString()));
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);
                var search = "<!-- Title -->";
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += search.Length;
                    search = "\">";
                    start = responseString.IndexOf(search, start, StringComparison.OrdinalIgnoreCase);
                    start += search.Length;
                    var end = responseString.IndexOf("</a>", start, StringComparison.OrdinalIgnoreCase);
                    var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                    search = "<span class=\"update_models\">";
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
                        ret.Actresses += string.Format("{0}, ", actress);
                    }
                    if (ret.Actresses != null)
                        ret.Actresses = ret.Actresses.RemoveEnd(", ");

                    ret.Title = string.Format("[JulesJordan] {0}", title);
                }
            }
            return ret;
        }
    }
}
