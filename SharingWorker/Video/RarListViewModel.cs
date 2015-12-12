using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;

namespace SharingWorker.Video
{
    public class RarListItem : PropertyChangedBase
    {
        public string Path;
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                NotifyOfPropertyChange(() => FileName);
            }
        }
        private string rarStatus;
        public string RarStatus
        {
            get { return rarStatus; }
            set
            {
                rarStatus = value;
                NotifyOfPropertyChange(() => RarStatus);
                NotifyOfPropertyChange(() => RarStatusColor);
            }
        }
        
        private string thumbnailStatus;
        public string ThumbnailStatus
        {
            get { return thumbnailStatus; }
            set
            {
                thumbnailStatus = value;
                NotifyOfPropertyChange(() => ThumbnailStatus);
                NotifyOfPropertyChange(() => ThumbnailStatusColor);
            }
        }

        private string coverStatus;
        public string CoverStatus
        {
            get { return coverStatus; }
            set
            {
                coverStatus = value;
                NotifyOfPropertyChange(() => CoverStatus);
                NotifyOfPropertyChange(() => CoverStatusColor);
            }
        }

        public Brush RarStatusColor
        {
            get { return rarStatus == "OK" ? Brushes.ForestGreen : Brushes.OrangeRed; }
        }

        public Brush ThumbnailStatusColor
        {
            get { return thumbnailStatus == "OK" ? Brushes.ForestGreen : Brushes.OrangeRed; }
        }

        public Brush CoverStatusColor
        {
            get { return coverStatus == "OK" ? Brushes.ForestGreen : Brushes.OrangeRed; }
        }
    }

    class RarListViewModel : Screen
    {
        public BindableCollection<RarListItem> FileList { get; set; }

        private List<RarListItem> selectedFiles = new List<RarListItem>();
        public List<RarListItem> SelectedFiles
        {
            get { return selectedFiles; }
            set
            {
                selectedFiles = value;
                NotifyOfPropertyChange(() => SelectedFiles);
            }
        }

        [ImportingConstructor]
        public RarListViewModel()
        {
            this.DisplayName = "Video List";
            FileList = new BindableCollection<RarListItem>();
        }

        public override void CanClose(Action<bool> callback)
        {
            SelectedFiles.Clear();
            FileList.Clear();
            base.CanClose(callback);
        }

        public void RemoveItem()
        {
            FileList.RemoveRange(SelectedFiles);
        }

        public void RemoveAll()
        {
            FileList.Clear();
        }

        public void CheckDuplicateName()
        {
            foreach (var duplicates in FileList.GroupBy(item => item.FileName).Where(group => group.Count() > 1))
            {
                foreach (var dup in duplicates)
                {
                    dup.FileName = VideoInfo.AddPartName(Path.GetFileNameWithoutExtension(dup.Path), dup.FileName);
                }
            }
        }
    }
}
