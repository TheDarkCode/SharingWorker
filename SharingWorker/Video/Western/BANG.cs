using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SharingWorker.Video.Western
{
    [WesternInfo("BANG_")]
    sealed class BANG : IVideoInfo
    {
        public async Task<VideoInfo> GetInfo(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("BANG_", string.Empty);
            var url = string.Format("https://5c5366a3b84ecedad405b95dbadea764.us-east-1.aws.found.io/videos/video/{0}", num);

            using (var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "YmFuZy1yZWFkOktqVDN0RzJacmQ1TFNRazI=");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

                var responseString = await client.GetStringAsync(url);

                dynamic result = JObject.Parse(responseString);
                ret.Title = result._source.name;
                foreach (var actor in result._source.actors)
                {
                    ret.Actresses += string.Format("{0}, ", actor.name);
                }
                if (ret.Actresses != null)
                    ret.Actresses = ret.Actresses.RemoveEnd(", ");

                ret.Title = string.Format("[BANG!] {0}", ret.Title.TrimEnd());
            }
            return ret;
        }
    }
}
