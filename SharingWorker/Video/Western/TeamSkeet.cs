using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video.Western
{
    [WesternInfo("TS_")]
    sealed class TeamSkeet : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("TS_", string.Empty);
            var url = "http://teamskeet.com/t1/search/results/";

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("searchType", ""),
                    new KeyValuePair<string, string>("page", "search"),
                    new KeyValuePair<string, string>("query", num),
                    new KeyValuePair<string, string>("x", "12"),
                    new KeyValuePair<string, string>("y", "12"),
                });
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                var search = string.Format("http://teamskeet.com/t1/trailer/view/{0}", num);
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    var end = responseString.IndexOf("/", start + search.Length, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var videoUrl = responseString.Substring(start, end - start);

                        responseString = await client.GetStringAsync(videoUrl);
                        search = "<title>";
                        start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                        if (start >= 0)
                        {
                            start = start + search.Length;
                            end = responseString.IndexOf("</title>", start, StringComparison.OrdinalIgnoreCase);

                            var titleStrings = responseString.Substring(start, end - start).Split('|');
                            if (titleStrings.Length < 2) return ret;

                            var title = HttpUtility.HtmlDecode(titleStrings[1].Trim());
                            ret.Actresses = HttpUtility.HtmlDecode(titleStrings[0].Trim());

                            ret.Title = string.Format("[TeamSkeet] {0} - {1}", title, ret.Actresses);
                        }
                    }
                }
            }
            return ret;
        }
    }
}
