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
using System.Windows;
using Caliburn.Micro;

namespace SharingWorker.ImageHost
{
    class ImageTwist : PropertyChangedBase, IImageHost
    {
        private static CookieContainer cookies;
        private string sess_id;
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

        public ImageTwist()
        {
            Name = "ImageTwist";
        }

        public bool LoadConfig()
        {
            var config = ConfigurationManager.GetSection("ImageTwist") as NameValueCollection;
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
                client.DefaultRequestHeaders.Referrer = new Uri("http://imagetwist.com/login.html");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("op", "login"),
                    new KeyValuePair<string, string>("redirect", "http://imagetwist.com/"),
                    new KeyValuePair<string, string>("login", User),
                    new KeyValuePair<string, string>("password", Password),
                    new KeyValuePair<string, string>("submit_btn", "Login"),
                });

                using (var response = await client.PostAsync("http://imagetwist.com/", content))
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (result.IndexOf("logout", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var responseCookies = cookies.GetCookies(client.BaseAddress);
                        var sess_idCookie = responseCookies["xfss"];
                        if (sess_idCookie != null) sess_id = sess_idCookie.Value;

                        const string find = "srv_tmp_url='http://";
                        var start = result.IndexOf(find, StringComparison.OrdinalIgnoreCase);
                        if (start >= 0)
                        {
                            start += find.Length;
                            var end = result.IndexOf("/tmp'", start, StringComparison.OrdinalIgnoreCase);
                            if (end > start)
                                uploadServer = result.Substring(start, end - start);
                        }

                        if (string.IsNullOrEmpty(uploadServer))
                        {
                            MessageBox.Show(Application.Current.MainWindow, "uploadServer error!");
                            LoggedIn = false;
                        }
                        else
                        {
                            LoggedIn = true;
                        }
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
                    AutomaticDecompression = DecompressionMethods.GZip,
                })
                using (var client = new HttpClient(handler))
                {
                    var results = new List<string>();
                    var webLinks = "";
                    var forumLinks = "";

                    foreach (var image in uploadImages)
                    {
                        client.DefaultRequestHeaders.ExpectContinue = false;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                        client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");
                        client.DefaultRequestHeaders.Add("Origin", "http://imagetwist.com");
                        client.DefaultRequestHeaders.Add("X-Requested-With", "ShockwaveFlash/22.0.0.209");
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

                        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                        var random = new Random();
                        var boundary = new string(
                            Enumerable.Repeat(chars, 30)
                                .Select(s => s[random.Next(s.Length)])
                                .ToArray());

                        using (var content = new MultipartFormDataContent(String.Format("----------{0}", boundary)))
                        {
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes(image.Name)), "\"Filename\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes(sess_id)), "\"sess_id\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("*.jpg;*.jpeg;*.gif;*.png;*.bmp")), "\"fileext\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("300x300")), "\"thumb_size\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("/")), "\"folder\"");
                            
                            var imageContent = new ByteArrayContent(image.Data);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                            content.Add(imageContent, "\"Filedata\"");
                            imageContent.Headers.ContentDisposition.FileName = image.Name;

                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("Submit Query")), "\"Upload\"");

                            App.Logger.Debug("ImgRock Start Upload...");

                            using (var message = await client.PostAsync(string.Format("http://{0}/cgi-bin/up1.cgi", uploadServer), content))
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
            var vars = response.Split(':');
            if (vars.Length >= 4)
            {
                var id = vars[0];
                var num = vars[2];
                var filename = vars[3];

                var url = string.Format("http://imagetwist.com/{0}/{1}", id, filename);
                var thumb = string.Format("http://{0}/th/{1}/{2}.jpg", uploadServer, num, id);
                
                links[0] = string.Format("<a href=\"{0}\" target=\"_blank\"><img src=\"{1}\" border=\"0\"></img></a><br />", url, thumb);
                links[1] = string.Format("[url={0}][img]{1}[/img][/url]", url, thumb);
            }
            return links;
        }
    }
}