using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Configuration;

namespace SharingWorker.UrlShortening
{
    [Export(typeof(IUrlShortening))]
    class Ourl : UrlShortening
    {
        public override string Name => "oURL";
        public override string ApiUrl => ((NameValueCollection) ConfigurationManager.GetSection("oURL"))["ApiUrl"];
    }
}
