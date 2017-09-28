using System.Threading.Tasks;

namespace SharingWorker.UrlShortening
{
    public interface IUrlShortening
    {
        bool Enabled { get; }
        bool FirstLinkEnabled { get; }
        string Name { get; }
        string ApiUrl { get; }

        Task<string> GetLink(string link);
    }
}
