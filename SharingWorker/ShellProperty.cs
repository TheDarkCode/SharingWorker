using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Caliburn.Micro;
using SharingWorker.FileHost;
using SharingWorker.ImageHost;
using SharingWorker.MailHost;
using SharingWorker.Post;
using SharingWorker.Video;

namespace SharingWorker
{
    [Flags]
    public enum LoginFlag
    {
        ImgChili = 1,
        ImgSpice = 1 << 2,
        MEGA = 1 << 3,
        Uploadable = 1 << 4,
        Blogger = 1 << 5,
        ImgDrive = 1 << 6,
        ImgRock = 1 << 7,
        ImgMega = 1 << 8,
        Rapidgator = 1 << 9,
    }

    partial class ShellViewModel : IShell
    {
        public static ImgChili ImgChili { get; set; }
        public static ImgSpice ImgSpice { get; set; }
        public static ImgMega ImgMega { get; set; }
        public static ImgDrive ImgDrive { get; set; }
        public static ImgRock ImgRock { get; set; }
        public BindableCollection<UploadInfo> UploadResults { get; set; }
        public RarListViewModel RarList { get; set; }
        public SharedFilesViewModel SharedFiles { get; set; }
        public static bool IsUploadFinished;

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

        private bool uploadableLoggedIn;
        public bool UploadableLoggedIn
        {
            get { return uploadableLoggedIn; }
            set
            {
                uploadableLoggedIn = value;
                NotifyOfPropertyChange(() => UploadableLoggedIn);
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

        public bool CheckMega
        {
            get { return MEGA.CheckEnabled; }
            set
            {
                MEGA.CheckEnabled = value;
                NotifyOfPropertyChange(() => CheckMega);
            }
        }

        public bool CheckUploadable
        {
            get { return Uploadable.CheckEnabled; }
            set
            {
                Uploadable.CheckEnabled = value;
                NotifyOfPropertyChange(() => CheckUploadable);
            }
        }

        public bool CheckBinbox
        {
            get { return Binbox.CheckEnabled; }
            set
            {
                Binbox.CheckEnabled = value;
                NotifyOfPropertyChange(() => CheckBinbox);
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

        public bool GetUploadable
        {
            get { return Uploadable.GetEnabled; }
            set
            {
                Uploadable.GetEnabled = value;
                NotifyOfPropertyChange(() => GetUploadable);
            }
        }
        
        public bool GetBinBox
        {
            get { return Binbox.GetEnabled; }
            set
            {
                Binbox.GetEnabled = value;
                NotifyOfPropertyChange(() => GetBinBox);
            }
        }

        public bool GetLinkbucks
        {
            get { return LinkBucks.GetEnabled; }
            set
            {
                LinkBucks.GetEnabled = value;
                NotifyOfPropertyChange(() => GetLinkbucks);
            }
        }

        private MailSource mailSource;
        public MailSource MailSource
        {
            get { return mailSource; }
            set
            {
                mailSource = value;
                NotifyOfPropertyChange(() => MailSource);
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
