using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharingWorker.Video.Western
{
    [WesternInfo("21Sextury_")]
    class _21Sextury : _21Members
    {
    }

    [WesternInfo("21Naturals_")]
    class _21Naturals : _21Members
    {
    }

    class _21Members : IVideoInfo
    {
        public virtual async Task<VideoInfo> GetInfo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var brand = id.Split('_')[0];
            var num = id.Split('_')[1];

            url = string.Format("http://21eroticanal.21naturals.com/en/join/{0}/", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h2 class=\"title\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf(": ", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                        start = end + 2;
                        end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);

                        ret.Actresses = responseString.Substring(start, end - start);
                        ret.Title = string.Format("[{0}] {1} - {2}", brand, ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }
    }
}
