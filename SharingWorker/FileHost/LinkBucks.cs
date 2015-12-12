using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharingWorker.FileHost
{
    public static class LinkBucks
    {
        public static bool GetEnabled;

        private static readonly string singleApiUrl = ((NameValueCollection)ConfigurationManager.GetSection("Linkbucks"))["SingleApiUrl"];
        private static readonly string apiPassword = ((NameValueCollection)ConfigurationManager.GetSection("Linkbucks"))["ApiPassword"];
        private static readonly string user = ((NameValueCollection)ConfigurationManager.GetSection("Linkbucks"))["User"];
        
        class SingleLinkJson
        {
            public string user { get; set; }
            public string apiPassword { get; set; }
            public string originalLink { get; set; }
            public int adType { get; set; }
            public int contentType { get; set; }
            public string domain { get; set; }
        }

        class ErrorJson
        {
            public int errorCode { get; set; }
            public string errorDescription { get; set; }
        }

        class SingleLinkbucksJson
        {
            public int linkId { get; set; }
            public string link { get; set; }
        }

        public static List<string> AliasUrls = new List<string>
        {
            "allanalpass.com",
            "linkbabes.com",
            "freean.us",
            "rqq.co",
            "zff.co",
            "poontown.net",
            "tnabucks.com",
            "fapoff.com",
        };

        public static async Task<string> GetLinkbucksSingle(string title, string link)
        {
            using (var client = new HttpClient())
            {
                var jsonObj = new SingleLinkJson
                {
                    user = user,
                    apiPassword = apiPassword,
                    originalLink = link,
                    adType = 2,
                    contentType = 2,
                    domain = AliasUrls.Random(),
                };

                var json = JsonConvert.SerializeObject(jsonObj);

                using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = await client.PostAsync(singleApiUrl, content))
                {
                    var result = await response.Content.ReadAsStringAsync();

                    var retjsonObj = JsonConvert.DeserializeObject(result) as JObject;
                    if (retjsonObj == null) return string.Empty;
                    var successJsonObj = retjsonObj.ToObject<SingleLinkbucksJson>();
                    if (successJsonObj != null)
                    {
                        return successJsonObj.link;
                    }
                    return string.Empty;
                }
            }
        }
    }
}