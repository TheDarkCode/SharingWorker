using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
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
using SharingWorker.UrlShortening;
using SharingWorker.Video;

namespace SharingWorker
{
	[Export(typeof(IShell))]
	partial class ShellViewModel : Screen
	{
		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IWindowManager windowManager;

		[ImportingConstructor]
		public ShellViewModel(IWindowManager windowManager, 
			[ImportMany] IEnumerable<IMailHost> mailHosts, 
			[ImportMany] IEnumerable<IUrlShortening> urlShortenings,
			[ImportMany] IEnumerable<IFileHost> fileHosts)
		{
			var version = Assembly.GetExecutingAssembly().GetName().Version;
			this.DisplayName = string.Format("Sharing Worker {0}.{1}.{2}", version.Major, version.Minor, version.Build);

			UploadResults = new BindableCollection<UploadInfo>();
			
			ImgChili = new ImgChili();
			LoadConfig(ImgChili);
			ImgRock = new ImgRock();
			LoadConfig(ImgRock);
			PixSense = new PixSense();
			LoadConfig(PixSense);
			
			GetMega = bool.Parse(((NameValueCollection)ConfigurationManager.GetSection("Mega"))["Enabled"]);
			GetUploadGIG = bool.Parse(((NameValueCollection)ConfigurationManager.GetSection("UploadGIG"))["Enabled"]);
			GetRapidgator = bool.Parse(((NameValueCollection)ConfigurationManager.GetSection("Rapidgator"))["Enabled"]);

			Blogger.PostProgressEvent += Blogger_PostProgressEvent;
			
			CompressToRar = true;
			GetThumbnail = true;
			GetCover = true;
			RarList = new RarListViewModel();
			
			MailHosts = new List<IMailHost>(mailHosts);
			UrlShortenings = new List<IUrlShortening>(urlShortenings.OrderBy(u => u.Order));
			FileHosts = new List<IFileHost>(fileHosts);
			foreach (var fileHost in FileHosts)
			{
				fileHost.LoadConfig();
			}
			
			this.windowManager = windowManager;
		}

		public async void SelectUploadImages()
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
			if (ImgRock.Enabled) flag |= LoginFlag.ImgRock;
			if (PixSense.Enabled) flag |= LoginFlag.PixSense;
			if (GetMega) flag |= LoginFlag.MEGA;
			if (GetRapidgator) flag |= LoginFlag.Rapidgator;
			if (GetUploadGIG) flag |= LoginFlag.UploadGIG;
			if (Zippyshare.Enabled) flag |= LoginFlag.Zippyshare;

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
				if (!ImgRock.LoggedIn && flag.HasFlag(LoginFlag.ImgRock))
					loginTasks.Add(ImgRock.LogIn());
				if (!PixSense.LoggedIn && flag.HasFlag(LoginFlag.PixSense))
					loginTasks.Add(PixSense.LogIn());

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

				Task<bool> uploadGIGLoginTask = null;
				if (!UploadGIGLoggedIn && flag.HasFlag(LoginFlag.UploadGIG))
				{
					uploadGIGLoginTask = UploadGIG.LogIn();
					loginTasks.Add(uploadGIGLoginTask);
				}

				if (!Zippyshare.LoggedIn && flag.HasFlag(LoginFlag.Zippyshare))
				{
					loginTasks.Add(Zippyshare.LogIn());
				}


				Task<bool> blogLoginTask = null;
				if (!BloggerLoggedIn && flag.HasFlag(LoginFlag.Blogger))
				{
					blogLoginTask = Blogger.LogIn();
					loginTasks.Add(blogLoginTask);
				}
				
				await Task.WhenAll(loginTasks);

				if (megaLoginTask != null)
					MegaLoggedIn =await megaLoginTask;
				if (rapidgatorLoginTask != null)
					RapidgatorLoggedIn = await rapidgatorLoginTask;
				if (uploadGIGLoginTask != null)
					UploadGIGLoggedIn = await uploadGIGLoginTask;
				if (blogLoginTask != null)
					BloggerLoggedIn = await blogLoginTask;
			}
			catch (Exception ex)
			{
				Message = ex.Message;
				Logger.ErrorException(ex.Message, ex);
			}

			Message = "Login done";

			var ret = true;
			if (flag.HasFlag(LoginFlag.ImgChili)) ret &= ImgChili.LoggedIn;
			if (flag.HasFlag(LoginFlag.ImgRock)) ret &= ImgRock.LoggedIn;
			if (flag.HasFlag(LoginFlag.PixSense)) ret &= PixSense.LoggedIn;
			if (flag.HasFlag(LoginFlag.MEGA)) ret &= MegaLoggedIn;
			if (flag.HasFlag(LoginFlag.Rapidgator)) ret &= RapidgatorLoggedIn;
			if (flag.HasFlag(LoginFlag.UploadGIG)) ret &= UploadGIGLoggedIn;
			if (flag.HasFlag(LoginFlag.Zippyshare)) ret &= Zippyshare.LoggedIn;
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

