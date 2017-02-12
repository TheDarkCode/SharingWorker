using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SharingWorker.FileHost
{
    static class Datafile
    {
        private static readonly string user = ((NameValueCollection)ConfigurationManager.GetSection("Datafile"))["User"];
        private static readonly string password = ((NameValueCollection)ConfigurationManager.GetSection("Datafile"))["Password"];
        private static CookieContainer cookies;

        public static bool CheckEnabled;
        public static bool GetEnabled;

        public static async Task<bool> LogIn()
        {
            cookies = new CookieContainer();

            using (var handler = new HttpClientHandler { CookieContainer = cookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.datafile.com/login.html");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Add("Origin", "https://www.datafile.com");
                client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("login", user),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("remember_me", "0"),
                    new KeyValuePair<string, string>("remember_me", "1"),
                });

                using (var response = await client.PostAsync("https://www.datafile.com/login.html", content))
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (result.Contains("logout") || result.IndexOf("log out", StringComparison.OrdinalIgnoreCase) > 0)
                        return true;
                }
            }
            return false;
        }

        public static async Task<List<string>> GetLinks(string filename)
        {
            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                AutomaticDecompression = DecompressionMethods.GZip
            })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
                client.DefaultRequestHeaders.Referrer = new Uri("http://www.datafile.com/files.html");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

                var searchUri =
                    string.Format(
                        "http://www.datafile.com/files.html?folder_id=&filter%5Btype%5D=0&filter%5Bsize%5D=0&filter%5Bumethod%5D=0&filter%5Bdate_from%5D=&filter%5Bdate_to%5D=&filter%5Bsearch%5D={0}&go=",
                        filename);

                using (var response = await client.GetAsync(searchUri))
                {
                    var links = new List<string>();
                    var result = await response.Content.ReadAsStringAsync();

                    const string findId = "<span class=\"name\">";
                    foreach (var first in result.AllIndexesOf(findId))
                    {
                        var start = first + findId.Length;
                        var end = result.IndexOf("</span>", start, StringComparison.Ordinal);
                        var fileName = result.Substring(start, end - start);

                        var findLink = "<span class=\"link\">";
                        start = result.IndexOf(findLink, end, StringComparison.Ordinal);
                        start = start + findLink.Length;
                        end = result.IndexOf("</span>", start, StringComparison.Ordinal);
                        var link = result.Substring(start, end - start);

                        var linkWithFileName = string.Format("{0}/{1}", link, fileName);
                        links.Add(linkWithFileName);
                    }
                    return links;
                }
            }
        }
    }
}
