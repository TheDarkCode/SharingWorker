using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;

namespace SharingWorker.UrlShortening
{
    class ShorteSt : UrlShortening
    {
        private static readonly string apiKey = "5a4c6fe40bd647013ff2dadafb0668ba";

        public override string ApiUrl => "https://api.shorte.st/v1/data/url";
        public override string Name => "ShorteSt";

        public override async Task<string> GetLink(string link)
        {
            try
            {
                return await Policy
                    .Handle<Exception>()
                    .OrResult<string>(l => string.IsNullOrEmpty(l) || l.Length > 40)
                    .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)))
                    .ExecuteAsync(async () =>
                    {
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("public-api-token", apiKey);
                            using (var content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("urlToShorten", link),
                            }))
                            using (var response = await client.PutAsync(ApiUrl, content))
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var reply = JsonConvert.DeserializeObject<Reply>(result);
                                return reply.shortenedUrl;
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex.Message, ex);
                return string.Empty;
            }
        }
    }
}
