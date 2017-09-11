using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharingWorker.MailHost
{
    // API: https://temp-mail.ru/en/api/
    class TempMail : MailHostBase
    {
        public override async Task<string> GetMegaSignupMail(string mailAddress)
        {
            var ret = string.Empty;
            var md5 = GetMd5Hash(mailAddress);
            try
            {
                var mailId = string.Empty;
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(string.Format("http://api.temp-mail.ru/request/mail/id/{0}/format/json/", md5)).ConfigureAwait(false))
                    {
                        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var mails = JArray.Parse(result);
                        var megaSignupMail = mails.Children<JObject>().FirstOrDefault(s => s.Property("mail_from").Value.ToString().IndexOf("mega", StringComparison.OrdinalIgnoreCase) >= 0);
                        if(megaSignupMail != null)
                            mailId = megaSignupMail.Property("mail_id").Value.ToString();
                    }
                    if (string.IsNullOrEmpty(mailId)) return ret;

                    using (var response = await client.GetAsync(string.Format("http://api.temp-mail.ru/request/source/{0}", mailId)).ConfigureAwait(false))
                    {
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get mails from TempMail!", ex);
                return ret;
            }
        }
        
        public override async Task<IEnumerable<string>> GetAvailiableDomains()
        {
            var ret = new List<string>();
            try
            {
                using (var client = new HttpClient())
                using (var response = await client.GetAsync("http://api.temp-mail.ru/request/domains/format/json/").ConfigureAwait(false))
                {
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ret = JsonConvert.DeserializeObject<JArray>(result).Values<string>().ToList();
                    return ret;
                }
            }
            catch(Exception ex)
            {
                logger.Error("Failed to get available domains from TempMail!", ex);
                return ret;
            }
        }
    }
}
