using System.ComponentModel.Composition;

namespace SharingWorker.UrlShortening
{
    [Export(typeof(IUrlShortening))]
    class OkeIo : UrlShortening
    {
        public override string ApiUrl => "http://oke.io/api/?api=88f5fe11287ff09883a328709ef04cf17055bd10&url=";
        public override string Name => "OkeIo";
    }
}
