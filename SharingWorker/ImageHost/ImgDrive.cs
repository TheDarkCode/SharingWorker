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

namespace SharingWorker.ImageHost
{
    class ImgDrive : PropertyChangedBase, IImageHost
    {
        private static Random rand = new Random();
        private static CookieContainer cookies;

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

        public ImgDrive()
        {
            Name = "ImgDrive";
        }

        public bool LoadConfig()
        {
            var config = ConfigurationManager.GetSection("ImgDrive") as NameValueCollection;
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
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Referrer = new Uri("http://www.imgdrive.net/login.php");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("usr_email", User),
                    new KeyValuePair<string, string>("pwd", Password),
                    new KeyValuePair<string, string>("doLogin", "Login"),
                });

                using (var response = await client.PostAsync("http://www.imgdrive.net/login.php", content))
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (result.IndexOf("logout", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        LoggedIn = true;
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
                    AutomaticDecompression = DecompressionMethods.GZip,
                    //Proxy = new WebProxy("proxy.hinet.net:80"),
                    //UseProxy = true,
                })
                using (var client = new HttpClient(handler))
                {
                    var results = new List<string>();
                    var webLinks = "";
                    var forumLinks = "";

                    foreach (var image in uploadImages)
                    {
                        client.DefaultRequestHeaders.ExpectContinue = false;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                        client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");
                        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                        
                        const string chars = "0123456789";
                        var boundary = new string(
                            Enumerable.Repeat(chars, 15)
                                .Select(s => s[rand.Next(s.Length)])
                                .ToArray());

                        using (var content = new MultipartFormDataContent(string.Format("---------------------------{0}", boundary)))
                        {
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("1")), "adult");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("")), "set_gallery");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("2")), "thumb_size_contaner");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("")), "download_links");

                            var imageContent = new ByteArrayContent(image.Data);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                            content.Add(imageContent, "uploaded");
                            imageContent.Headers.ContentDisposition.FileName = image.Name;

                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("Upload")), "simple_upload");

                            App.Logger.Debug("ImgDrive Start Upload...");

                            using (var request = new HttpRequestMessage(HttpMethod.Post, "http://www.imgdrive.net/upload.php")
                            {
                                Content = content,
                            })
                            using (var message = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
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

            int first = response.IndexOf("All HTML Codes (medium):", 0, StringComparison.Ordinal);
            int textarea = response.IndexOf("<textarea", first, StringComparison.Ordinal);
            int start = response.IndexOf(">", textarea, StringComparison.Ordinal) + 1;
            int end = response.IndexOf("</textarea>", start, StringComparison.Ordinal);
            var result = response.Substring(start, end - start);
            links[0] = WebUtility.HtmlDecode(result).TrimEnd(' ').Replace("</a>", "</a><br />");

            first = response.IndexOf("All BB Codes (medium):", 0, StringComparison.Ordinal);
            textarea = response.IndexOf("<textarea", first, StringComparison.Ordinal);
            start = response.IndexOf(">", textarea, StringComparison.Ordinal) + 1;
            end = response.IndexOf("</textarea>", start, StringComparison.Ordinal);
            links[1] = WebUtility.HtmlDecode(response.Substring(start, end - start)).TrimStart(' ');

            return links;
        }
    }
}