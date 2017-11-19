using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Polly;

namespace SharingWorker.UrlShortening
{
    public abstract class UrlShortening : IUrlShortening, INotifyPropertyChanged
    {
        protected NLog.Logger Logger => NLog.LogManager.GetLogger(this.GetType().FullName);

        protected class Reply
        {
            public string status { get; set; }
            public string shortenedUrl { get; set; }
        }

        public abstract string ApiUrl { get; }
        public abstract string Name { get; }
        public virtual bool FirstLinkEnabled => false;

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                OnPropertyChanged();
            }
        }

        public UrlShortening()
        {
            var nameSec = ConfigurationManager.GetSection(Name);
            if (nameSec != null)
            {
                Enabled = bool.Parse(((NameValueCollection) nameSec)["Enabled"]);
            }
        }

        public virtual async Task<string> GetLink(string link)
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
                            var url = string.Format("{0}{1}", ApiUrl, HttpUtility.UrlEncode(link));
                            var result = await client.GetStringAsync(url);
                            var reply = JsonConvert.DeserializeObject<Reply>(result);
                            return reply.shortenedUrl;
                        }
                    });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex.Message, ex);
                return string.Empty;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
