using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SharingWorker.Video.Western
{
    [WesternInfo("HushPass_")]
    sealed class HushPass : HushHushEntertainment
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("HushPass_", "HushPass", "https://hushpass.com", id);
        }
    }

    [WesternInfo("IPass_")]
    sealed class InterracialPass : HushHushEntertainment
    {
        public override async Task<VideoInfo> GetInfo(string id)
        {
            return await GetInfo("IPass_", "InterracialPass", "https://interracialpass.com", id);
        }
    }

    abstract class HushHushEntertainment : IVideoInfo
    {
        public abstract Task<VideoInfo> GetInfo(string id);

        protected async Task<VideoInfo> GetInfo(string prefix, string brand, string brandUrl, string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace(prefix, string.Empty);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                client.DefaultRequestHeaders.Referrer = new Uri(string.Format("{0}/video/{1}.html", brandUrl, num));

                var content = new StringContent(string.Format("{{\"method\":\"JSONP\",\"headers\":{{\"Content-Type\":\"application/x-www-form-urlencoded;charset=utf-8;\"}},\"slug\":\"{0}.html\"}}", num), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(string.Format("{0}/administrator/admin/{1}/api/getVideobyUrl", brandUrl, brand.ToLower()), content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic result = JObject.Parse(responseString);
                ret.Title = result.video.title;
                ret.Actresses = result.video.scenes_artist_name;
                ret.Title = string.Format("[{0}] {1} - {2}", brand, ret.Title, ret.Actresses);
            }
            return ret;
        }
    }
}
