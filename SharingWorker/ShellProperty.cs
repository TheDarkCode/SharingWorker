using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using SharingWorker.FileHost;
using SharingWorker.ImageHost;
using SharingWorker.MailHost;
using SharingWorker.UrlShortening;
using SharingWorker.Video;

namespace SharingWorker
{
	[Flags]
	public enum LoginFlag
	{
		Blogger = 1,
		MEGA = 1 << 1,
		Rapidgator = 1 << 2,
		UploadGIG = 1 << 3,
		ImgChili = 1 << 4,
		ImgRock = 1 << 5,
		PixSense = 1 << 6,
		Zippyshare = 1 << 8,
	}

	partial class ShellViewModel : IShell
	{
		public static ImgChili ImgChili { get; set; }
		public static ImgRock ImgRock { get; set; }
		public static PixSense PixSense { get; set; }
		public List<IMailHost> MailHosts { get; set; }
		public List<IUrlShortening> UrlShortenings { get; set; }
		public List<IFileHost> FileHosts { get; set; }
		public BindableCollection<UploadInfo> UploadResults { get; set; }
		public RarListViewModel RarList { get; set; }
		public static bool IsUploadFinished;

		public IFileHost Zippyshare
		{
			get { return FileHosts.FirstOrDefault(f => f.Name == "Zippyshare"); }
		}

		private string message;
		public string Message
		{
			get { return message; }
			set
			{
				message = value;
				NotifyOfPropertyChange(() => Message);
			}
		}

		private bool canUpload;
		public bool CanUpload
		{
			get { return canUpload; }
			set
			{
				canUpload = value;
				NotifyOfPropertyChange(() => CanUpload);
			}
		}

		private bool megaLoggedIn;
		public bool MegaLoggedIn
		{
			get { return megaLoggedIn; }
			set
			{
				megaLoggedIn = value;
				NotifyOfPropertyChange(() => MegaLoggedIn);
			}
		}

		private bool rapidgatorLoggedIn;
		public bool RapidgatorLoggedIn
		{
			get { return rapidgatorLoggedIn; }
			set
			{
				rapidgatorLoggedIn = value;
				NotifyOfPropertyChange(() => RapidgatorLoggedIn);
			}
		}

		private bool uploadGIGLoggedIn;
		public bool UploadGIGLoggedIn
		{
			get { return uploadGIGLoggedIn; }
			set
			{
				uploadGIGLoggedIn = value;
				NotifyOfPropertyChange(() => UploadGIGLoggedIn);
			}
		}
		
		private bool bloggerLoggedIn;
		public bool BloggerLoggedIn
		{
			get { return bloggerLoggedIn; }
			set
			{
				bloggerLoggedIn = value;
				NotifyOfPropertyChange(() => BloggerLoggedIn);
			}
		}

		private bool compressToRar;
		public bool CompressToRar
		{
			get { return compressToRar; }
			set
			{
				compressToRar = value;
				NotifyOfPropertyChange(() => CompressToRar);
			}
		}

		private bool getCover;
		public bool GetCover
		{
			get { return getCover; }
			set
			{
				getCover = value;
				NotifyOfPropertyChange(() => GetCover);
			}
		}

		private bool getThumbnail;
		public bool GetThumbnail
		{
			get { return getThumbnail; }
			set
			{
				getThumbnail = value;
				NotifyOfPropertyChange(() => GetThumbnail);
			}
		}

		private bool canProcess;
		public bool CanProcess
		{
			get { return canProcess; }
			set
			{
				canProcess = value;
				NotifyOfPropertyChange(() => CanProcess);
			}
		}

		private List<UploadInfo> selectedUploadInfos = new List<UploadInfo>();
		public List<UploadInfo> SelectedUploadInfos
		{
			get { return selectedUploadInfos; }
			set
			{
				selectedUploadInfos = value;
				NotifyOfPropertyChange(() => SelectedUploadInfos);
			}
		}
		
		public bool GetMega
		{
			get { return MEGA.GetEnabled; }
			set
			{
				MEGA.GetEnabled = value;
				NotifyOfPropertyChange(() => GetMega);
			}
		}

		public bool GetRapidgator
		{
			get { return Rapidgator.GetEnabled; }
			set
			{
				Rapidgator.GetEnabled = value;
				NotifyOfPropertyChange(() => GetRapidgator);
			}
		}

		public bool GetUploadGIG
		{
			get { return UploadGIG.GetEnabled; }
			set
			{
				UploadGIG.GetEnabled = value;
				NotifyOfPropertyChange(() => GetUploadGIG);
			}
		}
	}

	internal class EnumToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null) return false;

			string checkValue = value.ToString();
			string targetValue = parameter.ToString();
			return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null) return Binding.DoNothing;

			bool useValue = (bool)value;
			string targetValue = parameter.ToString();
			if (useValue)
				return Enum.Parse(targetType, targetValue);

			return Binding.DoNothing;
		}
	}
}
