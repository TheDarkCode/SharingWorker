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
    class ImgSpice : PropertyChangedBase, IImageHost
    {
        private static CookieContainer cookies;
        private string sess_id;

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

        public ImgSpice()
        {
            Name = "ImgSpice";
        }

        public bool LoadConfig()
        {
            var config = ConfigurationManager.GetSection("ImgSpice") as NameValueCollection;
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
                CookieContainer = cookies, UseProxy = true,
                Proxy = new WebProxy("proxy.hinet.net", 80),
            })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(Url);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("op", "login"),
                    new KeyValuePair<string, string>("redirect", ""),
                    new KeyValuePair<string, string>("login", User),
                    new KeyValuePair<string, string>("password", Password),
                });

                using (var response = await client.PostAsync(LoginPath, content))
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    if (result.IndexOf("logout", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        LoggedIn = true;

                        var responseCookies = cookies.GetCookies(client.BaseAddress);
                        var sess_idCookie = responseCookies["xfss"];
                        if (sess_idCookie != null) sess_id = sess_idCookie.Value;
                    }
                }
            }
        }

        public async Task<List<string>> Upload(IEnumerable<UploadImage> uploadImages)
        {
            try
            {
                var uploadUrl = await GetUploadUrl();
                if (string.IsNullOrEmpty(uploadUrl))
                    throw new Exception("Upload url not found");

                using (var handler = new HttpClientHandler 
                {
                    CookieContainer = cookies, 
                    AutomaticDecompression = DecompressionMethods.GZip,
                    UseProxy = true,
                    //Proxy = new WebProxy("proxy.hinet.net", 80),
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
                        client.DefaultRequestHeaders.Add("Origin", Url);
                        client.DefaultRequestHeaders.Referrer = new Uri(Url);
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("sdch"));

                        using (var content = new MultipartFormDataContent(String.Format("------------{0:30}", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 30))))
                        {

                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes(image.Name)), "\"Filename\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("0")), "\"to_folder\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes(sess_id)), "\"sess_id\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("250x250")), "\"thumb_size\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("adult")), "\"adult\"");
                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("/")), "\"folder\"");

                            var imageContent = new ByteArrayContent(image.Data);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                            content.Add(imageContent, "\"Filedata\"");
                            imageContent.Headers.ContentDisposition.FileName = image.Name;

                            content.Add(new ByteArrayContent(Encoding.Default.GetBytes("Submit Query")), "\"Upload\"");

                            using (var message = await client.PostAsync(uploadUrl + "/cgi-bin/up_flash.cgi", content))
                            {
                                var response = await message.Content.ReadAsStringAsync();

                                webLinks += ParseResponse(response, uploadUrl)[0];
                                forumLinks += ParseResponse(response, uploadUrl)[1];
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

        private string[] ParseResponse(string response, string uploadUrl)
        {
            var param = response.Split(':');
            var links = new string[2];

            links[0] = string.Format("<a href=\"http://imgspice.com/{0}/{1}.html\" target=\"_blank\"><img src=\"{2}/i/{3}/{4}_t.jpg\" border=\"0\"></img></a>", param[0], param[3], uploadUrl, param[2], param[0]);
            links[1] = string.Format("[URL=http://imgspice.com/{0}/{1}.html][IMG]{2}/i/{3}/{4}_t.jpg[/IMG][/URL]\n", param[0], param[3], uploadUrl, param[2], param[0]);

            return links;
        }

        private async Task<string> GetUploadUrl()
        {
            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseProxy = true,
                //Proxy = new WebProxy("proxy.hinet.net", 80),
            })
            using (var client = new HttpClient(handler))
            {
                using (var message = await client.GetAsync(Url))
                {
                    var response = await message.Content.ReadAsStringAsync();
                    var start = response.IndexOf("srv_tmp_url", 0, StringComparison.Ordinal);
                    if (start < 0) return null;
                    start = response.IndexOf("http://", start, StringComparison.Ordinal);
                    var end = response.IndexOf("/tmp", start, StringComparison.Ordinal);
                    return end - start <= 0 ? null : response.Substring(start, end - start);
                }
            }
        }

        public static async Task<string> GetImagesCode(string id)
        {
            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseProxy = true,
                //Proxy = new WebProxy("proxy.hinet.net", 80),
            })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("http://imgspice.com");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("op", "my_files"),
                    new KeyValuePair<string, string>("fld_id", "0"),
                    new KeyValuePair<string, string>("key", id),
                    new KeyValuePair<string, string>("create_new_folder", ""),
                    new KeyValuePair<string, string>("pub", "1"),
                    new KeyValuePair<string, string>("pub", "1"),
                    new KeyValuePair<string, string>("to_folder", ""),
                    new KeyValuePair<string, string>("domain", ""),
                });

                var fileIds = new List<string>();

                using (var response = await client.PostAsync("/", content))
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    const string search = "file_id\" value=\"";
                    foreach (var s in result.AllIndexesOf(search))
                    {
                        var start = s + search.Length;
                        var end = result.IndexOf("\"", start);
                        if(end - start < 10) 
                            fileIds.Add(result.Substring(start, end - start));
                    }
                }

                if (!fileIds.Any()) return string.Empty;
                content.Dispose();
                
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("op", "my_files"));
                postData.Add(new KeyValuePair<string, string>("fld_id", "0"));
                postData.Add(new KeyValuePair<string, string>("key", id));
                postData.Add(new KeyValuePair<string, string>("create_new_folder", ""));
                foreach (var fileId in fileIds)
                {
                    postData.Add(new KeyValuePair<string, string>("file_id", fileId));
                    postData.Add(new KeyValuePair<string, string>("pub", "1"));
                }
                postData.Add(new KeyValuePair<string, string>("to_folder", ""));
                postData.Add(new KeyValuePair<string, string>("domain", "http://imgspice.com"));
                content = new FormUrlEncodedContent(postData);

                using (var response = await client.PostAsync("/", content))
                {
                    var result = response.Content.ReadAsStringAsync().Result;

                    var start = result.IndexOf("All Images : HTML Codes for Sites", 0);
                    if (start >= 0)
                    {
                        start = result.IndexOf("<a href=", start);
                        var end = result.IndexOf("</textarea>", start);
                        if (end >= 0)
                        {
                            return result.Substring(start, end - start).Replace(Environment.NewLine, "").Replace("\n", "");
                        }
                    }
                }
                return string.Empty;
            }
        }
    }
}
