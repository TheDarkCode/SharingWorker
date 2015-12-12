using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Google.GData.Client;
using Google.GData.Photos;
using NLog;

namespace SharingWorker.Post
{
    internal static class Picasa
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly string albumId = ((NameValueCollection) ConfigurationManager.GetSection("Picasa"))["AlbumId"];
        private static readonly string user = ((NameValueCollection) ConfigurationManager.GetSection("Picasa"))["User"];
        private static readonly string password = ((NameValueCollection) ConfigurationManager.GetSection("Picasa"))["Password"];

        private static PicasaService service;

        public static async Task<bool> LogIn()
        {
            var config = ConfigurationManager.GetSection("Picasa") as NameValueCollection;
            if (config == null) return false;

            service = new PicasaService("PostImages");
            service.Credentials = new GDataCredentials(user, password);

            return await Task.Run(() =>
            {
                try
                {
                    var factory = (GDataGAuthRequestFactory) service.RequestFactory;
                    factory.QueryAuthToken(service.Credentials);

                    return true;
                }
                catch (Exception ex)
                {
                    App.Logger.Warn(ex.Message);
                    return false;
                }
            });
        }

        public static async Task<bool> Upload(string id, IEnumerable<UploadImage> uploadImages)
        {
            return await Task.Run(() =>
            {
                var postUri = new Uri(PicasaQuery.CreatePicasaUri(user, albumId));
                var ret = true;
                foreach (var image in uploadImages)
                {
                    try
                    {
                        var fileInfo = new FileInfo(image.Path);
                        using (var fileStream = fileInfo.OpenRead())
                        {
                            var entry = (PicasaEntry) service.Insert(postUri, fileStream, "image/jpeg", image.Path);
                            entry.Media.Keywords.Value = id;
                            entry.Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(string.Format("Failed to upload {0}", image.Name), ex);
                        ret = false;
                    }
                }
                return ret;
            });
        }

        public static async Task<List<string>> GetImageUrls(string id)
        {
            var query = new PhotoQuery(PicasaQuery.CreatePicasaUri(user, albumId));
            query.Tags = id;
            query.ExtraParameters = "imgmax=d";
            query.NumberToRetrieve = 5;

            return await Task.Run(() =>
            {
                var ret = new List<string>();
                try
                {
                    var feed = service.Query(query);
                    foreach (var entry in feed.Entries)
                    {
                        var photoEntry = entry as PicasaEntry;
                        if (photoEntry == null) continue;
                        ret.Add(photoEntry.Media.Content.Url);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                return ret;
            });
        }

        public static async Task DownloadImages(string id)
        {
            var query = new PhotoQuery(PicasaQuery.CreatePicasaUri(user, albumId))
            {
                Tags = id,
                ExtraParameters = "imgmax=d",
                NumberToRetrieve = 5
            };

            try
            {
                var feed = service.Query(query);
                await Task.Run(() => Parallel.ForEach(feed.Entries, entry =>
                {
                    var photoEntry = entry as PicasaEntry;
                    if (photoEntry == null) return;

                    using (var client = new WebClient())
                    {
                        var fileName = Path.GetFileName(photoEntry.Media.Content.Url) ?? string.Format("{0}-{1}", id, Guid.NewGuid());
                        client.DownloadFile(new Uri(photoEntry.Media.Content.Url), fileName);
                    }
                }));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}