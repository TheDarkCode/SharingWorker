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
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharingWorker.ImageHost
{
    class ImgRock : PropertyChangedBase, IImageHost
    {
        private static CookieContainer cookies;
        private string sess_id;
        private static string token;
        private static string uploadServer;

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                NotifyOfPropertyChange(() => Enabled);
            }
        }
        private bool loggedIn;
        public bool LoggedIn
        {
            get { return loggedIn; }
            set
            {
                loggedIn = value;
                NotifyOfPropertyChange(() => LoggedIn);
            }
        }
        public string Name { get; set; }
        public string Url { get; set; }
        public string LoginPath { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public ImgRock()
        {
            Name = "ImgRock";
        }

        public bool LoadConfig()
        {
            var config = ConfigurationManager.GetSection("ImgRock") as NameValueCollection;
            if (config == null) return false;

            try
            {
                Enabled = bool.Parse(config["Enabled"]);
                Url = config["Url"];
                LoginPath = config["LoginPath"];
                User = config["User"];
                Password = config["Password"];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task LogIn()
        {
            LoggedIn = false;
            cookies = new CookieContainer();

            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                //Proxy = new WebProxy("proxy.hinet.net:80"),
                //UseProxy = true,
            })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Referrer = new Uri("http://imgrock.net/login.html");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("op", "login"),
                    new KeyValuePair<string, string>("redirect", "http://imgrock.net/"),
                    new KeyValuePair<string, string>("login", User),
                    new KeyValuePair<string, string>("password", Password)
                });

                using (var response = await client.PostAsync("http://imgrock.net/", content))
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    if (result.IndexOf("logout", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        LoggedIn = true;

                        var responseCookies = cookies.GetCookies(client.BaseAddress);
                        var sess_idCookie = responseCookies["xfss"];
                        if (sess_idCookie != null) sess_id = sess_idCookie.Value;

                        const string find = "srv_htdocs_url\" value=\"http://";
                        var start = result.IndexOf(find, StringComparison.OrdinalIgnoreCase);
                        if (start >= 0)
                        {
                            start += find.Length;
                            var end = result.IndexOf("\"", start, StringComparison.OrdinalIgnoreCase);
                            if (end > start)
                                uploadServer = result.Substring(start, end - start);
                        }
                    }
                }

                token = null;
                using (var response = await client.GetAsync("http://imgrock.net/?op=my_files"))
                {
                    var result = response.Content.ReadAsStringAsync().Result;

                    const string find = "token=";
                    var start = result.IndexOf(find, StringComparison.OrdinalIgnoreCase);
                    if (start >= 0)
                    {
                        start += find.Length;
                        var end = result.IndexOf("\"", start, StringComparison.OrdinalIgnoreCase);
                        if (end > start)
                            token = result.Substring(start, end - start);
                    }
                }
            }
        }

        public async Task<List<string>> Upload(IEnumerable<UploadImage> uploadImages)
        {
            try
            {
                using (var handler = new HttpClientHandler
                {
                    CookieContainer = cookies,
                    //Proxy = new WebProxy("proxy.hinet.net:80"),
                    //UseProxy = true,
                    //AutomaticDecompression = DecompressionMethods.GZip,
                })
                using (var client = new HttpClient(handler))
                {
                    var results = new List<string>();
                    var webLinks = "";
                    var forumLinks = "";

                    foreach (var image in uploadImages)
                    {
                        client.DefaultRequestHeaders.ExpectContinue = false;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/*"));
                        client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                        client.DefaultRequestHeaders.Add("User-Agent", "Shockwave Flash");
                        client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                        //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                        //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

                        const string chars = "abcdefghijklmnopqrstuvwxyz";
                        var random = new Random();
                        var boundary = new string(
                            Enumerable.Repeat(chars, 30)
                                .Select(s => s[random.Next(s.Length)])
                                .ToArray());

                        using (var content = new MultipartFormDataContent(String.Format("----------{0}", boundary)))
                        {
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes(image.Name)), "\"Filename\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("300x300")), "\"thumb_size\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes(sess_id)), "\"sess_id\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("1")), "\"file_adult\"");

                            var imageContent = new ByteArrayContent(image.Data);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                            content.Add(imageContent, "\"Filedata\"");
                            imageContent.Headers.ContentDisposition.FileName = image.Name;

                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("Submit Query")), "\"Upload\"");

                            if (string.IsNullOrEmpty(uploadServer)) uploadServer = "r01.imgrock.net";
                            using (var message = await client.PostAsync(string.Format("http://{0}/cgi-bin/upload_flash.cgi", uploadServer), content))
                            {
                                var response = await message.Content.ReadAsStringAsync();

                                webLinks += ParseResponse(response)[0];
                                forumLinks += ParseResponse(response)[1];
                            }
                        }
                    }

                    results.Add(webLinks);
                    results.Add(forumLinks);
                    return results;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(Name, ex);
            }
        }

        private string[] ParseResponse(string response)
        {
            var links = new string[2];
            var json = JsonConvert.DeserializeObject(response.TrimStart('(').TrimEnd(')')) as JObject;
            if (json != null)
            {
                var url = "";
                var thumb = "";
                if (json.Property("link_url") != null)
                    url = json.Property("link_url").Value.ToString();
                if (json.Property("thumb_url") != null)
                    thumb = json.Property("thumb_url").Value.ToString();

                links[0] = string.Format("<a href=\"{0}\" target=\"_blank\"><img src=\"{1}\" border=\"0\"></img></a><br />", url, thumb);
                links[1] = string.Format("[url={0}][img]{1}[/img][/url]", url, thumb);
            }
            return links;
        }

        //public static async Task<string> GetImagesCode(string id)
        //{
        //    if (string.IsNullOrEmpty(token)) return string.Empty;
        //    var ret = string.Empty;

        //    using (var handler = new HttpClientHandler { CookieContainer = cookies })
        //    using (var client = new HttpClient(handler))
        //    {
        //        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");
        //        client.DefaultRequestHeaders.ExpectContinue = false;
        //        client.DefaultRequestHeaders.Referrer = new Uri("http://imgrock.net/?op=my_files");
        //        client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");

        //        var content = new FormUrlEncodedContent(new[]
        //        {
        //            new KeyValuePair<string, string>("op", "my_files"),
        //            new KeyValuePair<string, string>("token", token),
        //            new KeyValuePair<string, string>("fld_id", "0"),
        //            new KeyValuePair<string, string>("key", id),
        //            new KeyValuePair<string, string>("create_new_folder", ""),
        //            new KeyValuePair<string, string>("to_folder", ""),
        //        });

        //        var fileIds = new List<string>();
        //        using (var response = await client.PostAsync("http://imgrock.net/", content))
        //        {
        //            var result = response.Content.ReadAsStringAsync().Result;

        //            const string find = "file_id\" value=\"";
        //            foreach (var findStart in result.AllIndexesOf(find))
        //            {
        //                var start = findStart + find.Length;
        //                var end = result.IndexOf("\"", start);
        //                var fileId = result.Substring(start, end - start);

        //                fileIds.Add(fileId);
        //            }
        //        }

        //        if (!fileIds.Any()) return string.Empty;

        //        content.Dispose();
        //        var urlParam = new List<KeyValuePair<string, string>>();
        //        urlParam.Add(new KeyValuePair<string, string>("op", "my_files_export"));
        //        foreach (var fileId in fileIds)
        //        {
        //            urlParam.Add(new KeyValuePair<string, string>("file_id", fileId));
        //        }
        //        content = new FormUrlEncodedContent(urlParam);

        //        using (var response = await client.PostAsync("http://imgrock.net/", content))
        //        {
        //            var result = response.Content.ReadAsStringAsync().Result;

        //            var start = result.IndexOf("HTML code");
        //            if (start >= 0)
        //            {
        //                start = result.IndexOf("<a href=", start);
        //                var end = result.IndexOf("</textarea>", start);
        //                if (end >= 0)
        //                {
        //                    ret = result.Substring(start, end - start).Replace("</a>\"", "</a><br/>");
        //                }
        //            }

        //            return ret;
        //        }
        //    }
        //}
    }
}