using System.Collections.Specialized;
using System.Configuration;

namespace SharingWorker.UrlShortening
{
    class Urle : UrlShortening
    {
        public override string ApiUrl => ((NameValueCollection)ConfigurationManager.GetSection("Urle"))["ApiUrl"];
        public override string Name => "Urle";
    }
}
