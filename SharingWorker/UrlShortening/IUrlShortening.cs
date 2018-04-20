using System.Threading.Tasks;

namespace SharingWorker.UrlShortening
{
    public interface IUrlShortening
    {
        int Order { get; }
        bool Enabled { get; }
        bool FirstLinkEnabled { get; }
        string Name { get; }
        string ApiUrl { get; }

        Task<string> GetLink(string link);
    }
}
