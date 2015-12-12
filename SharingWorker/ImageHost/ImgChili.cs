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
using Google.GData.Client;

namespace SharingWorker.ImageHost
{
    class ImgChili : PropertyChangedBase, IImageHost
    {
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

        public ImgChili()
        {
            Name = "imgChili";
        }

        public bool LoadConfig()
        {
            var config = ConfigurationManager.GetSection("imgChili") as NameValueCollection;
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

            using (var handler = new HttpClientHandler {CookieContainer = cookies})
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(Url);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", User),
                    new KeyValuePair<string, string>("password", Password)
                });

                using (var response = await client.PostAsync(LoginPath, content))
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    if (result.Contains("http://imgchili.net/logout"))
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
                using (var handler = new HttpClientHandler {CookieContainer = cookies, AutomaticDecompression = DecompressionMethods.GZip })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.ExpectContinue = false;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    client.DefaultRequestHeaders.Referrer = new Uri(Url);
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                    client.DefaultRequestHeaders.Add("DNT", "1");
                    client.DefaultRequestHeaders.Pragma.Add(new NameValueHeaderValue("no-cache"));

                    using (var content = new MultipartFormDataContent(String.Format("---------------------------{0:14}", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 14))))
                    {
                        foreach (var image in uploadImages)
                        {
                            var imageContent = new ByteArrayContent(image.Data);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                            content.Add(imageContent, "userfile[]", image.Name);
                        }

                        content.Add(new ByteArrayContent(Encoding.Default.GetBytes("300")), "thumbsize");
                        content.Add(new ByteArrayContent(Encoding.Default.GetBytes("1")), "adult");
                        content.Add(new ByteArrayContent(Encoding.Default.GetBytes("0")), "upload_to");
                        content.Add(new ByteArrayContent(Encoding.Default.GetBytes("0")), "private_upload");
                        content.Add(new ByteArrayContent(Encoding.Default.GetBytes("normal-boxed")), "upload_type");
                        content.Add(new ByteArrayContent(Encoding.Default.GetBytes("on")), "tos");

                        using (var message = await client.PostAsync("http://imgchili.net/upload.php", content))
                        {
                            var response = await message.Content.ReadAsStringAsync();
                            return ParseResponse(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(Name, ex);
            }
        }

        private List<string> ParseResponse(string response)
        {
            var links = new List<string>();

            int first = response.IndexOf("Thumbnails for Website", 4900, StringComparison.Ordinal);
            int textarea = response.IndexOf("<textarea", first, StringComparison.Ordinal);
            int start = response.IndexOf(">", textarea, StringComparison.Ordinal) + 1;
            int end = response.IndexOf("</textarea>", start, StringComparison.Ordinal);
            var result = response.Substring(start, end - start);
            links.Add(WebUtility.HtmlDecode(result).TrimStart(' ').Replace("</a>", "</a><br />"));

            first = response.IndexOf("Thumbnails for Forums:", end, StringComparison.Ordinal);
            textarea = response.IndexOf("<textarea", first, StringComparison.Ordinal);
            start = response.IndexOf(">", textarea, StringComparison.Ordinal) + 1;
            end = response.IndexOf("</textarea>", start, StringComparison.Ordinal);
            links.Add(WebUtility.HtmlDecode(response.Substring(start, end - start)).TrimStart(' '));

            return links;
        }

        public static async Task<string> GetImagesCode(string id)
        {
            id = id.Replace("-", "_");
            var ret = "";

            using (var handler = new HttpClientHandler { CookieContainer = cookies })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Referrer = new Uri("http://imgchili.net/gallery/0");

                using (var response = await client.GetAsync(string.Format("http://imgchili.net/gallery/0/s/{0}", id)))
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    const string search = "link3=\"";
                    foreach (var s in result.AllIndexesOf(search).Reverse())
                    {
                        var start = s + search.Length;
                        var end = result.IndexOf("\" /></td>", start);
                        if (end >= 0)
                        {
                            ret += result.Substring(start, end - start);
                        }
                    }
                }
            }
            return HttpUtility.HtmlDecode(ret);
        }
    }
}
