using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("TE_")]
    sealed class TeenErotica : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("TE_", string.Empty);
            var queryStr = new StringBuilder();
            for (var i = 0; i < num.Length; i++)
            {
                if (i > 0 && char.IsUpper(num[i])) queryStr.Append(" ");
                queryStr.Append(num[i]);
            }

            var url = string.Format("http://teenerotica.xxx/videos/?q={0}", queryStr.ToString().Replace(" ", "+"));
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = string.Format("<h2>{0}", queryStr);
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += 4;
                    var end = responseString.IndexOf("</h2>", start, StringComparison.OrdinalIgnoreCase);
                    var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                    search = "<div class=\"updateModels\">";
                    start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                    start += search.Length;
                    end = responseString.IndexOf("</div>", start, StringComparison.Ordinal);

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

                    ret.Title = string.Format("[TeenErotica] {0} - {1}", title, ret.Actresses);
                }
            }
            return ret;
        }
    }
}
