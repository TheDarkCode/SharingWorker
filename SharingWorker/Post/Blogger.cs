using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Blogger.v3;
using Google.Apis.Services;
using Newtonsoft.Json;

namespace SharingWorker.Post
{
    static class Blogger
    {
        public class BlogPost
        {
            public BlogPost(string title, string content, string link, LinksBackup linksBackup)
            {
                Title = title;
                Content = content;
                Link = link;
                LinksBackup = linksBackup;
            }
            public string Title;
            public string Content;
            public string Link;
            public LinksBackup LinksBackup;
        }
        
        private static string apiKey = "{API-KEY}";
        private static string blogUrl = "{BLOG-URL}";

        public static readonly string BlogId = ((NameValueCollection)ConfigurationManager.GetSection("Blogger"))["BlogId"];
        private static readonly string postUri = string.Format("https://www.blogger.com/feeds/{0}/posts/default", BlogId);
        private static readonly int postInterval = int.Parse(((NameValueCollection)ConfigurationManager.GetSection("Blogger"))["PostInterval"]) * 1000;
        private static readonly ConcurrentQueue<BlogPost> postQueue = new ConcurrentQueue<BlogPost>();
        private static readonly BackgroundWorker postWorker = new BackgroundWorker();
        private static int postCount;

        private static string clientId = "308480037624-ac96560c6eggvm3bmg4hr2n6pnumhjqh.apps.googleusercontent.com";
        private static string clientSecret = "p9V8HF4vjCSzsBJ5uflj720o";
        private static UserCredential credential;
        private static BloggerService service;

        public delegate void PostProgressEventHandler(object sender, ProgressChangedEventArgs e);
        public static event PostProgressEventHandler PostProgressEvent;

        static Blogger()
        {
            postWorker.WorkerReportsProgress = true;
            postWorker.ProgressChanged += postWorker_ProgressChanged;
            postWorker.DoWork += postWorker_DoWork;
            postCount = 0;
        }

        public static async Task<bool> LogIn()
        {
            //credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            //    new ClientSecrets
            //    {
            //        ClientId = clientId,
            //        ClientSecret = clientSecret,
            //    },
            //    new[]
            //    {
            //        BloggerService.Scope.Blogger
            //    },
            //    "user",                               //如果只有一人, 可以使用任何固定字串, 例如: "user"
            //    CancellationToken.None,
            //    new FileDataStore("Blogger.Auth.Store")     //用來儲存 Token 的目錄
            //);
            
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets, new[] { BloggerService.Scope.Blogger },
                    "epcjav4@gmail.com", CancellationToken.None);
            }

