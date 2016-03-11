using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using SharingWorker.FileHost;
using SharingWorker.Post;
using SharingWorker.Video;

namespace SharingWorker
{
    class UploadImage
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public byte[] Data { get; set; }
    }

    class ImageLink
    {
        public string WebLinks = null;
        public string ForumLinks = null;
    }

    class LinksBackup
    {
        public string Id { get; set; }
        public string Links { get; set; }
        public string PostUri { get; set; }
    }

    class UploadInfo : PropertyChangedBase
    {
        private static int imageHostIndex;
        private static readonly Random rnd = new Random();

        private static readonly string outputPath_l =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\share_l.txt";

        private static readonly string outputPath_lh =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\share_lh.txt";

        private static readonly string outputPath_w =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\share_w.txt";

        private static readonly string outputPath_blog =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\share_blog.txt";

        public static readonly string[] UploadPaths = ConfigurationManager.AppSettings["UploadFolder"].Split(';');

        public List<UploadImage> UploadList { get; set; }
        public string BackupBinboxLink;

        public UploadInfo(string id = null)
        {
            Id = id;
            idColor = Colors.White;

            WarningBrush1 = SystemColors.ActiveBorderBrush;
            WarningBrush2 = SystemColors.ActiveBorderBrush;
            WarningBrush3 = SystemColors.ActiveBorderBrush;
            WarningBrush4 = SystemColors.ActiveBorderBrush;
        }

        private string id;

        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                NotifyOfPropertyChange(() => Id);
            }
        }

        private Color idColor;

        public Color IdColor
        {
            get { return idColor; }
            set
            {
                idColor = value;
                NotifyOfPropertyChange(() => IdColor);
            }
        }

        private SolidColorBrush warningBrush1;

        public SolidColorBrush WarningBrush1
        {
            get { return warningBrush1; }
            set
            {
                warningBrush1 = value;
                NotifyOfPropertyChange(() => WarningBrush1);
            }
        }

        private SolidColorBrush warningBrush2;

        public SolidColorBrush WarningBrush2
        {
            get { return warningBrush2; }
            set
            {
                warningBrush2 = value;
                NotifyOfPropertyChange(() => WarningBrush2);
            }
        }

        private SolidColorBrush warningBrush3;

        public SolidColorBrush WarningBrush3
        {
            get { return warningBrush3; }
            set
            {
                warningBrush3 = value;
                NotifyOfPropertyChange(() => WarningBrush3);
            }
        }

        private SolidColorBrush warningBrush4;

        public SolidColorBrush WarningBrush4
        {
            get { return warningBrush4; }
            set
            {
                warningBrush4 = value;
                NotifyOfPropertyChange(() => WarningBrush4);
            }
        }

        private string webLinks1;

        public string WebLinks1
        {
            get { return webLinks1; }
            set
            {
                webLinks1 = value;
                NotifyOfPropertyChange(() => WebLinks1);
            }
        }

        private string webLinks2;

        public string WebLinks2
        {
            get { return webLinks2; }
            set
            {
                webLinks2 = value;
                NotifyOfPropertyChange(() => WebLinks2);
            }
        }

        private string webLinks3;

        public string WebLinks3
        {
            get { return webLinks3; }
            set
            {
                webLinks3 = value;
                NotifyOfPropertyChange(() => WebLinks3);
            }
        }

        private string webLinks4;

        public string WebLinks4
        {
            get { return webLinks4; }
            set
            {
                webLinks4 = value;
                NotifyOfPropertyChange(() => WebLinks4);
            }
        }

        private string forumLinks1;

        public string ForumLinks1
        {
            get { return forumLinks1; }
            set
            {
                forumLinks1 = value;
                NotifyOfPropertyChange(() => ForumLinks1);
            }
        }

        private string forumLinks2;

        public string ForumLinks2
        {
            get { return forumLinks2; }
            set
            {
                forumLinks2 = value;
                NotifyOfPropertyChange(() => ForumLinks2);
            }
        }

        private string forumLinks3;

        public string ForumLinks3
        {
            get { return forumLinks3; }
            set
            {
                forumLinks3 = value;
                NotifyOfPropertyChange(() => ForumLinks3);
            }
        }

        private string forumLinks4;

        public string ForumLinks4
        {
            get { return forumLinks4; }
            set
            {
                forumLinks4 = value;
                NotifyOfPropertyChange(() => ForumLinks4);
            }
        }

        public async Task WriteOutput(string megaLinks, string rgLinks)
        {
            string imageCode = null;
            string imageCodeBlog = null;
            bool isCensored = true;

            var title = id;
            if (char.IsDigit(id, 0) || id.Contains("heyzo") || id.Contains("TokyoHot") || id.Contains("gachi") || id.Contains("XXX-AV")
                || id.Contains("H0930") || id.Contains("h0930") || id.Contains("H4610") || id.Contains("h4610") || id.Contains("C0930") || id.Contains("c0930")
                || id.Contains("heydouga") || id.Contains("av-sikou"))
            {
                isCensored = false;
            }
            else
            {
                title = id.ToUpper();
                int dash = 0;
                for (int i = 0; i < title.Length - 1; i++)
                {
                    if (char.IsLetter(title[i]))
                    {
                        if (!char.IsDigit(title[i + 1])) continue;
                        dash = i + 1;
                    }
                }
                if (dash > 0) title = title.Insert(dash, "-");
            }

            switch (imageHostIndex%4)
            {
                case 0:
                    imageCode = ForumLinks1;
                    imageCodeBlog = WebLinks2;
                    break;
                case 1:
                    imageCode = ForumLinks2;
                    imageCodeBlog = WebLinks3;
                    break;
                case 2:
                    imageCode = ForumLinks3;
                    imageCodeBlog = WebLinks4;
                    break;
                case 3:
                    imageCode = ForumLinks4;
                    imageCodeBlog = WebLinks1;
                    break;
            }

            if (string.IsNullOrEmpty(imageCode))
            {
                imageCode =
                    new List<string> {ForumLinks4, ForumLinks2, ForumLinks3, ForumLinks1}.Where(
                        links => !string.IsNullOrEmpty(links)).Random();
            }
            if (string.IsNullOrEmpty(imageCodeBlog))
            {
                imageCodeBlog =
                    new List<string> {WebLinks4, WebLinks2, WebLinks3, WebLinks1}.Where(
                        links => !string.IsNullOrEmpty(links)).Random();
            }

            imageHostIndex++;

            try
            {
                string filePath = null;
                string uploadPath = null;

                foreach (var path in UploadPaths)
                {
                    if (!Directory.Exists(path)) continue;
                    filePath =
                        Directory.GetFiles(path, string.Format("{0}*.part*.rar", id), SearchOption.TopDirectoryOnly)
                            .FirstOrDefault();
                    if (filePath == null) continue;
                    uploadPath = path;
                    break;
                }

                string fileFormat = null, fileSize = null;
                if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(uploadPath))
                {
                    fileFormat = VideoUtil.GetFileFormat(filePath);
                    fileSize = VideoUtil.GetFileSizeGB(uploadPath, id) + "GB";
                }

                string links1 = null, links2 = null;
                switch (rnd.Next(0, 1000)%2)
                {
                    case 0:
                        links1 = megaLinks;
                        links2 = rgLinks;
                        break;
                    case 1:
                        links1 = rgLinks;
                        links2 = megaLinks;
                        break;
                }

                var content = string.Empty;
                content = string.Format("{0}\\nor\\n{1}", links1, links2);
                content += "\\n\\nAll links are interchangeable and no password.";

                var linksBackup = new LinksBackup
                {
                    Id = id,
                    Links = content,
                };

                if (Binbox.GetEnabled)
                {
                    var binboxLinks = await Binbox.GetEncodedLink(id, content, Binbox.ApiUrlType.Main);
                    BackupBinboxLink = await Binbox.GetEncodedLink(id, content, Binbox.ApiUrlType.Backup);

                    var linkbucksLinks = string.Empty;
                    if (LinkBucks.GetEnabled)
                    {
                        linkbucksLinks = await LinkBucks.GetLinkbucksSingle(id, binboxLinks);
                        if (string.IsNullOrEmpty(linkbucksLinks)) linkbucksLinks = binboxLinks;
                    }

                    File.AppendAllText(string.Format("Binbox_Backup_{0}.log", DateTime.Now.ToString("yyyy-MM")),
                        string.Format("{0} | {1} | {2}", BackupBinboxLink, id,
                            DateTime.Now.ToString("yyyy-MM-dd") + Environment.NewLine));

                    await
                        GenerateOutput(title, fileSize, fileFormat, imageCode, imageCodeBlog, linkbucksLinks,
                            binboxLinks, linksBackup, isCensored);
                }
                else
                {
                    content = content.Replace("\\n", Environment.NewLine);
                    await
                        GenerateOutput(title, fileSize, fileFormat, imageCode, imageCodeBlog, content, string.Empty,
                            linksBackup, isCensored);
                }

                GenerateScanLover(rgLinks.Replace("\\n", Environment.NewLine));
                GenerateWestern(rgLinks.Replace("\\n", Environment.NewLine), fileSize, fileFormat, imageCode);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex.Message);
            }
        }

        private async Task GenerateOutput(string title, string fileSize, string fileFormat, string imageCode,
            string imageCodeBlog, string linkbucksLinks, string binboxLinks, LinksBackup linksBackup , bool isCensored)
        {
            var videoInfoTw = await VideoInfo.QueryVideoInfo(title, QueryLang.TW);
            //var videoInfoEn = await VideoInfo.QueryVideoInfo(title, QueryLang.EN);

            var blogTitle = "";
            if (videoInfoTw.Title.Contains("Tokyo Hot"))
            {
                blogTitle = string.Format("{0}", videoInfoTw.Title.Replace("Tokyo Hot", "Tokyo-Hot"));
            }
            else
            {
                blogTitle = string.Format("{0} ({1})", videoInfoTw.Title, title);
                if (blogTitle.Count(c => c == '-') == 1 && !blogTitle.Contains('_'))
                    blogTitle = blogTitle.Replace("-", "");
            }

            if (Binbox.GetEnabled)
                Blogger.AddPost(new Blogger.BlogPost(blogTitle, imageCodeBlog, linkbucksLinks, linksBackup));
            else
                Blogger.AddPost(new Blogger.BlogPost(blogTitle, imageCodeBlog, "", linksBackup));

            var censored = isCensored ? "有碼" : "無碼";
            var wTitle = string.Format(" ({0})", title);
            if (title.Contains("1pon") || title.Contains("carib")) wTitle = string.Empty;
            var content = string.Format(@"{3}

[color=green][b]Download (Mega.co.nz & Rapidgator)：[/b][/color]
[url]{4}[/url]

[b][color=#cc0000]nanamiyusa's Collection[/color] [/b]: [url=http://www.epc-jav.com]Erotic Public Cloud[/url]

==

", wTitle, fileSize, fileFormat, imageCode, binboxLinks);

            File.AppendAllText(outputPath_w, content);

            content = string.Format(@"{5} ({0}) [{1}/{2}][多空]

[color=green][b]【影片名稱】：[/b][/color]{5} ({0})

[color=green][b]【出演女優】：[/b][/color]{6}

[color=green][b]【檔案大小】：[/b][/color]{1}

[color=green][b]【影片時間】：[/b][/color]120 Min

[color=green][b]【檔案格式】：[/b][/color]{2}

[color=green][b]【下載空間】：[/b][/color]Mega & Rapidgator

[color=green][b]【有／無碼】：[/b][/color]{7}

[color=green][b]【圖片預覽】：[/b][/color]
{3}

[color=green][b]【檔案載點】：[/b][/color]
[hide]{4}[/hide]

==

", title, fileSize, fileFormat, imageCode, binboxLinks, videoInfoTw.Title, videoInfoTw.Actresses, censored);

            File.AppendAllText(outputPath_lh, content);
            File.AppendAllText(outputPath_l, content.Replace("[hide]", string.Empty).Replace("[/hide]", string.Empty));

            content = string.Format("{0} ({1})", videoInfoTw.Title, title) + Environment.NewLine + Environment.NewLine +
                      "<div style='text-align: center;'>" +
                      imageCodeBlog +
                      "</div>" +
                      "Download (Mega.co.nz, Rapidgator) : <br /><!--more-->" +
                      "<a href=\"" + linkbucksLinks + "\">" + linkbucksLinks + "</a>"
                      + Environment.NewLine + Environment.NewLine +
                      "==" + Environment.NewLine + Environment.NewLine;

            File.AppendAllText(outputPath_blog, content);
        }

        private void GenerateScanLover(string rgLinks)
        {
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\scanlover.txt";
            var content = string.Format(@"[color=green][b]Download：[/b][/color]
{0}

[b][color=#cc0000]nanamiyusa's Collection[/color] [/b]: [url=http://www.epc-jav.com]Erotic Public Cloud[/url]

==

", rgLinks);

            File.AppendAllText(outputPath, content);
        }

        private void GenerateWestern(string rgLinks, string fileSize, string fileFormat, string imageCode)
        {
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\west.txt";
            var content = string.Format(@"{0}

Size: {1}
Format: {2}

[color=green][b]Download：[/b][/color]
{3}

==

",imageCode, fileSize, fileFormat, rgLinks);

            File.AppendAllText(outputPath, content);
        }
    }
}