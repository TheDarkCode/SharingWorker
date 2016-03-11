using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using SharingWorker.FileHost;
using SharingWorker.ImageHost;
using SharingWorker.MailHost;
using SharingWorker.Post;
using SharingWorker.Video;

namespace SharingWorker
{
    [Export(typeof(IShell))]
    partial class ShellViewModel : Screen
    {
        private readonly IWindowManager windowManager;

        [ImportingConstructor]
        public ShellViewModel(IWindowManager windowManager)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.DisplayName = string.Format("Sharing Worker {0}.{1}.{2}", version.Major, version.Minor, version.Build);

            UploadResults = new BindableCollection<UploadInfo>();
            
            ImgChili = new ImgChili();
            LoadConfig(ImgChili);
            ImgSpice = new ImgSpice();
            LoadConfig(ImgSpice);
            ImgMega = new ImgMega();
            LoadConfig(ImgMega);
            ImgDrive = new ImgDrive();
            LoadConfig(ImgDrive);
            ImgRock = new ImgRock();
            LoadConfig(ImgRock);

            GetMega = bool.Parse(((NameValueCollection)ConfigurationManager.GetSection("Mega"))["Enabled"]);
            GetUploadable = bool.Parse(((NameValueCollection)ConfigurationManager.GetSection("Uploadable"))["Enabled"]);
            GetRapidgator = bool.Parse(((NameValueCollection)ConfigurationManager.GetSection("Rapidgator"))["Enabled"]);
            GetBinBox = bool.Parse(((NameValueCollection)ConfigurationManager.GetSection("Binbox"))["Enabled"]);
            GetLinkbucks = bool.Parse(((NameValueCollection)ConfigurationManager.GetSection("Linkbucks"))["Enabled"]);

            Blogger.PostProgressEvent += Blogger_PostProgressEvent;
            
            CompressToRar = true;
            GetThumbnail = true;
            GetCover = true;
            RarList = new RarListViewModel();

            CheckMega = true;
            CheckUploadable = true;
            CheckBinbox = true;
            SharedFiles = new SharedFilesViewModel();

            MailSource = Enum.GetValues(typeof(MailSource)).Cast<MailSource>().Random();

            this.windowManager = windowManager;
        }

