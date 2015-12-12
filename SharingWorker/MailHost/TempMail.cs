using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharingWorker.MailHost
{
    // API: http://api.temp-mail.ru/
    static class TempMail
    {
        private static readonly Random Rnd = new Random();

        public static async Task<string> GetMailAddress()
        {
            var account = GenerateAccount();
            var domain = await GetAvailiableDomains().ConfigureAwait(false);
            if (!domain.Any()) return null;
            return account + domain.Random();
        }

        public static async Task<string> GetMegaSignupMail(string mailAddress)
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

                    using (var response = await client.GetAsync(string.Format("http://temp-mail.org/source/{0}", mailId)).ConfigureAwait(false))
                    {
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error("Failed to get mails from TempMail!", ex);
                return ret;
            }
        }

        private static string GenerateAccount()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var ret = new StringBuilder();
            ret.Append(chars.Where(char.IsLetter).Random());
            for (int i = 0; i < Rnd.Next(6,9); i++)
            {
                ret.Append(chars.Random());
            }
            return ret.ToString();
        }

        private static async Task<List<string>> GetAvailiableDomains()
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
                App.Logger.Error("Failed to get available domains from TempMail!", ex);
                return ret;
            }
        }

        private static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash. 
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }
    }
}