			UploadInfo.RemoveSignature(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\west.txt");
			UploadInfo.RemoveSignature(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\west_pornbb.txt");

			var rnd = new Random();
			foreach (var uploadInfo in UploadResults)
			{
				var megaLinks = MEGA.GetEnabled ? await MEGA.GetLinks(uploadInfo.Id) : Enumerable.Empty<string>();
				var rapidgatorLinks = Rapidgator.GetEnabled ? await Rapidgator.GetLinks(uploadInfo.Id) : Enumerable.Empty<string>();
				
				if (MEGA.GetEnabled && !megaLinks.Any())
				{
					uploadInfo.Id += "\n(No MEGA links!)";
					continue;
				}
				if (Rapidgator.GetEnabled && !rapidgatorLinks.Any())
				{
					uploadInfo.Id += "\n(No Rapidgator links!)";
					continue;
				}

				if (MEGA.GetEnabled && Rapidgator.GetEnabled && megaLinks.Count() != rapidgatorLinks.Count())
				{
					uploadInfo.Id += "\n(Missing some Mega/Rapidgator links!)";
					continue;
				}

				// TODO: refactor
				var zippyshareLinks = Enumerable.Empty<string>();
				var linkSuccess = true;
				foreach (var fileHost in FileHosts)
				{
					if (!fileHost.Enabled) continue;

					var links = await fileHost.GetLinks(uploadInfo.Id);
					if (!links.Any())
					{
						uploadInfo.Id += $"\n(No {fileHost.Name} links!)";
						linkSuccess = false;
						break;
					}
					if (MEGA.GetEnabled && megaLinks.Count() != links.Count)
					{
						uploadInfo.Id += $"\n(Missing some Mega/{fileHost.Name} links!)";
						linkSuccess = false;
						break;
					}

					zippyshareLinks = links;
				}
				if (!linkSuccess)
				{
					continue;
				}

				var uploadTasks = new List<Task<List<string>>>();

				if (ImgChili.LoggedIn && ImgChili.Enabled)
					uploadTasks.Add(ImgChili.Upload(uploadInfo.UploadList));
				if (ImgRock.LoggedIn && ImgRock.Enabled)
					uploadTasks.Add(ImgRock.Upload(uploadInfo.UploadList));
				if (PixSense.LoggedIn && PixSense.Enabled)
					uploadTasks.Add(PixSense.Upload(uploadInfo.UploadList));

				var taskCount = uploadTasks.Count;

				var uploadCount = 0;
				while (uploadTasks.Count > 0)
				{
					try
					{
						var finishedTask = await Task.WhenAny(uploadTasks);
						uploadTasks.Remove(finishedTask);

						var links = await Task.Run(() => finishedTask);
						if (links == null) continue;

						if (links[0].Contains("imgchili"))
						{
							uploadInfo.WebLinks1 = links[0].Trim();
							uploadInfo.ForumLinks1 = ToLowerForumCode(links[1].Trim());
							uploadCount++;
						}
						else if (links[0].Contains("imgrock"))
						{
							uploadInfo.WebLinks2 = links[0].Trim();
							uploadInfo.ForumLinks2 = links[1].Trim();
							uploadCount++;
						}
						else if (links[0].Contains("pixsense") || links[0].Contains("iceimg") || links[0].Contains("imgvip"))
						{
							uploadInfo.WebLinks3 = links[0].Trim();
							uploadInfo.ForumLinks3 = links[1].Trim();
							uploadCount++;
						}
					}
					catch (Exception ex)
					{
						if (ex.Message.Contains(ImgChili.Name))
							uploadInfo.WarningBrush1 = Brushes.Red;
						else if (ex.Message.Contains(ImgRock.Name))
							uploadInfo.WarningBrush2 = Brushes.Red;
						else if (ex.Message.Contains(PixSense.Name))
							uploadInfo.WarningBrush3 = Brushes.Red;

						uploadInfo.IdColor = Colors.Red;
						Message = ex.InnerException.Message;
					}
				}

				try
				{
					if (Rapidgator.GetEnabled)
					{
						await uploadInfo.WriteOutput(string.Join("\\n", megaLinks), string.Join("\\n", rapidgatorLinks));
					}
					else if (Zippyshare.Enabled)
					{
						await uploadInfo.WriteOutput(string.Join("\\n", megaLinks), string.Join("\\n", zippyshareLinks));
					}
					else
					{
						await uploadInfo.WriteOutput(string.Join("\\n", megaLinks), string.Join("\\n", rapidgatorLinks));
					}
				}
				catch (Exception ex)
				{
					Logger.Error(string.Format("Failed to write output: {0}", uploadInfo.Id), ex);
					if (ex.InnerException != null)
						Logger.Error(string.Format("Failed to write output (InnerException): {0}", uploadInfo.Id), ex.InnerException);
				}

				// Delete or Move images
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
							Logger.Error(string.Format("Failed to delete images: {0}", uploadInfo.Id), ex);
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
							Logger.ErrorException(string.Format("Failed to move images: {0}", uploadInfo.Id), ex);
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

			UploadInfo.WriteSignature(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\west.txt");
			UploadInfo.WriteSignature(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\west_pornbb.txt");
			IsUploadFinished = true;
			Message = "Upload done";
		}

		private string ToLowerForumCode(string code)
		{
			return code.Replace("[URL", "[url").Replace("URL]", "url]").Replace("IMG]", "img]");
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
						coverTask = VideoCover.DownloadCovers(file.FileName);
						videoTasks.Add(coverTask);
					}

					await Task.WhenAll(videoTasks);

					if (rarTask != null)
						file.RarStatus = await rarTask ? "OK" : "Error";
					if (thumbnailTask != null)
						file.ThumbnailStatus = await thumbnailTask ? "OK" : "Error";
					if (coverTask != null)
						file.CoverStatus = await coverTask ? "OK" : "Error";
				}
				catch (Exception ex)
				{
					Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
				}
			}
		}

		public async void CreateMegaAccount(object sender)
		{
			var mailHost = sender as IMailHost;
			if(mailHost == null) return;
			
			Message = "Create new mega account...";
			var account = await MEGA.CreateNewAccount(mailHost);

			if (string.IsNullOrEmpty(account))
			{
				Message = "Failed to create account!";
				return;
			}

			Message = MEGA.SetAccountInfo(account) ? 
				"Account created & set!" : "Account created but failed to set!";
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
