using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharingWorker.Video
{
    static class SiroutoDouga
    {
        private static Dictionary<string, string> idUrls = new Dictionary<string, string>()
        {
            { "SIRO-", "siro" },
            { "200GANA-", "gana" },
            { "259LUXU-", "lux" },
            { "261ARA-", "ara" },
        };

        public static bool Match(string id)
        {
            return idUrls.Keys.Any(id.StartsWith);
        }

        public static async Task<VideoInfo> GetInfo(string id)
        {
            var task = idUrls.First(p => id.StartsWith(p.Key));
            return await GetInfo(id, task.Key);
        }

        private static async Task<VideoInfo> GetInfo(string id, string key)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace(key, string.Empty);

            url = string.Format("http://sirouto-douga.1000.tv/{0}{1}.php", idUrls[key], num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var bytes = Encoding.UTF8.GetBytes("タイトル：");
                var search = Encoding.UTF8.GetString(bytes);

                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</li>", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var title = responseString.Substring(start, end - start);

                        bytes = Encoding.UTF8.GetBytes("出演者：");
                        search = Encoding.UTF8.GetString(bytes);
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start = start + search.Length;
                            end = responseString.IndexOf("</li>", start, StringComparison.Ordinal);
                            ret.Actresses = responseString.Substring(start, end - start);
                            ret.Title = string.Format("{0}{1} {2} {3}", key, num, title, ret.Actresses);
                        }
                    }
                }
            }
            return ret;
        }
    }
}
