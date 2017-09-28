using System;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Polly;

namespace SharingWorker.UrlShortening
{
    [Export(typeof(IUrlShortening))]
    class Ouo : UrlShortening
    {
        public override string ApiUrl => ((NameValueCollection)ConfigurationManager.GetSection("Ouo"))["ApiUrl"];
        public override string Name => "Ouo";
        public override bool FirstLinkEnabled => true;

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
                            return await client.GetStringAsync(string.Format("{0}{1}", ApiUrl,
                                HttpUtility.UrlEncode(link)));
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
