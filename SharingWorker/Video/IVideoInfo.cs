using System.Threading.Tasks;

namespace SharingWorker.Video
{
    public interface IVideoInfo
    {
        Task<VideoInfo> GetInfo(string id);
    }
}
