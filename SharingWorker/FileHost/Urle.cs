using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace SharingWorker.FileHost
{
    static class Urle
    {
        class Reply
        {
            public string status { get; set; }
            public string shortenedUrl { get; set; }
        }

        private static readonly string apiUrl = ((NameValueCollection)ConfigurationManager.GetSection("Urle"))["ApiUrl"];
        public static bool GetEnabled;

        public static async Task<string> GetLink(string link)
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(string.Format("{0}{1}", apiUrl, HttpUtility.UrlEncode(link))))
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
