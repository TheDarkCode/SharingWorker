using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NLog;

namespace SharingWorker.FileHost
{
    public static class Ouo
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly string singleApiUrl = ((NameValueCollection)ConfigurationManager.GetSection("Ouo"))["ApiUrl"];
        public static bool GetEnabled;

        public static async Task<string> GetLink(string link)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (
                        var response =
                            await client.GetAsync(string.Format("{0}{1}", singleApiUrl, HttpUtility.UrlEncode(link))))
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        if (result.Length > 40) result = string.Empty;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.Message, ex);
                return string.Empty;
            }
        }
    }
}