            if (credential == null) return false;
            service = new BloggerService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "blogtestproject", // 這個名稱必須與申請憑證時所使用的專案名稱相同
            });
            return service != null;
        }

        public static void StartPosting()
        {
            postWorker.RunWorkerAsync();
        }

        public static void AddPost(BlogPost blogPost)
        {
            postQueue.Enqueue(blogPost);
        }

        static void postWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var rnd = new Random();
            while (!ShellViewModel.IsUploadFinished)
            {
                BlogPost blogPost;
                if (postQueue.TryDequeue(out blogPost))
                {
                    PostNewDraftEntry(blogPost);
                    postWorker.ReportProgress(++postCount);
                    SpinWait.SpinUntil(() => false, rnd.Next(postInterval, postInterval + 2000));
                }
            }
        }
        
        static void postWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (PostProgressEvent != null)
                PostProgressEvent(sender, e);
        }

        /** Creates a new blog entry and sends it to the specified Uri */
        public static void PostNewDraftEntry(BlogPost blogPost)
        {
            try
            {
                // 建立 Post 物件資料
                var post = new Google.Apis.Blogger.v3.Data.Post
                {
                    Title = blogPost.Title,
                    Content = "<div style='text-align: center;'>" +
                                blogPost.Content +
                                " </div>" +
                                "Download (Mega.co.nz, Rapidgator) : <br /><hr class=\"more\"></hr>" +
                                "<a href=\"" + blogPost.Link + "\">" + blogPost.Link + "</a>",
                };

                // 送出 Insert Request
                var request = new PostsResource.InsertRequest(service, post, BlogId)
                {
                    IsDraft = true
                };
                var result = request.Execute();
                if (result == null)
                {
                    App.Logger.Error("Inserting blog blogPost failed");
                }
                else
                {
                    blogPost.LinksBackup.PostUri = result.SelfLink;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex.Message);
            }
            finally
            {
                NLog.LogManager.GetLogger("LinksBackup").
                    Info(JsonConvert.SerializeObject(blogPost.LinksBackup, Formatting.Indented));
            }
        }

        //public static async Task<IEnumerable<AtomEntry>> GetBlogPosts(DateTime startDate, DateTime endDate, int postNum = 0)
        //{
            //return await Task.Run(() =>
            //{
            //    var query = new FeedQuery
            //    {
            //        Uri = new Uri(postUri),
            //        MinPublication = startDate,
            //        MaxPublication = endDate,
            //    };
                
            //    if (postNum != 0)
            //        query.NumberToRetrieve = postNum;
                
            //    var feed = service.Query(query);
            //    return feed.Entries;
            //});
        //}

        //public static async Task<AtomEntry> GetBlogPost(string uri)
        //{
        //    return await Task.Run(() => service.Get(uri));
        //}

        //public static void ReplacePostImages(string entryUri, string newImages)
        //{
        //    var post = service.Get(entryUri);
        //    var search = "<div style=\"text-align: center;\">";
        //    var start = post.Content.Content.IndexOf(search, 0);
        //    if (start >= 0)
        //    {
        //        start += search.Length;
        //        var end = post.Content.Content.IndexOf("</div>", start);
        //        if (end > start)
        //        {
        //            var oldImages = post.Content.Content.Substring(start, end - start);
        //            post.Content.Content = post.Content.Content.Replace(oldImages, newImages);

        //            post.Content.Content = post.Content.Content.Replace("<br /><a name='more'></a>", "<br /><hr class=\"more\"></hr>");
        //            post.IsDraft = true;
        //            post.Update();
        //        }
        //    }
        //}

        public static void ReplaceBinbox(string entryUri, string newLink)
        {
            //var post = service.Get(entryUri);

            //if (post.Content.Content.IndexOf("https://binbox", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //    post.Content.Content.IndexOf("http://binbox", StringComparison.OrdinalIgnoreCase) >= 0)
            //{
            //    if (newLink.Contains("binbox"))
            //    {
            //        foreach (var start in post.Content.Content.AllIndexesOf("https://binbox"))
            //        {
            //            var oldBinbox = post.Content.Content.Substring(start, 32);
            //            post.Content.Content = post.Content.Content.Replace(oldBinbox, newLink);
            //        }
            //        foreach (var start in post.Content.Content.AllIndexesOf("http://binbox"))
            //        {
            //            var oldBinbox = post.Content.Content.Substring(start, 31);
            //            post.Content.Content = post.Content.Content.Replace(oldBinbox, newLink);
            //        }
            //    }
            //    else
            //    {
            //        var replace = true;
            //        while (replace)
            //        {
            //            var start = post.Content.Content.IndexOf("https://binbox");
            //            if (start >= 0)
            //            {
            //                var oldBinbox = post.Content.Content.Substring(start, 32);
            //                post.Content.Content = post.Content.Content.Replace(oldBinbox, newLink);
            //            }
            //            else
            //                replace = false;
            //        }

            //        replace = true;
            //        while (replace)
            //        {
            //            var start = post.Content.Content.IndexOf("http://binbox");
            //            if (start >= 0)
            //            {
            //                var oldBinbox = post.Content.Content.Substring(start, 32);
            //                post.Content.Content = post.Content.Content.Replace(oldBinbox, newLink);
            //            }
            //            else
            //                replace = false;
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (var aliasUrl in LinkBucks.AliasUrls)
            //    {
            //        if (!post.Content.Content.Contains(aliasUrl)) continue;

            //        var search = string.Format("http://www.{0}/", aliasUrl);
            //        var diff = 0;
            //        foreach (var start in post.Content.Content.AllIndexesOf(search))
            //        {
            //            var oldLinkbucks = post.Content.Content.Substring(start + diff, search.Length + 5);
            //            diff = newLink.Length - oldLinkbucks.Length;
            //            post.Content.Content = post.Content.Content.Replace(oldLinkbucks, newLink);
            //        }
            //    }
            //}

            //post.Content.Content = post.Content.Content.Replace("<br /><a name='more'></a>", "<br /><hr class=\"more\"></hr>");
            //post.IsDraft = true;
            //post.Update();
        }
    }
}
