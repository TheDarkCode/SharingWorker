using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("JJV_")]
    sealed class JulesJordanVideo : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var nums = id.Replace("JJV_", string.Empty).Split('_');
            if (nums.Count() < 2) return ret;

            var queryStr = nums[0];
            var actStr = new StringBuilder();
            for (var i = 0; i < nums[1].Length; i++)
            {
                if (i > 0 && char.IsUpper(nums[1][i])) actStr.Append(" ");
                actStr.Append(nums[1][i]);
            }
            ret.Actresses = actStr.ToString();

            var url = string.Format("http://www.julesjordanvideo.com/movie/{0}/index.html", queryStr);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<strong>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</strong>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        ret.Title = string.Format("[JulesJordan] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
