using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharingWorker.Video
{
    static class WesternInfo
    {
        private static IEnumerable<Lazy<IVideoInfo, IVideoInfoMetadata>> infoParts =
            MefBootstrapper.GetAllInstances<IVideoInfo, IVideoInfoMetadata>(VideoInfoContractNames.Western);

        public static bool Match(string id)
        {
            return infoParts.Any(p => id.StartsWith(p.Metadata.Prefix));
        }

        public static async Task<VideoInfo> GetInfo(string id)
        {
            var part = infoParts.First(p => id.StartsWith(p.Metadata.Prefix));
            var info = await part.Value.GetInfo(id);
            info.HideId = true;
            return info;
        }
    }
}
