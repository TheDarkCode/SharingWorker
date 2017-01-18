using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharingWorker.FileHost
{
    static class Rapidgator
    {
        private static readonly string user = ((NameValueCollection)ConfigurationManager.GetSection("Rapidgator"))["User"];
        private static readonly string password = ((NameValueCollection)ConfigurationManager.GetSection("Rapidgator"))["Password"];
        private static CookieContainer cookies;

        public static bool CheckEnabled;
        public static bool GetEnabled;

        public static async Task<bool> LogIn()
        {
            cookies = new CookieContainer();

            try
            {
                using (var handler = new HttpClientHandler {CookieContainer = cookies})
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");

                    client.DefaultRequestHeaders.Referrer = new Uri("https://rapidgator.net/auth/login");
                    client.DefaultRequestHeaders.ExpectContinue = false;

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("LoginForm[email]", user),
                        new KeyValuePair<string, string>("LoginForm[password]", password),
                        new KeyValuePair<string, string>("LoginForm[rememberMe]", "0"),
                        new KeyValuePair<string, string>("LoginForm[verifyCode]", ""),
                    });

                    using (var response = await client.PostAsync("https://rapidgator.net/auth/login", content))
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result.Contains("logout");
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error("Failed to login to Rapidgator.", ex);
                return false;
            }
        }

        public static async Task<string> GetLinks(string filename)
        {
            try
            {
                using (var handler = new HttpClientHandler {CookieContainer = cookies})
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");

                    client.DefaultRequestHeaders.Referrer = new Uri("http://rapidgator.net/filesystem/index");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                    client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                    client.DefaultRequestHeaders.ExpectContinue = false;

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("search_text", filename),
                    });

                    using (var response = await client.PostAsync("http://rapidgator.net/filesystem/FindFile", content))
                    {
                        var links = new StringBuilder();
                        var result = await response.Content.ReadAsStringAsync();
                        const string findId = "id32&gt;";
                        foreach (var first in result.AllIndexesOf(findId))
                        {
                            var start = first + findId.Length;
                            var end = result.IndexOf(",", start, StringComparison.Ordinal);
                            var id = result.Substring(start, end - start);

                            var findName = "name&gt;";
                            start = result.IndexOf(findName, end, StringComparison.Ordinal);
                            start = start + findName.Length;
                            end = result.IndexOf(",", start, StringComparison.Ordinal);
                            var name = result.Substring(start, end - start);

                            var link = string.Format("http://rapidgator.net/file/{0}/{1}.html", id, name);
                            links.AppendLine(link);
                        }
                        return links.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error("Failed to get links from Rapidgator.", ex);
                return string.Empty;
            }
        }
    }
}
