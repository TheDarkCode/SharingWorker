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
    public class PixSense : PropertyChangedBase, IImageHost
    {
        //class LoginStatus
        //{
        //    public string status { get; set; }
        //    public string user_id { get; set; }
        //}

        class UploadStatus
        {
            public string data { get; set; }
            public string image { get; set; }
            public string status { get; set; }
        }

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

        public PixSense()
        {
            Name = "PixSense";
        }

        public bool LoadConfig()
        {
            var config = ConfigurationManager.GetSection("PixSense") as NameValueCollection;
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
                CookieContainer = cookies, AutomaticDecompression = DecompressionMethods.GZip
                //Proxy = new WebProxy("proxy.hinet.net:80"),
                //UseProxy = true,
            })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Referrer = new Uri("http://www.pixsense.net/");
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                };
                client.DefaultRequestHeaders.Add("DNT", "1");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", User),
                    new KeyValuePair<string, string>("password", Password),
                    new KeyValuePair<string, string>("remember", "true"),
                });

                using (var response = await client.PostAsync("http://www.pixsense.net/site/index", content))
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (result.IndexOf("logout", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        LoggedIn = true;
                    }
                    //var login = JsonConvert.DeserializeObject<LoginStatus>(result);
                    //if (login != null)
                    //{
                    //    if(!login.status.Contains("error") && !string.IsNullOrEmpty(login.user_id))
                    //        LoggedIn = true;
                    //}
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
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                        client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0");
                        client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                        client.DefaultRequestHeaders.Add("DNT", "1");
                        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

                        const string chars = "abcdefghijklmnopqrstuvwxyz";
                        var random = new Random();
                        var boundary = new string(
                            Enumerable.Repeat(chars, 14)
                                .Select(s => s[random.Next(s.Length)])
                                .ToArray());

                        using (var content = new MultipartFormDataContent(String.Format("---------------------------{0}", boundary)))
                        {
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("on")), "\"upload\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("1")), "\"domain\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("1")), "\"Image[adult]\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("0")), "\"optimized\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("793")), "\"Image[gallery]\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("11")), "\"resize\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("")), "\"resizeWidth\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("")), "\"resizeHeight\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("normal")), "\"UploadType\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("blank")), "\"GalleryCode\"");

                            var imageContent = new ByteArrayContent(image.Data);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                            content.Add(imageContent, "\"upl\"");
                            imageContent.Headers.ContentDisposition.FileName = image.Name;
                            
                            using (var message = await client.PostAsync("http://www.pixsense.net/site/imageUpload", content))
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
            var json = JsonConvert.DeserializeObject<UploadStatus>(response);

            if (json != null && json.status == "success")
            {
                links[0] = string.Format("<a href=\"{0}\"><img src=\"{1}\" border=\"0\"></img></a><br />", json.data, json.image);
                links[1] = string.Format("[url={0}][img]{1}[/img][/url]", json.data, json.image);
            }
            return links;
        }
    }
}
