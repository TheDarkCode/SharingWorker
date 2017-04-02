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
    class ShinkIn
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly string apiUrl = ((NameValueCollection)ConfigurationManager.GetSection("ShinkIn"))["ApiUrl"];
        public static bool GetEnabled;

        public static async Task<string> GetLink(string link)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (
                        var response =
                            await client.GetAsync(string.Format("{0}{1}", apiUrl, HttpUtility.UrlEncode(link))))
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result.TrimStart(Environment.NewLine.ToCharArray());
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
