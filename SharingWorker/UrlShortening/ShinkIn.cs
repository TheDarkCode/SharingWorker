using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Polly;

namespace SharingWorker.UrlShortening
{
    [Export(typeof(IUrlShortening))]
    class ShinkIn : UrlShortening
    {
        public override string ApiUrl => "http://shink.me/stxt/0/id/90552/auth_token/VEFsRf?s=";
        public override string Name => "ShinkIn";

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
                            var result =
                                await client.GetStringAsync(
                                    string.Format("{0}{1}", ApiUrl, HttpUtility.UrlEncode(link)));
                            return result.TrimStart(Environment.NewLine.ToCharArray());
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
