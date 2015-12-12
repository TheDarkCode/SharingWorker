using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Google.GData.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharingWorker.MailHost;

namespace SharingWorker.FileHost
{
    static class MEGA
    {
        private static readonly Random rnd = new Random();
        private static string user = ((NameValueCollection)ConfigurationManager.GetSection("Mega"))["User"];
        private static string password = ((NameValueCollection)ConfigurationManager.GetSection("Mega"))["Password"];
        private static List<string> links;

        public static bool CheckEnabled;
        public static bool GetEnabled;

        public static bool SetAccountInfo(string account)
        {
            user = account;
            try
            {
                File.AppendAllText("MegaAccount.txt", string.Format("{0}  |  {1}{2}", account, DateTime.Now.ToString("yyyy-MM-dd"), Environment.NewLine));

                var appConfig = XDocument.Load("SharingWorker.exe.config");
                var megaUser = (from el in appConfig.Descendants("Mega").Descendants("add")
                    where el.Attribute("key").Value == "User"
                    select el).FirstOrDefault();

                if (megaUser == null) return false;
                megaUser.Attribute("value").SetValue(account);
                appConfig.Save("SharingWorker.exe.config");
                return true;
            }
            catch (Exception ex)
            {
                App.Logger.Error("Failed to set MEGA account!", ex);
                return false;
            }
        }
        
        public static async Task<bool> LogIn()
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = @"megatools/megals.exe",
                UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true,
                Arguments = string.Format("-u {0} -p {1} -e /Root/", user, password)
            };

            return await Task.Run(() =>
            {
                using (var process = Process.Start(processInfo))
                {
                    using (var reader = process.StandardOutput)
                    {
                        var result = reader.ReadToEnd();
                        if (string.IsNullOrEmpty(result) || !result.Contains("https://mega.co.nz/#"))
                        {
                            App.Logger.Warn(result);
                            return false;
                        }
                        links = new List<string>(result.Split('\n'));

                        if (links.Count <= 0)
                        {
                            App.Logger.Warn("No link found");
                            return false;
                        }
                        
                        links.RemoveAt(links.Count - 1);
                        return true;
                    }
                }
            });
        }

        public static async Task<string> GetLinks(string filename)
        {
            if (links == null) return string.Empty;
            return await Task.Run(() =>
            {
                var searchLinks = links.Where(s => s.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0);

                string ret = null;
                foreach (var link in searchLinks)
                {
                    ret += (link.Remove(link.IndexOf(" /Root"), link.Length - link.IndexOf(" /Root")) + Environment.NewLine);
                }
                return ret;
            });
        }

        // Mega links checker, using http://urlchecker.org/
        public static async Task<bool> CheckLinks1(IEnumerable<string> links)
        {
            var ret = links.Any();
            foreach (var link in links)
            {
                try
                {
                    using (var handler = new HttpClientHandler())
                    using (var client = new HttpClient(handler))
                    using (var response = await client.GetAsync(new Uri(string.Format("http://api.urlchecker.net/?response_format=json&link={0}", HttpUtility.UrlEncode(link)))))
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                        var jsonLink = jsonObj.Property("link").Value.ToString();
                        var jsonResult = jsonObj.Property("result").Value.ToString();
                        var jsonStatus = jsonObj.Property("status").Value.ToString();
                        App.Logger.Info(string.Format("{0} | Result: {1} | Status: {2}", jsonLink, jsonResult, jsonStatus));
                        if (jsonStatus != "working") ret = false;
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.ErrorException("Error in CheckMegaLinks1()", ex);
                    ret = false;
                }
            }
            return ret;
        }

        // Mega links checker, using http://www.luenephysio.de/check/
        public static async Task<bool> CheckLinks2(IEnumerable<string> links)
        {

            return await Task.Run(async () =>
            {
                var ret = links.Any();
                using (var client = new HttpClient())
                {
                    foreach (var link in links)
                    {
                        try
                        {
                            var content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("links", link),
                            });

                            using (var response = await client.PostAsync(new Uri(string.Format("http://www.luenephysio.de/check/check.php?rand={0}", rnd.NextDouble().ToString("F16"))), content))
                            {

                                var result = response.Content.ReadAsStringAsync().Result;
                                var status = "Unknown";
                                if (result.Contains("error") || result.Contains("dead"))
                                {
                                    status = "Dead";
                                    ret = false;
                                }
                                else if (result.Contains("good") && result.Contains("mfname"))
                                {
                                    status = "Good";
                                }
                                else
                                    ret = false;

                                NLog.LogManager.GetLogger("CheckPost").Info("{0} | Status: {1}", link, status);
                            }
                        }
                        catch (Exception ex)
                        {
                            App.Logger.ErrorException("Error in CheckMegaLinks2()", ex);
                            ret = false;
                        }
                    }
                }
                Thread.Sleep(1000);
                return ret;
            });
        }

        public static async Task<string> CreateNewAccount(MailSource mailSource)
        {
            var mailAddress = "";
            switch (mailSource)
            {
                case MailSource.TempMail:
                    mailAddress = await TempMail.GetMailAddress().ConfigureAwait(false);
                    break;
                case MailSource.Mailinator:
                    mailAddress = Mailinator.GetMailAddress();
                    break;
            }
            
            var processInfo = new ProcessStartInfo
            {
                FileName = @"megatools/megareg.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            processInfo.Arguments = string.Format("--register --email {0} --name \"Javepc\" --password \"{1}\"", mailAddress, password);
            
            var verifyCommand = await Task.Run(() =>
            {
                using (var process = Process.Start(processInfo))
                {
                    using (var reader = process.StandardOutput)
                    {
                        var result = reader.ReadToEnd();
                        if (string.IsNullOrEmpty(result)) return null;
                        var start = result.IndexOf("--verify ", 0);
                        if (start < 0) return null;
                        var end = result.IndexOf("@LINK@", start);
                        if (end < 0) return null;
                        return result.Substring(start, end - start);
                    }
                }
            }).ConfigureAwait(false);

            if (string.IsNullOrEmpty(verifyCommand)) return string.Empty;
            
            await Task.Delay(12000);

            int start1, end1;
            var signupMail = "";
            var validationUrl = "";
            switch (mailSource)
            {
                case MailSource.TempMail:
                    signupMail = await TempMail.GetMegaSignupMail(mailAddress).ConfigureAwait(false);
                    start1 = signupMail.IndexOf("https://mega.co.nz/#confirm", 0, StringComparison.Ordinal);
                    if (start1 < 0) return string.Empty;
                    end1 = signupMail.IndexOf(Environment.NewLine, start1, StringComparison.Ordinal);
                    validationUrl = signupMail.Substring(start1, end1 - start1);
                    break;
                case MailSource.Mailinator:
                    signupMail = await Mailinator.GetMegaSignupMail(mailAddress).ConfigureAwait(false);
                    start1 = signupMail.IndexOf("<a href=\"https://mega.co.nz/#confirm", 0, StringComparison.Ordinal);
                    if (start1 < 0) return string.Empty;
                    start1 += 9;
                    end1 = signupMail.IndexOf("\"", start1, StringComparison.Ordinal);
                    validationUrl = signupMail.Substring(start1, end1 - start1);
                    break;
            }

            processInfo.Arguments = verifyCommand + validationUrl;

            var verifyResult = await Task.Run(() =>
            {
                using (var process = Process.Start(processInfo))
                {
                    using (var reader = process.StandardOutput)
                    {
                        var result = reader.ReadToEnd();
                        return result.Contains("successfully");
                    }
                }
            }).ConfigureAwait(false);

            return verifyResult? mailAddress : string.Empty;
        }
    }
}
