using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharingWorker.FileHost
{
    static class Uploadable
    {
        private static readonly string user = ((NameValueCollection)ConfigurationManager.GetSection("Uploadable"))["User"];
        private static readonly string password = ((NameValueCollection)ConfigurationManager.GetSection("Uploadable"))["Password"];
        private static CookieContainer cookies;

        public static bool CheckEnabled;
        public static bool GetEnabled;

        public static async Task<bool> LogIn()
        {
            cookies = new CookieContainer();
            
            using (var handler = new HttpClientHandler { CookieContainer = cookies })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("http://www.uploadable.ch");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("userName", user),
                    new KeyValuePair<string, string>("userPassword", password),
                    new KeyValuePair<string, string>("action__login", "normalLogin"),
                });

                using (var response = await client.PostAsync("/login.php", content))
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    if (result.Contains("logout"))
                        return true;
                }
            }
            return false;
        }

        public static async Task<string> GetLinks(string filename)
        {
            using (var handler = new HttpClientHandler { CookieContainer = cookies })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("http://www.uploadable.ch");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("fileOrFolderSearch", "file"),
                    new KeyValuePair<string, string>("folderNameSearch", "Find Folder..."),
                    new KeyValuePair<string, string>("fileTypeSearch", "1"),
                    new KeyValuePair<string, string>("fileSizeSearch", "All"),
                    new KeyValuePair<string, string>("upperFileSizeSearch", ""),
                    new KeyValuePair<string, string>("lowerFileSizeSearch", ""),
                    new KeyValuePair<string, string>("uploadMethodSearch", ""),
                    new KeyValuePair<string, string>("fileSearchFormSubmit", "Search"),
                    new KeyValuePair<string, string>("startDateSearch", ""),
                    new KeyValuePair<string, string>("endDateSearch", ""),
                    new KeyValuePair<string, string>("fileNameSearch", filename),
                });

                using (var response = await client.PostAsync("/filesystem.php", content))
                {
                    var links = new StringBuilder();
                    var result = response.Content.ReadAsStringAsync().Result;
                    const string find = "<label name='download_link' style='display:none'>";
                    foreach (var first in result.AllIndexesOf(find))
                    {
                        var start = first + find.Length;
                        var end = result.IndexOf(".rar", start, StringComparison.Ordinal) + 4;
                        links.AppendLine(result.Substring(start, end - start));
                    }
                    return links.ToString();
                }
            }
        }

        public static async Task<bool> CheckLinks(IEnumerable<string> links)
        {
            if (!links.Any()) return false;
            var urls = new StringBuilder();
            foreach (var link in links)
            {
                urls.Append(link + Environment.NewLine);
            }
            
            var ret = true;
            try
            {
                using (var handler = new HttpClientHandler())
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("http://www.uploadable.ch");
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("urls", urls.ToString()),
                    });

                    using (var response = await client.PostAsync("/check.php", content))
                    {

                        var result = response.Content.ReadAsStringAsync().Result;
                        const string find = "http://www.uploadable.ch/file/";
                        foreach (var start in result.AllIndexesOf(find))
                        {
                            var end = result.IndexOf("</a>", start, StringComparison.Ordinal);
                            var link = result.Substring(start, end - start);
                            const string search = "<span class=\"left\">";
                            var start1 = result.IndexOf(search, end, StringComparison.Ordinal) + search.Length;
                            end = result.IndexOf("</span>", start1, StringComparison.Ordinal);
                            var status = result.Substring(start1, end - start1);
                            NLog.LogManager.GetLogger("CheckPost").Info("{0} | Status: {1}", link, status);
                            if (status != "Available") ret = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.ErrorException("Failed to check Uploadable links!", ex);
                ret = false;
            }
            return ret;
        }
    }
}
