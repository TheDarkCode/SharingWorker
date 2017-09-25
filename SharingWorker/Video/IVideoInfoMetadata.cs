namespace SharingWorker.Video
{
    public static class VideoInfoContractNames
    {
        public const string Western = "WesternInfo";
    }

    public interface IVideoInfoMetadata
    {
        string Prefix { get; }
    }
}
