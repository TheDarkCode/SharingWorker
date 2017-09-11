using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NLog;

namespace SharingWorker.FileHost
{
    static class UploadGIG
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static CookieContainer cookies;

        public static string User;
        public static string Password;
        
        public static bool CheckEnabled;
        public static bool GetEnabled;

        static UploadGIG()
        {
            var sec = ConfigurationManager.GetSection("UploadGIG");
            if (sec == null) return;
            User = ((NameValueCollection)sec)["User"];
            Password = ((NameValueCollection)sec)["Password"];
        }
        
        public static async Task<bool> LogIn()
        {
            cookies = new CookieContainer();

            try
            {
                using (var handler = new HttpClientHandler
                {
                    CookieContainer = cookies,
                    AutomaticDecompression = DecompressionMethods.GZip
                })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");

                    client.DefaultRequestHeaders.Referrer = new Uri("https://uploadgig.com");
                    client.DefaultRequestHeaders.ExpectContinue = false;
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

                    var responseString = await client.GetStringAsync("https://uploadgig.com/login/form");
                    var search = "csrf_tester\" value=\"";
                    var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                    if (start < 0) return false;
                    start += search.Length;
                    var end = responseString.IndexOf("\"", start, StringComparison.Ordinal);
                    var csrf = responseString.Substring(start, end - start);
                    
                    client.DefaultRequestHeaders.Referrer = new Uri("https://uploadgig.com/register");
                    
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("csrf_tester", csrf),
                        new KeyValuePair<string, string>("email", User),
                        new KeyValuePair<string, string>("pass", Password),
                        new KeyValuePair<string, string>("rememberme", "1"),
                    });

                    using (var response = await client.PostAsync("https://uploadgig.com/login/do_login", content))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to login to UploadGIG.", ex);
                return false;
            }
        }
        
        public static async Task<List<string>> GetLinks(string filename, bool withFileName = false)
        {
            try
            {
                using (var handler = new HttpClientHandler
                {
                    CookieContainer = cookies,
                    AutomaticDecompression = DecompressionMethods.GZip
                })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");

                    client.DefaultRequestHeaders.Referrer = new Uri("https://uploadgig.com");
                    client.DefaultRequestHeaders.ExpectContinue = false;
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                    
                    var links = new List<string>();
                    var result = await client.GetStringAsync(string.Format("https://uploadgig.com/files/index/name/{0}", filename));
                    
                    const string findId = "<tr id=\"";
                    foreach (var first in result.AllIndexesOf(findId))
                    {
                        var start = first + findId.Length;
                        var end = result.IndexOf("\"", start, StringComparison.Ordinal);
                        var id = result.Substring(start, end - start);

                        string link;
                        if (withFileName)
                        {
                            var findName = string.Format("{0}/", id);
                            start = result.IndexOf(findName, end, StringComparison.Ordinal);
                            start = start + findName.Length;
                            end = result.IndexOf("\"", start, StringComparison.Ordinal);
                            var name = result.Substring(start, end - start);

                            link = string.Format("https://uploadgig.com/file/download/{0}/{1}", id, name);
                        }
                        else
                        {
                            link = string.Format("https://uploadgig.com/file/download/{0}", id);
                        }
                        links.Add(link);
                    }
                    return links;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get links from UploadGIG.", ex);
                return Enumerable.Empty<string>().ToList();
            }
        }
    }
}
