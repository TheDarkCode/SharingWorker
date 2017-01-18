using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace SharingWorker.FileHost
{
    static class Elink
    {
        class Reply
        {
            public string status { get; set; }
            public string shortenedUrl { get; set; }
        }

        private static readonly string apiUrl = ((NameValueCollection)ConfigurationManager.GetSection("Elink"))["ApiUrl"];
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
