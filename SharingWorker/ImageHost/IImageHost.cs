using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharingWorker.ImageHost
{
    public enum ImageHost { ImgChili, ImgSpice, ImgMega, ImgDrive, ImgRock }
    interface IImageHost
    {
        string Name { get; set; }
        bool Enabled { get; set; }
        bool LoggedIn { get; set; }
        string Url { get; set; }
        string User { get; set; }
        string Password { get; set; }

        bool LoadConfig();
        Task LogIn();
        Task<List<string>> Upload(IEnumerable<UploadImage> uploadImages);
    }
}
