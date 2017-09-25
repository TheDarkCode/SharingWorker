using System;
using System.ComponentModel.Composition;

namespace SharingWorker.Video.Western
{
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class)]
    class WesternInfoAttribute : ExportAttribute, IVideoInfoMetadata
    {
        public WesternInfoAttribute(string prefix) : base(VideoInfoContractNames.Western, typeof(IVideoInfo))
        {
            Prefix = prefix;
        }
        
        public string Prefix { get; }
    }
}
