using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharingWorker.MailHost
{
    [Export(typeof(IMailHost))]
    class Nada : MailHostBase
    {
        private class Mail
        {
            public string uid { get; set; }
            public string ib { get; set; }
            public string f { get; set; }
            public string s { get; set; }
            public string d { get; set; }
            public string r { get; set; }
        }
        
        public override string Name => "Nada";

        public override async Task<string> GetMailAddress()
        {
            var mailAddress = await base.GetMailAddress();

            using (Process.Start(string.Format("https://app.getnada.com/inbox/{0}", mailAddress)))
            {
                return mailAddress;
            }
        }

        public override async Task<IEnumerable<string>> GetAvailiableDomains()
        {
            try
            {
                using (var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip
                })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    client.DefaultRequestHeaders.Referrer = new Uri("https://app.getnada.com/inbox/");

                    var responseString = await client.GetStringAsync("https://app.getnada.com/api/v1/domains");
                    var result = JsonConvert.DeserializeObject<JArray>(responseString);
                    var domains = result.Values().SelectMany(v => v.Values<string>().Select(addr => "@" + addr));
                    return domains.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get available domains from Nada!", ex);
                return Enumerable.Empty<string>();
            }
        }

        public override async Task<string> GetMegaSignupMail(string mailAddress)
        {
            var ret = string.Empty;
            try
            {
                using (var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip
                })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    client.DefaultRequestHeaders.Referrer = new Uri("https://app.getnada.com/inbox/");

                    var responseString = await client
                        .GetStringAsync(string.Format("https://app.getnada.com/api/v1/inboxes/{0}", mailAddress))
                        .ConfigureAwait(false);

                    var mails = JsonConvert.DeserializeObject<List<Mail>>(responseString);
                    var megaMail = mails.FirstOrDefault(m => m.f.IndexOf("MEGA", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (string.IsNullOrEmpty(megaMail?.uid)) return ret;
                        
                    responseString = await client.GetStringAsync(string.Format("https://app.getnada.com/api/v1/messages/{0}", megaMail.uid)).ConfigureAwait(false);
                    return responseString;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get mails from Nada!", ex);
                return ret;
            }
        }
    }
}
