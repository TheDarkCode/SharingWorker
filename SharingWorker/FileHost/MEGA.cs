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
using NLog;
using Polly;
using SharingWorker.MailHost;

namespace SharingWorker.FileHost
{
    static class MEGA
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Random rnd = new Random();
        private static List<string> links;

        public static string User;
        public static string Password;
        
        public static bool CheckEnabled;
        public static bool GetEnabled;

        static MEGA()
        {
            var configSec = ConfigurationManager.GetSection("Mega");
            if (configSec == null) return;

            User = ((NameValueCollection) configSec)["User"];
            Password = ((NameValueCollection) configSec)["Password"];
        }

        public static bool SetAccountInfo(string account)
        {
            User = account;
            try
            {
                File.AppendAllText("MegaAccount.txt",
                    string.Format("{0}{1}   |   {2}", Environment.NewLine,
                    account, DateTime.Now.ToString("yyyy-MM-dd")));

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
                logger.Error("Failed to set MEGA account!", ex);
                return false;
            }
        }

        public static async Task<bool> LogIn()
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = @"megatools/megals.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = string.Format("-u {0} -p {1} -e -n /Root/", User, Password)
            };

            return await Task.Run(() =>
            {
                using (var process = Process.Start(processInfo))
                {
                    using (var reader = process.StandardOutput)
                    {
                        try
                        {
                            var result = reader.ReadToEnd();
                            if (string.IsNullOrEmpty(result) || !result.Contains("https://mega.nz/#"))
                            {
                                return false;
                            }

                            links = new List<string>();
                            foreach (var linkBegin in result.AllIndexesOf("https://mega.nz/"))
                            {
                                var linkEnd = result.IndexOf(Environment.NewLine, linkBegin,
                                    StringComparison.OrdinalIgnoreCase);
                                if (linkEnd > 0)
                                {
                                    links.Add(result.Substring(linkBegin, linkEnd - linkBegin));
                                }
                            }

                            return links.Any();
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Failed to login MEGA!", ex);
                            return false;
                        }
                    }
                }
            });
        }

        public static async Task<List<string>> GetLinks(string filename)
        {
            if (links == null) return Enumerable.Empty<string>().ToList();
            return await Task.Run(() =>
            {
                var searchLinks = links.Where(s => s.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0);

                var ret = new List<string>();
                foreach (var link in searchLinks)
                {
                    var end = link.IndexOf(" ", StringComparison.OrdinalIgnoreCase);
                    ret.Add(link.Remove(end));
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
                    using (var response =
                        await client.GetAsync(new Uri(
                            string.Format("http://api.urlchecker.net/?response_format=json&link={0}",
                                HttpUtility.UrlEncode(link)))))
                    {
                        var jsonObj =
                            JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
                        var jsonLink = jsonObj.Property("link").Value.ToString();
                        var jsonResult = jsonObj.Property("result").Value.ToString();
                        var jsonStatus = jsonObj.Property("status").Value.ToString();
                        App.Logger.Info(string.Format("{0} | Result: {1} | Status: {2}", jsonLink, jsonResult,
                            jsonStatus));
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

                            using (var response =
                                await client.PostAsync(
                                    new Uri(string.Format("http://www.luenephysio.de/check/check.php?rand={0}",
                                        rnd.NextDouble().ToString("F16"))), content))
                            {

                                var result = await response.Content.ReadAsStringAsync();
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
        
        public static async Task<string> CreateNewAccount(IMailHost mailHost)
        {
            var mailAddress = await mailHost.GetMailAddress();
            if (string.IsNullOrEmpty(mailAddress)) return string.Empty;

            var verifyCommand = await RegisterAccount(mailAddress);
            if (string.IsNullOrEmpty(verifyCommand)) return string.Empty;
            
            var validationUrl = await GetValidationUrl(mailHost, mailAddress);
            
            return await VerifyAccount(verifyCommand, validationUrl) ? mailAddress : string.Empty;
        }

        public static async Task<string> RegisterAccount(string mailAddress)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = @"megatools/megareg.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            var user = mailAddress.Remove(mailAddress.IndexOf("@"));

            processInfo.Arguments = string.Format("--register --email {0} --name \"{1}\" --password \"{2}\"",
                mailAddress, user, Password);

            var ret = await Task.Run(() =>
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
            return ret;
        }

        private static async Task<string> GetValidationUrl(IMailHost mailHost, string mailAddress)
        {
            return await Policy
                .Handle<Exception>()
                .OrResult<string>(string.IsNullOrEmpty)
                .WaitAndRetryAsync(6, retryCount => TimeSpan.FromSeconds(20 * retryCount),
                    (exception, timeSpan, context) =>
                    {
                        Trace.WriteLine(timeSpan.ToString());
                    })
                .ExecuteAsync(() => GetSignupMail(mailHost, mailAddress));
        }

        private static async Task<string> GetSignupMail(IMailHost mailHost, string mailAddress)
        {
            var signupMail = await mailHost.GetMegaSignupMail(mailAddress).ConfigureAwait(false);
            var start = signupMail.IndexOf(">https://mega.nz/#confirm", 0, StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            start += 1;
            var end = signupMail.IndexOf("</a>", start, StringComparison.Ordinal);
            if (end < 0) return string.Empty;

            return signupMail.Substring(start, end - start);
        }

        internal static async Task<bool> VerifyAccount(string verifyCommand, string validationUrl)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = @"megatools/megareg.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            processInfo.Arguments = verifyCommand + " " + validationUrl;
            return await Task.Run(() =>
            {
                using (var process = Process.Start(processInfo))
                {
                    var result = process.StandardOutput.ReadToEnd();
                    if (string.IsNullOrEmpty(result))
                    {
                        var error = process.StandardError.ReadToEnd();
                        logger.Error(error);
                        return false;
                    }
                    return result.IndexOf("successfully", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }).ConfigureAwait(false);
        }
    }
}