        public void SelectUploadImages()
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".jpg",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                Filter = "Images (.jpg)|*.jpg",
                Multiselect = true
            };

            var result = dlg.ShowDialog();
            if (result != true) return;

            foreach (var group in dlg.SafeFileNames.Where(s => s.EndsWith("pl.jpg")))
            {
                var uploadInfo = new UploadInfo(@group.Substring(0, @group.Length - 6));
                UploadResults.Add(uploadInfo);
                uploadInfo.UploadList = new List<UploadImage>();

                foreach (var image in dlg.FileNames.Where(s => s.IndexOf(uploadInfo.Id, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var uploadImage = new UploadImage
                    {
                        Name = Path.GetFileName(image),
                        Path = image,
                        Data = File.ReadAllBytes(image)
                    };

                    if (image.EndsWith("pl.jpg"))
                        uploadInfo.UploadList.Insert(0, uploadImage);
                    else
                        uploadInfo.UploadList.Add(uploadImage);
                }
            }
        }
        
        public async void StartLogin()
        {
            LoginFlag flag = 0;
            flag |= LoginFlag.Blogger;
            if (ImgChili.Enabled) flag |= LoginFlag.ImgChili;
            if (ImgSpice.Enabled) flag |= LoginFlag.ImgSpice;
            if (ImgMega.Enabled) flag |= LoginFlag.ImgMega;
            if (ImgDrive.Enabled) flag |= LoginFlag.ImgDrive;
            if (ImgRock.Enabled) flag |= LoginFlag.ImgRock;
            if (GetMega) flag |= LoginFlag.MEGA;
            if (GetRapidgator) flag |= LoginFlag.Rapidgator;
            if (GetUploadable) flag |= LoginFlag.Uploadable;

            if (await Login(flag))
                await MahDialog.ShowMessage("Login", "Login Success!", MessageDialogStyle.Affirmative);
            else
                await MahDialog.ShowMessage("Login", "Login Failed!", MessageDialogStyle.Affirmative);
        }

        private async Task<bool> Login(LoginFlag flag)
        {
            Message = "";
            try
            {
                var loginTasks = new List<Task>();
                if (!ImgChili.LoggedIn && flag.HasFlag(LoginFlag.ImgChili))
                    loginTasks.Add(ImgChili.LogIn());
                if (!ImgMega.LoggedIn && flag.HasFlag(LoginFlag.ImgMega))
                    loginTasks.Add(ImgMega.LogIn());
                if (!ImgDrive.LoggedIn && flag.HasFlag(LoginFlag.ImgDrive))
                    loginTasks.Add(ImgDrive.LogIn());
                if (!ImgRock.LoggedIn && flag.HasFlag(LoginFlag.ImgRock))
                    loginTasks.Add(ImgRock.LogIn());
                if (!ImgSpice.LoggedIn && flag.HasFlag(LoginFlag.ImgSpice))
                    loginTasks.Add(ImgSpice.LogIn());

                Task<bool> megaLoginTask = null;
                if (!MegaLoggedIn && flag.HasFlag(LoginFlag.MEGA))
                {
                    megaLoginTask = MEGA.LogIn();
                    loginTasks.Add(megaLoginTask);
                }

                Task<bool> rapidgatorLoginTask = null;
                if (!RapidgatorLoggedIn && flag.HasFlag(LoginFlag.Rapidgator))
                {
                    rapidgatorLoginTask = Rapidgator.LogIn();
                    loginTasks.Add(rapidgatorLoginTask);
                }

                Task<bool> uploadableLoginTask = null;
                if (!UploadableLoggedIn && flag.HasFlag(LoginFlag.Uploadable))
                {
                    uploadableLoginTask = Uploadable.LogIn();
                    loginTasks.Add(uploadableLoginTask);
                }

                Task<bool> blogLoginTask = null;
                if (!BloggerLoggedIn && flag.HasFlag(LoginFlag.Blogger))
                {
                    blogLoginTask = Blogger.LogIn();
                    loginTasks.Add(blogLoginTask);
                }

                //Task<bool> picasaLoginTask = Picasa.LogIn();
                //loginTasks.Add(picasaLoginTask);
                
                await Task.WhenAll(loginTasks);

                if (megaLoginTask != null)
                    MegaLoggedIn = megaLoginTask.Result;
                if (rapidgatorLoginTask != null)
                    RapidgatorLoggedIn = rapidgatorLoginTask.Result;
                if (uploadableLoginTask != null)
                    UploadableLoggedIn = uploadableLoginTask.Result;
                if (blogLoginTask != null)
                    BloggerLoggedIn = blogLoginTask.Result;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                App.Logger.ErrorException(ex.Message, ex);
            }

            Message = "Login done";

            var ret = true;
            if (flag.HasFlag(LoginFlag.ImgChili)) ret &= ImgChili.LoggedIn;
            if (flag.HasFlag(LoginFlag.ImgSpice)) ret &= ImgSpice.LoggedIn;
            if (flag.HasFlag(LoginFlag.ImgMega)) ret &= ImgMega.LoggedIn;
            if (flag.HasFlag(LoginFlag.ImgDrive)) ret &= ImgDrive.LoggedIn;
            if (flag.HasFlag(LoginFlag.ImgRock)) ret &= ImgRock.LoggedIn;
            if (flag.HasFlag(LoginFlag.MEGA)) ret &= MegaLoggedIn;
            if (flag.HasFlag(LoginFlag.Rapidgator)) ret &= RapidgatorLoggedIn;
            if (flag.HasFlag(LoginFlag.Uploadable)) ret &= UploadableLoggedIn;
            if (flag.HasFlag(LoginFlag.Blogger)) ret &= BloggerLoggedIn;
            CanUpload = ret;
            return ret;
        }

        public async void Upload()
        {
            if (!UploadResults.Any()) return;

            IsUploadFinished = false;
            Message = "";
            Blogger.StartPosting();

            //var checkPicasa = true;
            var rnd = new Random();
            foreach (var uploadInfo in UploadResults)
            {
                var megaLinks = MEGA.GetEnabled ? await MEGA.GetLinks(uploadInfo.Id) : string.Empty;
                if (!string.IsNullOrEmpty(megaLinks))
                    megaLinks = megaLinks.TrimEnd(Environment.NewLine.ToCharArray()).Replace("\r\n", "\\n");

                var rapidgatorLinks = Rapidgator.GetEnabled ? await Rapidgator.GetLinks(uploadInfo.Id) : string.Empty;
                if (!string.IsNullOrEmpty(rapidgatorLinks))
                    rapidgatorLinks = rapidgatorLinks.TrimEnd(Environment.NewLine.ToCharArray()).Replace("\r\n", "\\n");

                //var uploadableLinks = Uploadable.GetEnabled ? await Uploadable.GetLinks(uploadInfo.Id) : string.Empty;
                //if (!string.IsNullOrEmpty(uploadableLinks))
                //    uploadableLinks = uploadableLinks.TrimEnd(Environment.NewLine.ToCharArray()).Replace("\r\n", "\\n");

                var megaCount = megaLinks.AllIndexesOf("mega.co.nz").Count();
                var rapidgatorCount = rapidgatorLinks.AllIndexesOf("rapidgator").Count();
                //var uploadableCount = uploadableLinks.AllIndexesOf("uploadable").Count();

                if (string.IsNullOrEmpty(megaLinks) && MEGA.GetEnabled)
                {
                    uploadInfo.Id += "\n(No MEGA links!)";
                    continue;
                }
                if (string.IsNullOrEmpty(rapidgatorLinks) && Rapidgator.GetEnabled)
                {
                    uploadInfo.Id += "\n(No Rapidgator links!)";
                    continue;
                }
                //if (string.IsNullOrEmpty(uploadableLinks) && Uploadable.GetEnabled)
                //{
                //    uploadInfo.Id += "\n(No Uploadable links!)";
                //    continue;
                //}
                if (MEGA.GetEnabled && Rapidgator.GetEnabled && megaCount != rapidgatorCount)
                {
                    uploadInfo.Id += "\n(Missing some Mega/Rapidgator links!)";
                    continue;
                }

                var uploadTasks = new List<Task<List<string>>>();

                if (ImgChili.LoggedIn && ImgChili.Enabled)
                    uploadTasks.Add(ImgChili.Upload(uploadInfo.UploadList));
                if (ImgSpice.LoggedIn && ImgSpice.Enabled)
                    uploadTasks.Add(ImgSpice.Upload(uploadInfo.UploadList));
                if (ImgMega.LoggedIn && ImgMega.Enabled)
                    uploadTasks.Add(ImgMega.Upload(uploadInfo.UploadList));
                if (ImgDrive.LoggedIn && ImgDrive.Enabled)
                    uploadTasks.Add(ImgDrive.Upload(uploadInfo.UploadList));
                if (ImgRock.LoggedIn && ImgRock.Enabled)
                    uploadTasks.Add(ImgRock.Upload(uploadInfo.UploadList));

                var taskCount = uploadTasks.Count;

                var uploadCount = 0;
                while (uploadTasks.Count > 0)
                {
                    try
                    {
                        var finishedTask = await Task.WhenAny(uploadTasks);
                        uploadTasks.Remove(finishedTask);

                        var links = await finishedTask;
                        if (links == null) continue;

                        if (links[0].Contains("imgchili"))
                        {
                            uploadInfo.WebLinks1 = links[0].Trim();
                            uploadInfo.ForumLinks1 = links[1].Trim().ToLowerInvariant();
                            uploadCount++;
                        }
                        else if (links[0].Contains("imgmega"))
                        {
                            uploadInfo.WebLinks2 = links[0].Trim();
                            uploadInfo.ForumLinks2 = links[1].Trim().ToLowerInvariant();
                            uploadCount++;
                        }
                        else if (links[0].Contains("imgrock"))
                        {
                            uploadInfo.WebLinks3 = links[0].Trim();
                            uploadInfo.ForumLinks3 = links[1].Trim().ToLowerInvariant();
                            uploadCount++;
                        }
                        else if (links[0].Contains("imgdrive"))
                        {
                            uploadInfo.WebLinks4 = links[0].Trim();
                            uploadInfo.ForumLinks4 = links[1].Trim().ToLowerInvariant();
                            uploadCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains(ImgChili.Name))
                            uploadInfo.WarningBrush1 = Brushes.Red;
                        else if (ex.Message.Contains(ImgMega.Name))
                            uploadInfo.WarningBrush2 = Brushes.Red;
                        else if (ex.Message.Contains(ImgRock.Name))
                            uploadInfo.WarningBrush3 = Brushes.Red;
                        else if (ex.Message.Contains(ImgDrive.Name))
                            uploadInfo.WarningBrush4 = Brushes.Red;

                        uploadInfo.IdColor = Colors.Red;
                        Message = ex.InnerException.Message;
                    }
                }

                //var picasaBackup = await Picasa.Upload(uploadInfo.Id, uploadInfo.UploadList);
                //checkPicasa |= picasaBackup;

                try
                {
                    await uploadInfo.WriteOutput(megaLinks, rapidgatorLinks);
                }
                catch (Exception ex)
                {
                    App.Logger.Error(string.Format("Failed to write output: {0}", uploadInfo.Id), ex);
                    if (ex.InnerException != null)
                        App.Logger.Error(string.Format("Failed to write output (InnerException): {0}", uploadInfo.Id), ex.InnerException);
                }

                // Delete or Move images
                //if (uploadCount == taskCount && picasaBackup)
                if (uploadCount == taskCount)
                {
                    foreach (var image in uploadInfo.UploadList)
                    {
                        try
                        {
                            File.Delete(image.Path);
                        }
                        catch (Exception ex)
                        {
                            App.Logger.Error(string.Format("Failed to delete images: {0}", uploadInfo.Id), ex);
                        }
                    }
                }
                else
                {
                    foreach (var image in uploadInfo.UploadList)
                    {
                        try
                        {
                            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Handled");
                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                            File.Move(image.Path, Path.Combine(dir, Path.GetFileName(image.Path)));
                        }
                        catch (Exception ex)
                        {
                            App.Logger.ErrorException(string.Format("Failed to move images: {0}", uploadInfo.Id), ex);
                        }
                    }
                }

                // Move files
                foreach (var path in UploadInfo.UploadPaths)
                {
                    if (!Directory.Exists(path)) continue;
                    var files = Directory.GetFiles(path, string.Format("{0}*.part*.rar", uploadInfo.Id), SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        var donePath = Path.Combine(fileInfo.DirectoryName, "Done");
                        if (!Directory.Exists(donePath)) Directory.CreateDirectory(donePath);

                        try
                        {
                            File.Move(file, Path.Combine(donePath, fileInfo.Name));
                        }
                        catch (IOException)
                        {
                            break;
                        }
                    }
                    if (files.Any()) break;
                }

                SpinWait.SpinUntil(() => false, rnd.Next(600, 1200));
            }
            
            Message = "Upload done";
        }

        public void SelectVideos()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Videos |*.avi;*.wmv;*.mkv;*.mp4",
                Multiselect = true,
            };
            if (Directory.Exists(@"D:\Upload"))
                dlg.InitialDirectory = @"D:\Upload";

            var result = dlg.ShowDialog();
            if (result != true) return;
            
            foreach (var filePath in dlg.FileNames)
            {
                RarList.FileList.Add(new RarListItem
                {
                    Path = filePath,
                    FileName = VideoInfo.GetNormalizedName(Path.GetFileNameWithoutExtension(filePath)),
                });
            }
            
            if (!RarList.FileList.Any()) return;

            RarList.CheckDuplicateName();

            if (!RarList.IsActive)
            {
                dynamic settings = new ExpandoObject();
                settings.WindowStyle = WindowStyle.ToolWindow;
                settings.ShowInTaskbar = true;
                windowManager.ShowWindow(RarList, null, settings);
            }
            
            CanProcess = true;
        }

        public async void ProcessVideos()
        {
            foreach (var file in RarList.FileList)
            {
                Task<bool> rarTask = null;
                Task<bool> thumbnailTask = null;
                Task<bool> coverTask = null;

                try
                {
                    var videoTasks = new List<Task>();
                    if (CompressToRar)
                    {
                        rarTask = VideoUtil.ToRAR(file.Path, file.FileName);
                        videoTasks.Add(rarTask);
                    }
                    if (GetThumbnail)
                    {
                        thumbnailTask = VideoCover.GetSnapshot(file.Path, file.FileName);
                        videoTasks.Add(thumbnailTask);
                    }
                    if (GetCover)
                    {
                        coverTask = VideoCover.GetCover(file.FileName);
                        videoTasks.Add(coverTask);
                    }

                    await Task.WhenAll(videoTasks);

                    if (rarTask != null)
                        file.RarStatus = rarTask.Result ? "OK" : "Error";
                    if (thumbnailTask != null)
                        file.ThumbnailStatus = thumbnailTask.Result ? "OK" : "Error";
                    if (coverTask != null)
                        file.CoverStatus = coverTask.Result ? "OK" : "Error";
                }
                catch (Exception ex)
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
            }
        }

        public async void Restore(SharedFilesViewModel.RestoreSource source)
        {
            switch (source)
            {
                case SharedFilesViewModel.RestoreSource.BinboxBackup:
                case SharedFilesViewModel.RestoreSource.LinksBackup:
                    await Login(LoginFlag.Blogger | LoginFlag.ImgMega | LoginFlag.ImgChili | LoginFlag.ImgDrive | LoginFlag.ImgRock);
                    break;
                case SharedFilesViewModel.RestoreSource.BlogPost:
                    await Login(LoginFlag.Blogger | LoginFlag.ImgMega | LoginFlag.ImgChili | LoginFlag.ImgDrive | LoginFlag.ImgRock);
                    CheckBinbox = false;
                    CheckMega = false;
                    CheckUploadable = false;
                    break;
            }

            SharedFiles.RestoreMode = source;

            if (!SharedFiles.IsActive)
            {
                dynamic settings = new ExpandoObject();
                settings.WindowStyle = WindowStyle.ToolWindow;
                settings.ShowInTaskbar = true;
                windowManager.ShowWindow(SharedFiles, null, settings);
            }
        }

        public async void CreateMegaAccount()
        {
            Message = "Create new mega account...";
            var account = await MEGA.CreateNewAccount(MailSource);
            
            if (string.IsNullOrEmpty(account))
            {
                Message = "Failed to create account!";
                return;
            }
            
            if (MEGA.SetAccountInfo(account))
                Message = "Account created & set!";
            else
                Message = "Account created but failed to set!";
        }

        public void CheckPosts()
        {
            SharedFiles.CheckItems();
        }

        public void Closing()
        {
            IsUploadFinished = true;
        }

        public void RemoveItem()
        {
            UploadResults.RemoveRange(SelectedUploadInfos);
        }

        public void RemoveAll()
        {
            UploadResults.Clear();
        }

        public void OpenBinboxBackups()
        {
            foreach (var uploadInfo in SelectedUploadInfos)
            {
                try
                {
                    Process.Start("chrome.exe", uploadInfo.BackupBinboxLink);
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex);
                }
            }
        }

        private void LoadConfig(IImageHost imageHost)
        {
            try
            {
                if (!imageHost.LoadConfig())
                    Message = String.Format("Config error in {0}", imageHost.Name);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private async void Blogger_PostProgressEvent(object sender, ProgressChangedEventArgs e)
        {
            Message = string.Format("Post : {0} / {1}", e.ProgressPercentage, UploadResults.Count);

            if (e.ProgressPercentage == UploadResults.Count)
            {
                await MahDialog.ShowMessage("", "Blogger posting finished!", MessageDialogStyle.Affirmative);
            }
        }
    }
}
