using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SharingWorker.FileHost
{
    // API: http://mailinator.com/apidocs.jsp
    static class Mailinator
    {
        private static readonly Random Rnd = new Random();
        private const string ApiToken = "621cd9852ebd40f48a7347b71aed2eb0";

        private static readonly List<string> AvailiableDomains = new List<string>
        {
            "@mailinator.com",
            "@mailtothis.com",
            "@reallymymail.com",
            "@monumentmail.com",
            "@mailinator.net",
            "@veryrealemail.com",
            "@mailismagic.com",
            "@spamhereplease.com",
            "@mailinator2.com",
            "@letthemeatspam.com",
        };

        public static string GetMailAddress()
        {
            var account = GenerateAccount();
            return account + AvailiableDomains.Random();
        }

        public static async Task<string> GetMegaSignupMail(string mailAddress)
        {
            var ret = string.Empty;
            var account = mailAddress.Remove(mailAddress.IndexOf("@", 0, StringComparison.Ordinal));
            try
            {
                var mailId = string.Empty;
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(string.Format("https://api.mailinator.com/api/inbox?to={0}&token={1}", account, ApiToken)).ConfigureAwait(false))
                    {
                        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var mails = (JArray)JObject.Parse(result).Property("messages").Value;
                        var megaSignupMail = mails.Children<JObject>().FirstOrDefault(s => s.Property("fromfull").Value.ToString().IndexOf("mega", StringComparison.OrdinalIgnoreCase) >= 0);
                        if (megaSignupMail != null)
                            mailId = megaSignupMail.Property("id").Value.ToString();
                    }
                    if (string.IsNullOrEmpty(mailId)) return ret;

                    using (var response = await client.GetAsync(string.Format("https://api.mailinator.com/api/email?msgid={0}&token={1}", mailId, ApiToken)).ConfigureAwait(false))
                    {
                        return (await response.Content.ReadAsStringAsync().ConfigureAwait(false)).Replace("\\", "");
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error("Failed to get mails from Mailinator!", ex);
                return ret;
            }
        }

        private static string GenerateAccount()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var ret = new StringBuilder();
            ret.Append(chars.Where(char.IsLetter).Random());
            for (int i = 0; i < Rnd.Next(6, 9); i++)
            {
                ret.Append(chars.Random());
            }
            return ret.ToString();
        }
    }
}
