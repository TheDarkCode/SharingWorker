using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SharingWorker.FileHost
{
    public static class ShorteSt
    {
        class Reply
        {
            public string status { get; set; }
            public string shortenedUrl { get; set; }
        }

        private static readonly string singleApiUrl = ((NameValueCollection)ConfigurationManager.GetSection("ShorteSt"))["ApiUrl"];
        private static readonly string apiKey = ((NameValueCollection)ConfigurationManager.GetSection("ShorteSt"))["ApiKey"];
        public static bool GetEnabled;


        public static async Task<string> GetLink(string link)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("public-api-token", apiKey);
                using (var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("urlToShorten", link),
                }))
                using (var response = await client.PutAsync(singleApiUrl, content))
                {
                    try
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var reply = JsonConvert.DeserializeObject<Reply>(result);
                        return reply.shortenedUrl;
                    }
                    catch (Exception ex)
                    {
                        return string.Empty;
                    }
                }
            }
        }
    }
}
