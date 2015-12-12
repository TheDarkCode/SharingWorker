using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Caliburn.Micro;
using Google.GData.Client;
using Microsoft.Win32;
using Newtonsoft.Json;
using SharingWorker.FileHost;
using SharingWorker.ImageHost;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace SharingWorker.Post
{
    public class SharedItem : PropertyChangedBase
    {
        public List<string> RarFilePaths;
        public DateTime? CreatedDate;
        public string Id;

        private string searchFileName;
        public string SearchFileName
        {
            get { return searchFileName; }
            set
            {
                searchFileName = value;
                NotifyOfPropertyChange(() => SearchFileName);
            }
        }

        private Uri coverUrl;
        public Uri CoverUrl
        {
            get { return coverUrl; }
            set
            {
                coverUrl = value;
                NotifyOfPropertyChange(() => CoverUrl);
            }
        }

        private Uri thumbnailUrl;
        public Uri ThumbnailUrl
        {
            get { return thumbnailUrl; }
            set
            {
                thumbnailUrl = value;
                NotifyOfPropertyChange(() => ThumbnailUrl);
            }
        }

        private string imageStatus;
        public string ImageStatus
        {
            get { return imageStatus; }
            set
            {
                imageStatus = value;
                NotifyOfPropertyChange(() => ImageStatus);
                NotifyOfPropertyChange(() => ImageStatusColor);
            }
        }

        private string host1Status;
        public string Host1Status
        {
            get { return host1Status; }
            set
            {
                host1Status = value;
                NotifyOfPropertyChange(() => Host1Status);
                NotifyOfPropertyChange(() => Host1StatusColor);
            }
        }

        private string host2Status;
        public string Host2Status
        {
            get { return host2Status; }
            set
            {
                host2Status = value;
                NotifyOfPropertyChange(() => Host2Status);
                NotifyOfPropertyChange(() => Host2StatusColor);
            }
        }

        private string host3Status;
        public string Host3Status
        {
            get { return host3Status; }
            set
            {
                host3Status = value;
                NotifyOfPropertyChange(() => Host3Status);
                NotifyOfPropertyChange(() => Host3StatusColor);
            }
        }

        public Brush ImageStatusColor
        {
            get { return imageStatus == "OK" ? Brushes.ForestGreen : Brushes.OrangeRed; }
        }

        public Brush Host1StatusColor
        {
            get { return host1Status == "OK" ? Brushes.ForestGreen : Brushes.OrangeRed; }
        }

        public Brush Host2StatusColor
        {
            get { return host2Status == "OK" ? Brushes.ForestGreen : Brushes.OrangeRed; }
        }

        public Brush Host3StatusColor
        {
            get { return host3Status == "OK" ? Brushes.ForestGreen : Brushes.OrangeRed; }
        }

        public string PostUrl;
        public string PostUri;
    }

    class SharedFilesViewModel : Screen
    {
        public enum RestoreSource { BinboxBackup, BlogPost, LinksBackup }
        public RestoreSource RestoreMode { get; set; }

        public BindableCollection<SharedItem> Items { get; set; }

        private bool fromBlogPost;
        private string historyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Links Backup\RestoreHistory");
        private string linkLogPath;
        private int linksBackupCount;
        private const int linksBackupNum = 20;
        private readonly List<LinksBackup> backupLinksCache = new List<LinksBackup>();

        private DateTime startDate;
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                NotifyOfPropertyChange(() => StartDate);
            }
        }

        private DateTime endDate;
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                endDate = value;
                NotifyOfPropertyChange(() => EndDate);
            }
        }
        
        private List<SharedItem> selectedItems = new List<SharedItem>();
        public List<SharedItem> SelectedItems
        {
            get { return selectedItems; }
            set
            {
                selectedItems = value;
                NotifyOfPropertyChange(() => SelectedItems);
            }
        }

        [ImportingConstructor]
        public SharedFilesViewModel()
        {
            this.DisplayName = "Shared Files Checklist";
            Items = new BindableCollection<SharedItem>();
        }

        protected override void OnViewAttached(object view, object context)
        {
            if (File.Exists("RestoreDate"))
            {
                var restoreDate = File.ReadAllText("RestoreDate");
                var date = restoreDate.Split(Environment.NewLine.ToCharArray());
                StartDate = DateTime.Parse(date[0]);
                EndDate = DateTime.Parse(date[2]);
            }
            else
            {
                StartDate = DateTime.Now.AddDays(-1);
                EndDate = DateTime.Now;
            }
            base.OnViewAttached(view, context);
        }

        public override void CanClose(Action<bool> callback)
        {
            SelectedItems.Clear();
            Items.Clear();
            base.CanClose(callback);
        }

        public void AddItem(SharedItem item)
        {
            if (Items.Any(i => i.SearchFileName == item.SearchFileName)) return;
            Items.Add(item);
        }

        public async void CheckItems()
        {
            if (File.Exists("PostCheck.log"))
                File.Delete("PostCheck.log");

            StartDate = StartDate - TimeSpan.FromDays(1);
            EndDate = EndDate + TimeSpan.FromDays(1);

            //var posts = (await Blogger.GetBlogPosts(StartDate, EndDate, 80)).ToList();

            //await CheckPosts(posts);
            
            if (File.Exists("PostCheck.log"))
                Process.Start("notepad.exe", "PostCheck.log");
        }

        private async Task CheckPosts(List<AtomEntry> posts)
        {
            foreach (var item in Items)
            {
                AtomEntry post = null;
                switch(RestoreMode)
                {
                    case RestoreSource.BinboxBackup:
                        post = posts.FirstOrDefault(p => p.Title.Text.Contains(item.SearchFileName));
                        break;
                    case RestoreSource.BlogPost:
                        post = posts.FirstOrDefault(p => p.Title.Text.Contains(item.Id));
                        break;
                    case RestoreSource.LinksBackup:
                        if (fromBlogPost)
                            post = posts.FirstOrDefault(p => p.Title.Text.Contains(item.Id));
                        else
                            post = posts.FirstOrDefault(p => p.SelfUri == item.PostUri);
                        break;
                }
                
                if (post == null)
                {
                    item.SearchFileName += "\n(Post not found!)";
                    continue;
                }

                // Post Url & Uri
                item.PostUri = post.SelfUri.ToString();
                var start = post.Id.AbsoluteUri.IndexOf("post-", 0);
                if (start >= 0)
                {
                    start += 5;
                    var postId = post.Id.AbsoluteUri.Substring(start, post.Id.AbsoluteUri.Length - start);
                    item.PostUrl = string.Format("http://www.blogger.com/post-edit.g?blogID={0}&postID={1}&from=pencil", Blogger.BlogId, postId);
                }

                // Cover & Thumbnail
                var images = GetImageUri(post);
                if (images.ElementAtOrDefault(0) != null)
                {
                    item.CoverUrl = images.ElementAtOrDefault(0);
                }
                if (images.ElementAtOrDefault(1) != null)
                {
                    item.ThumbnailUrl = images.ElementAtOrDefault(1);
                }

                // Binbox
                if (Binbox.CheckEnabled)
                {
                    var binboxContent = await GetBinboxContent(post);
                    if (binboxContent.Item1.Any() && binboxContent.Item2.Any() && binboxContent.Item1.First() == binboxContent.Item2.First())
                    {
                        item.Host1Status = item.Host2Status = "Binbox Failed";
                    }
                    else
                    {
                        if (MEGA.CheckEnabled)
                        {
                            item.Host1Status = await MEGA.CheckLinks2(binboxContent.Item1) ? "OK" : "Failed";
                        }
                        if (Uploadable.CheckEnabled)
                        {
                            item.Host2Status = await Uploadable.CheckLinks(binboxContent.Item2) ? "OK" : "Failed";
                        }
                    }
                }

                NLog.LogManager.GetLogger("CheckPost").Info("");
            }
        }

        //TODO
        public async void RestoreImages(string imgHost)
        {
            if (!SelectedItems.Any()) return;
            IoC.Get<IShell>().Message = string.Format("Restore images... (0/{0})", SelectedItems.Count());
            var count = 0;
            foreach (var selectedItem in SelectedItems)
            {
                try
                {
                    var newImages = "";
                    if (imgHost == "ImgMega")
                        newImages = await ImgMega.GetImagesCode(selectedItem.Id);
                    else if (imgHost == "ImgChili")
                        newImages = await ImgChili.GetImagesCode(selectedItem.Id);
                    else if (imgHost == "ImgDrive")
                        newImages = await ImgDrive.GetImagesCode(selectedItem.Id);
                        

                    //Blogger.ReplacePostImages(selectedItem.PostUri, newImages);

                    IoC.Get<IShell>().Message = string.Format("Restore images... ({0}/{1})", ++count, SelectedItems.Count());
                }
                catch (Exception ex)
                {
                    App.Logger.ErrorException("Failed to restore Images!", ex);
                }
            }
            IoC.Get<IShell>().Message = string.Format("Images restored! ({0}/{1})", count, SelectedItems.Count());
        }

        public void OpenPost(ListViewItem sender)
        {
            var selectedItem = sender.Content as SharedItem;
            if (selectedItem == null || string.IsNullOrEmpty(selectedItem.PostUrl)) return;
            try
            {
                Process.Start("firefox.exe", selectedItem.PostUrl);
            }
            catch (Exception ex)
            {
                App.Logger.ErrorException("Failed to launch Firefox!", ex);
            }
        }

        public async void OpenPosts()
        {
            foreach (var item in Items)
            {
                if (string.IsNullOrEmpty(item.PostUrl)) continue;
                try
                {
                    Process.Start("firefox.exe", item.PostUrl);
                    await Task.Delay(600);
                }
                catch (Exception){ }
            }
        }
        
        private List<Uri> GetImageUri(AtomEntry post)
        {
            var ret = new List<Uri>();
            
            // Parse imgchili
            if (ParseHtmlUrl(ImageHost.ImageHost.ImgChili, post.Content.Content, ret)) return ret;

            // Parse ImgSpice
            if (ParseHtmlUrl(ImageHost.ImageHost.ImgSpice, post.Content.Content, ret)) return ret;

            // Parse ImgDrive
            if (ParseHtmlUrl(ImageHost.ImageHost.ImgDrive, post.Content.Content, ret)) return ret;

            // Parse ImgRock
            if (ParseHtmlUrl(ImageHost.ImageHost.ImgRock, post.Content.Content, ret)) return ret;

            // Parse ImgMega
            if (ParseHtmlUrl(ImageHost.ImageHost.ImgMega, post.Content.Content, ret)) return ret;

            return ret;
        }

        private bool ParseHtmlUrl(ImageHost.ImageHost imageHost, string html, List<Uri> uris)
        {
            var ret = false;
            var name = string.Empty;
            switch(imageHost)
            {
                case ImageHost.ImageHost.ImgChili:
                    name = "imgchili";
                    break;
                case ImageHost.ImageHost.ImgSpice:
                    name = "imgspice";
                    break;
                case ImageHost.ImageHost.ImgDrive:
                    name = "imgdrive";
                    break;
                case ImageHost.ImageHost.ImgRock:
                    name = "imgrock";
                    break;
                case ImageHost.ImageHost.ImgMega:
                    name = "imgmega";
                    break;
            }

            if (html.IndexOf(name, 0) >= 0)
            {
                var search = "src=\"";
                foreach (var src in html.AllIndexesOf(search))
                {
                    var start = src + search.Length;
                    var end = html.IndexOf(".jpg", start);
                    if (end < 0) continue;
                    end += 4;
                    var uri = html.Substring(start, end - start);
                    uris.Add(new Uri(uri));
                    ret = true;
                }
            }
            return ret;
        }
        
        private async Task<Tuple<List<string>, List<string>>> GetBinboxContent(AtomEntry post)
        {
            // Parse Binbox id & password
            var binboxPass = "";
            var binboxId = "";
            var search = "https://binbox.io/";
            var start = post.Content.Content.IndexOf(search, 0);
            if (start >= 0)
            {
                start += search.Length;
                var end = post.Content.Content.IndexOf("#", start);
                if (end - start == 5)
                {
                    binboxId = post.Content.Content.Substring(start, end - start);
                    start = end + 1;
                    end = post.Content.Content.IndexOf("\"", start);
                    if (end - start == 8)
                        binboxPass = post.Content.Content.Substring(start, end - start);
                }
            }
            
            // Decode Binbox link
            var megaLinks = new List<string>();
            var uploadableLinks = new List<string>();
            if (!string.IsNullOrEmpty(binboxPass) && !string.IsNullOrEmpty(binboxId))
            {
                var decodedContent = await Binbox.GetDecodedContent(binboxId, binboxPass);
                if (decodedContent.IndexOf("INVALID", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    var links = decodedContent.Split(Environment.NewLine.ToCharArray());
                    foreach (var link in links)
                    {
                        if (link.Contains("mega.co.nz"))
                            megaLinks.Add(link);
                        else if (link.Contains("uploadable.ch"))
                            uploadableLinks.Add(link);
                    }
                }
                else
                {
                    megaLinks.Add("Error");
                    uploadableLinks.Add("Error");
                }
            }
            return Tuple.Create(megaLinks, uploadableLinks);
        }

        public void RemoveItem()
        {
            Items.RemoveRange(SelectedItems);
        }

        public void RemoveAll()
        {
            Items.Clear();
        }

        public void DeleteAllFiles()
        {
            IoC.Get<IShell>().Message = "Delete files...";
            var ret = true;
            var itemsToRemove = new List<SharedItem>();
            foreach (var item in Items)
            {
                if (item.RarFilePaths == null || !item.RarFilePaths.Any()) continue;
                foreach (var filePath in item.RarFilePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                        itemsToRemove.Add(item);
                    }
                    catch (Exception ex)
                    {
                        ret = false;
                        App.Logger.Error(ex);
                    }
                }
            }
            Items.RemoveRange(itemsToRemove);
            IoC.Get<IShell>().Message = ret ? "Files deleted!" : "Files deleted! (Error)";
        }

        public async void RestoreBinbox()
        {
            if (!SelectedItems.Any()) return;
            IoC.Get<IShell>().Message = string.Format("Restore Binbox... (0/{0})", SelectedItems.Count());
            
            var count = 0;
            switch(RestoreMode)
            {
                case RestoreSource.BinboxBackup:
                    count = await RestoreBinboxBackup();
                    break;
                case RestoreSource.BlogPost:
                    count = await RestoreNewBinbox();
                    break;
                case RestoreSource.LinksBackup:
                    count = await RestoreLinkBackup();
                    if (!fromBlogPost)
                    {
                        linksBackupCount += linksBackupNum;
                        var history = linkLogPath + Environment.NewLine + linksBackupCount;
                        File.WriteAllText(historyPath, history);
                    }
                    break;
            }

            if (fromBlogPost)
            {
                StartDate = StartDate.AddDays(-1);
                EndDate = EndDate.AddDays(-1);
                var restoreDate = StartDate + Environment.NewLine + EndDate;
                File.WriteAllText("RestoreDate", restoreDate);
            }

            IoC.Get<IShell>().Message = string.Format("Binbox restored! ({0}/{1})", count, SelectedItems.Count());
        }

        private async Task<int> RestoreBinboxBackup()
        {
            return await Task.Run(() =>
            {
                var count = 0;
                var binboxLog = "";
                foreach (var selectedItem in SelectedItems)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(binboxLog))
                        {
                            var binboxLogPath = string.Format("Binbox_Backup_{0}.log", selectedItem.CreatedDate.Value.ToString("yyyy-MM"));
                            binboxLog = File.ReadAllText(binboxLogPath);
                        }

                        var start = fromBlogPost ? binboxLog.IndexOf(selectedItem.SearchFileName, 0, StringComparison.OrdinalIgnoreCase) : binboxLog.IndexOf(selectedItem.Id, 0, StringComparison.OrdinalIgnoreCase);
                        if (start < 0)
                        {
                            selectedItem.SearchFileName += "\n(No Binbox backup!)";
                            continue;
                        }
                        var end = start - 3;
                        start = binboxLog.LastIndexOf("https:", end, StringComparison.Ordinal);
                        var newBinbox = binboxLog.Substring(start, end - start);

                        Blogger.ReplaceBinbox(selectedItem.PostUri, newBinbox);

                        IoC.Get<IShell>().Message = string.Format("Restore Binbox... ({0}/{1})", ++count, SelectedItems.Count());
                    }
                    catch (Exception ex)
                    {
                        App.Logger.ErrorException("Failed to restore Binbox!", ex);
                    }
                }
                return count;
            });
        }

        private async Task<int> RestoreNewBinbox()
        {
            var count = 0;
            foreach (var selectedItem in SelectedItems)
            {
                try
                {
                    var newBinbox = "";
                    var uploadableLinks = await Uploadable.GetLinks(selectedItem.SearchFileName);
                    if (!string.IsNullOrEmpty(uploadableLinks))
                    {
                        uploadableLinks = uploadableLinks.TrimEnd(Environment.NewLine.ToCharArray()).Replace("\r\n", "\\n");
                        newBinbox = await Binbox.GetEncodedLink(selectedItem.SearchFileName, uploadableLinks, Binbox.ApiUrlType.Restore);
                        if (string.IsNullOrEmpty(newBinbox))
                        {
                            selectedItem.SearchFileName += "\n(Failed to create Binbox link!)";
                            continue;
                        }
                    }
                    else
                    {
                        selectedItem.SearchFileName += "\n(No Uploadable.ch links!)";
                        continue;
                    }
                    Blogger.ReplaceBinbox(selectedItem.PostUri, newBinbox);
                    IoC.Get<IShell>().Message = string.Format("Restore Binbox... ({0}/{1})", ++count, SelectedItems.Count());
                }
                catch (Exception ex)
                {
                    App.Logger.Error("Failed to restore Binbox!", ex);
                }
            }
            return count;
        }

        private async Task<int> RestoreLinkBackup()
        {
            var count = 0;
            var backupLinks = new List<LinksBackup>();

            foreach (var selectedItem in SelectedItems)
            {
                try
                {
                    if (!backupLinks.Any())
                    {
                        var linkLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"Links Backup\{0}.log", selectedItem.CreatedDate.Value.ToString("yyyy-MM")));
                        var linkLog = File.ReadAllText(linkLogPath);

                        foreach (var start in linkLog.AllIndexesOf("{"))
                        {
                            var end = linkLog.IndexOf("}", start) + 1;
                            backupLinks.Add(JsonConvert.DeserializeObject<LinksBackup>(linkLog.Substring(start, end - start)));
                        }
                    }
                    
                    var newPostLinks = "";
                    var links = "";
                    LinksBackup linksBackup = null;
                    if (fromBlogPost)
                        linksBackup = backupLinks.FirstOrDefault(l => l.Id == selectedItem.SearchFileName);
                    else
                        linksBackup = backupLinks.FirstOrDefault(l => l.Id == selectedItem.Id);

                    if (linksBackup != null)
                        links = linksBackup.Links;

                    if (!string.IsNullOrEmpty(links))
                    {
                        newPostLinks = await Binbox.GetEncodedLink(selectedItem.SearchFileName, links, Binbox.ApiUrlType.Restore);
                        
                        if (LinkBucks.GetEnabled)
                        {
                            var linkbucksLinks = await LinkBucks.GetLinkbucksSingle(selectedItem.SearchFileName, newPostLinks);
                            if (!string.IsNullOrEmpty(linkbucksLinks)) newPostLinks = linkbucksLinks;
                        }

                        if (string.IsNullOrEmpty(newPostLinks))
                        {
                            selectedItem.SearchFileName += "\n(Failed to create Binbox link!)";
                            continue;
                        }
                    }
                    else
                    {
                        selectedItem.SearchFileName += "\n(No Link backup!)";
                        continue;
                    }

                    Blogger.ReplaceBinbox(selectedItem.PostUri, newPostLinks);
                    await Task.Delay(300);
                    IoC.Get<IShell>().Message = string.Format("Restore Binbox... ({0}/{1})", ++count, SelectedItems.Count());
                }
                catch (Exception ex)
                {
                    App.Logger.Error("Failed to restore Binbox!", ex);
                }
            }
            return count;
        }

        public async void LoadBlogPosts()
        {
            fromBlogPost = true;

            //var posts = (await Blogger.GetBlogPosts(StartDate, EndDate, 80)).ToList();
            //foreach (var post in posts)
            //{
            //    var id = post.Title.Text;
            //    var start = post.Title.Text.IndexOf("(", StringComparison.Ordinal);
            //    if (start >= 0)
            //    {
            //        start += 1;
            //        var end = post.Title.Text.IndexOf(")", start, StringComparison.Ordinal);
            //        if (end >= 0) id = post.Title.Text.Substring(start, end - start);
            //    }

            //    var searchFileName = id;
            //    if (searchFileName.Contains("HEYZO "))
            //        searchFileName = id.Replace("HEYZO ", "heyzo_hd_");
            //    else if (searchFileName.Contains("Tokyo-Hot "))
            //        searchFileName = id.Replace("Tokyo-Hot ", "TokyoHot_");
            //    else if (searchFileName.Contains("XXX-AV "))
            //        searchFileName = id.Replace("XXX-AV ", "XXX-AV_");

            //    Items.Add(new SharedItem
            //    {
            //        Id = id,
            //        SearchFileName = searchFileName,
            //        CreatedDate = post.Published,
            //    });
            //}

            //await CheckPosts(posts);
        }

        public void LoadFiles()
        {
            fromBlogPost = false;

            var dlg = new OpenFileDialog
            {
                Filter = "RARs |*.rar",
                Multiselect = true,
            };
            if (Directory.Exists(@"J:\Upload\Done"))
                dlg.InitialDirectory = @"J:\Upload\Done";

            var result = dlg.ShowDialog();
            if (result != true || !dlg.FileNames.Any()) return;

            var folder = Path.GetDirectoryName(dlg.FileNames.First());
            foreach (var group in dlg.SafeFileNames.GroupBy(s => Regex.Replace(Path.GetFileNameWithoutExtension(s), @"\.part\d+", "")))
            {
                if (!group.Any()) continue;

                var filename = Regex.Replace(Path.GetFileNameWithoutExtension(group.First()), @"\.part\d+", "");
                var id = filename;
                if (filename.Contains("heyzo_hd_"))
                {
                    filename = filename.Replace("heyzo_hd_", "HEYZO ");
                }
                else if (filename.Contains("TokyoHot_"))
                {
                    filename = filename.Replace("TokyoHot_", "Tokyo-Hot ");
                }

                var sharedItem = new SharedItem
                {
                    Id = id,
                    SearchFileName = filename,
                    RarFilePaths = new List<string>(),
                };
                foreach (var rarFile in group)
                {
                    var filePath = Path.Combine(folder, rarFile);
                    if (sharedItem.CreatedDate == null)
                    {
                        var info = new FileInfo(filePath);
                        sharedItem.CreatedDate = info.CreationTime;
                    }
                    sharedItem.RarFilePaths.Add(filePath);
                }
                
                if (Items.Any(i => i.SearchFileName == sharedItem.SearchFileName)) return;
                Items.Add(sharedItem);

                var files = dlg.FileNames.Select(f => new FileInfo(f)).ToList();
                this.EndDate = files.Max(info => info.CreationTime);
                this.StartDate = files.Min(info => info.CreationTime);
            }
        }

        public async void LoadLinksBackup()
        {
            fromBlogPost = false;
            if (File.Exists(historyPath))
            {
                var history = File.ReadAllText(historyPath);
                var historyInfo = history.Split(Environment.NewLine.ToCharArray());
                linkLogPath = historyInfo[0];
                linksBackupCount = Convert.ToInt32(historyInfo[2]);
            }
            else
            {
                linksBackupCount = 0;

                var dlg = new OpenFileDialog
                {
                    Filter = "Links Backup |*.log",
                    Multiselect = false,
                };
                if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Links Backup")))
                    dlg.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Links Backup");

                var result = dlg.ShowDialog();
                if (result != true || !dlg.FileNames.Any()) return;
                linkLogPath = dlg.FileName;
                backupLinksCache.Clear();
            }

            if (!backupLinksCache.Any())
            {
                var linkLog = File.ReadAllText(linkLogPath);
                foreach (var start in linkLog.AllIndexesOf("{"))
                {
                    var end = linkLog.IndexOf("}", start) + 1;
                    backupLinksCache.Add(JsonConvert.DeserializeObject<LinksBackup>(linkLog.Substring(start, end - start)));
                }
            }

            var atomEntries = new List<AtomEntry>();
            foreach (var linksBackup in backupLinksCache.Skip(linksBackupCount).Take(linksBackupNum))
            {
                var sharedItem = new SharedItem
                {
                    Id = linksBackup.Id,
                    SearchFileName = linksBackup.Id,
                    PostUri = linksBackup.PostUri, 
                };
                Items.Add(sharedItem);

                //var ret = await Blogger.GetBlogPost(sharedItem.PostUri);
                
                //atomEntries.Add(ret);
            }
            
            await CheckPosts(atomEntries);
        }

        public async void LoadTitles()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "TXT files |*.txt",
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            };
            if (dlg.ShowDialog() != true) return;

            var titles = File.ReadLines(dlg.FileName).ToList();
            fromBlogPost = false;
            var count = 0;
            var backupLinks = new List<LinksBackup>();

            foreach (var title in titles)
            {
                try
                {
                    if (!backupLinks.Any())
                    {
                        var linkLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Links Backup", string.Format("{0}.log", StartDate.ToString("yyyy-MM")));
                        var linkLog = File.ReadAllText(linkLogPath);

                        foreach (var start in linkLog.AllIndexesOf("{"))
                        {
                            var end = linkLog.IndexOf("}", start) + 1;
                            backupLinks.Add(JsonConvert.DeserializeObject<LinksBackup>(linkLog.Substring(start, end - start)));
                        }
                    }

                    var sharedItem = new SharedItem
                    {
                        Id = title,
                        SearchFileName = title,
                    };
                    Items.Add(sharedItem);

                    var newBinbox = "";
                    var links = "";
                    LinksBackup linksBackup = null;
                    linksBackup = backupLinks.FirstOrDefault(l => l.Id == title);

                    if (linksBackup != null)
                    {
                        links = linksBackup.Links;
                        sharedItem.PostUri = linksBackup.PostUri;
                    }

                    if (!string.IsNullOrEmpty(links))
                    {
                        newBinbox = await Binbox.GetEncodedLink(title, links, Binbox.ApiUrlType.Restore);
                        if (string.IsNullOrEmpty(newBinbox))
                        {
                            sharedItem.SearchFileName += "\n(Failed to create Binbox link!)";
                            continue;
                        }
                    }
                    else
                    {
                        sharedItem.SearchFileName += "\n(No Link backup!)";
                        continue;
                    }

                    if (string.IsNullOrEmpty(sharedItem.PostUri) || sharedItem.PostUri.Contains("http"))
                        Blogger.ReplaceBinbox(sharedItem.PostUri, newBinbox);
                    else
                    {
                        sharedItem.SearchFileName += "\n(No Uri!)";
                        continue;
                    }

                    IoC.Get<IShell>().Message = string.Format("Restore Binbox... ({0}/{1})", ++count, titles.Count());
                }
                catch (Exception ex)
                {
                    App.Logger.Error("Failed to restore Binbox!", ex);
                }
            }
        }
    }
}
