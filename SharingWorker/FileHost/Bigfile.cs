using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Upload;
using Newtonsoft.Json;

namespace SharingWorker.FileHost
{
    class BigfileSearchJson
    {
        public List<BigfileUploadJson> Uploads { get; set; }

        public BigfileSearchJson()
        {
            Uploads = new List<BigfileUploadJson>();
        }
    }

    class BigfileUploadJson
    {
        public int DownloadCount { get; set; }
        public int UploadId { get; set; }

        public string UploadFileName { get; set; }
        public string DownloadUrl { get; set; }

        public string DownloadLinkWithFileName
        {
            get { return string.Format("{0}/{1}", DownloadUrl, UploadFileName); }
        }
    }

    static class Bigfile
    {
        private static readonly string user = ((NameValueCollection)ConfigurationManager.GetSection("Bigfile"))["User"];
        private static readonly string password = ((NameValueCollection)ConfigurationManager.GetSection("Bigfile"))["Password"];
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
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.bigfile.to/login.php");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Add("Origin", "https://www.bigfile.to");
                client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("userName", user),
                    new KeyValuePair<string, string>("userPassword", password),
                    new KeyValuePair<string, string>("autoLogin", "on"),
                    new KeyValuePair<string, string>("action__login", "normalLogin"),
                });

                using (var response = await client.PostAsync("https://www.bigfile.to/login.php", content))
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
                CookieContainer = cookies, AutomaticDecompression = DecompressionMethods.GZip
            })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.bigfile.to/filesystem.php");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Add("Origin", "https://www.bigfile.to");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("parent_folder_id", "0"),
                    new KeyValuePair<string, string>("total_folder_count", "0"),
                    new KeyValuePair<string, string>("current_page", "1"),
                    new KeyValuePair<string, string>("sort_field", "0"),
                    new KeyValuePair<string, string>("sort_order", "ASC"),
                    new KeyValuePair<string, string>("extra", "filesPanel"),
                    new KeyValuePair<string, string>("fileNameSearch", filename),
                    new KeyValuePair<string, string>("fileTypeSearch", "1"),
                    new KeyValuePair<string, string>("is_search", "true"),                    
                });

                using (var response = await client.PostAsync("https://www.bigfile.to/file-manager-expand-folder.php", content))
                {
                    var links = new List<string>();
                    var result = await response.Content.ReadAsStringAsync();

                    var jsonSearch = JsonConvert.DeserializeObject<BigfileSearchJson>(result);
                    if (jsonSearch != null)
                    {
                        links.AddRange(jsonSearch.Uploads.Select(u => u.DownloadLinkWithFileName));
                    }
                    return links;
                }
            }
        }
    }
}
