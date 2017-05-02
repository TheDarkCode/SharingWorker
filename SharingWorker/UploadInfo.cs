using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using SharingWorker.FileHost;
using SharingWorker.Post;
using SharingWorker.Video;

namespace SharingWorker
{
    public class UploadImage
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

        public static string SecondHostName
        {
            get
            {
                if (Rapidgator.GetEnabled)
                    return "Rapidgator";
                if (Bigfile.GetEnabled)
                    return "Bigfile";
                if (Datafile.GetEnabled)
                    return "Datafile";
                return string.Empty;
            }
        }

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
            var megaBackup = megaLinks;
            var rgBackup = rgLinks;

            var title = id;
            if (((char.IsDigit(id, 0) && !id.StartsWith("00")) || id.Contains("heyzo") || id.Contains("TokyoHot") || id.Contains("gachi") || id.Contains("XXX-AV")
                || id.Contains("H0930") || id.Contains("h0930") || id.Contains("H4610") || id.Contains("h4610") || id.Contains("C0930") || id.Contains("c0930")
                || id.Contains("heydouga") || id.Contains("av-sikou") || id.Contains("fc2-ppv") || id.StartsWith("lp_"))
                && !id.Contains("200GANA") && !id.Contains("259LUXU") && !id.Contains("215LES") && !id.Contains("261ARA"))
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

            var flinks = new Dictionary<int, string>
            {
                { 0, ForumLinks1 },
                { 1, ForumLinks2 },
                { 2, ForumLinks3 },
                //{ 3, ForumLinks4 },
            };
            var wlinks = new Dictionary<int, string>
            {
                { 0, WebLinks1 },
                { 1, WebLinks2 },
                { 2, WebLinks3 },
                //{ 3, WebLinks4 },
            };

            if (flinks.Count(p => !string.IsNullOrEmpty(p.Value)) == 1 || wlinks.Count(p => !string.IsNullOrEmpty(p.Value)) == 1)
            {
                imageCode = flinks.First().Value;
                imageCodeBlog = wlinks.First().Value;
            }
            else
            {
                var fIndex = imageHostIndex % flinks.Count;
                var wIndex = fIndex < flinks.Count - 1 ? fIndex + 1 : 0;

                imageCode = flinks[fIndex];
                imageCodeBlog = wlinks[wIndex];

                if (string.IsNullOrEmpty(imageCode) || string.IsNullOrEmpty(imageCodeBlog))
                {
                    imageCode = flinks.Where(l => !string.IsNullOrEmpty(l.Value)).Select(l => l.Value).Random();
                    var imgCodeIndex = flinks.FirstOrDefault(l => l.Value == imageCode).Key;
                    imageCodeBlog = wlinks.Where(l => l.Key != imgCodeIndex && !string.IsNullOrEmpty(l.Value)).Select(l => l.Value).Random();
                }
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

                //
                if (ShinkIn.GetEnabled && !string.IsNullOrEmpty(megaLinks))
                {
                    var allLinks = megaLinks.Split(new string[] { "\\n" }, StringSplitOptions.None);
                    var firstLine = true;
                    var sb = new StringBuilder();
                    foreach (var link in allLinks)
                    {
                        if (firstLine)
                        {
                            var firstLink = await Ouo.GetLink(link);
                            if (string.IsNullOrEmpty(firstLink) || firstLink.Length > 30)
                            {
                                firstLink = await ShinkIn.GetLink(link);
                                if (string.IsNullOrEmpty(firstLink))
                                    firstLink = link;
                            }

                            sb.Append(string.Format("{0}\\n", firstLink));
                            firstLine = false;
                        }
                        else
                        {
                            sb.Append(string.Format("{0}\\n", link));
                        }
                    }
                    megaLinks = sb.ToString().TrimEnd("\\n".ToCharArray());
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
                content = content.Replace("\\n", "<br />");

                var contentBackup = string.Format("{0}\\nor\\n{1}", megaBackup, rgBackup);
                contentBackup += "\\n\\nAll links are interchangeable and no password.";
                contentBackup = contentBackup.Replace("\\n", "<br />");

                var linksBackup = new LinksBackup
                {
                    Id = id,
                    Links = contentBackup,
                };

                var shinkinLink = string.Empty;
                var ouoLink = string.Empty;

                var linksPage = Blogger.CreateLinksPost(content);
                if (string.IsNullOrEmpty(linksPage))
                {
                    await
                        GenerateOutput(title, fileSize, fileFormat, imageCode, imageCodeBlog, linksPage, linksBackup, isCensored);
                }
                else
                {
                    if (ShinkIn.GetEnabled && Ouo.GetEnabled)
                    {
                        shinkinLink = await GetLinksPageShink(linksPage);
                        ouoLink = await Ouo.GetLink(linksPage);
                    }
                    else if (ShinkIn.GetEnabled && !Ouo.GetEnabled)
                    {
                        shinkinLink = await GetLinksPageShink(linksPage);
                    }
                    else if (!ShinkIn.GetEnabled && Ouo.GetEnabled)
                    {
                        ouoLink = await Ouo.GetLink(linksPage);
                    }

                    await
                        GenerateOutput(title, fileSize, fileFormat, imageCode, imageCodeBlog, linksPage, linksBackup, isCensored, shinkinLink, ouoLink);
                }

                if (!string.IsNullOrEmpty(linksPage) && string.IsNullOrEmpty(ouoLink))
                    ouoLink = await Ouo.GetLink(linksPage);

                GenerateJavLibrary(ouoLink, fileSize, fileFormat, imageCode);
                GenerateWestern(ouoLink, shinkinLink, fileSize, fileFormat, imageCode);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex.Message);
            }
        }

        private async Task<string> GetLinksPageShink(string linksPage)
        {
            var ret = await ShinkIn.GetLink(linksPage);
            if (string.IsNullOrEmpty(ret) || ret.Length > 30)
            {
                await Task.Delay(3000);
                ret = await ShinkIn.GetLink(linksPage);
            }
            return ret;
        }

        private void GenerateBlogPost(string title, string imageCodeBlog, string linksPage, string shinkinLink, string ouoLink, LinksBackup linksBackup, VideoInfo videoInfo)
        {
            var blogTitle = "";
            if (videoInfo.Title.Contains("Tokyo Hot"))
            {
                blogTitle = string.Format("{0}", videoInfo.Title.Replace("Tokyo Hot", "Tokyo-Hot"));
            }
            else
            {
                blogTitle = string.Format("[{1}] {0}", videoInfo.Title, title);
                if (blogTitle.Count(c => c == '-') == 1 && !blogTitle.Contains('_'))
                    blogTitle = blogTitle.Replace("-", "");
            }
            blogTitle = RemoveTitle(title, blogTitle);

            var linksContent = linksPage;
            if (!string.IsNullOrEmpty(shinkinLink) && !string.IsNullOrEmpty(ouoLink))
            {
                linksContent = "<a href=\"" + shinkinLink + "\">" + shinkinLink + "</a><br />or<br /><a href=\"" + ouoLink + "\">" + ouoLink + "</a>";
            }
            else if (!string.IsNullOrEmpty(shinkinLink) && string.IsNullOrEmpty(ouoLink))
            {
                linksContent = "<a href=\"" + shinkinLink + "\">" + shinkinLink + "</a>";
            }
            else if (string.IsNullOrEmpty(shinkinLink) && !string.IsNullOrEmpty(ouoLink))
            {
                linksContent = "<a href=\"" + ouoLink + "\">" + ouoLink + "</a>";
            }

            Blogger.AddPost(new Blogger.BlogPost(blogTitle, imageCodeBlog, linksContent, linksBackup));

            var content = string.Format("{0} ({1})", videoInfo.Title, title) + Environment.NewLine + Environment.NewLine +
                    "<div style='text-align: center;'>" +
                    imageCodeBlog +
                    "</div>" +
                    string.Format("Download(Mega.nz, {0}) : < br />< !--more-- > ", SecondHostName) +
                    linksContent
                    + Environment.NewLine + Environment.NewLine +
                    "==" + Environment.NewLine + Environment.NewLine;

            File.AppendAllText(outputPath_blog, content);
        }

        private async Task GenerateOutput(string title, string fileSize, string fileFormat, string imageCode,
            string imageCodeBlog, string linksPage, LinksBackup linksBackup , bool isCensored,
            string shinkinLink = null, string ouoLink = null)
        {
            var videoInfoTw = await VideoInfo.QueryVideoInfo(title, QueryLang.TW);

            GenerateBlogPost(title, imageCodeBlog, linksPage, shinkinLink, ouoLink, linksBackup, videoInfoTw);

            var censored = isCensored ? "有碼" : "無碼";
            var linksContent = linksPage;
            if (!string.IsNullOrEmpty(shinkinLink) && !string.IsNullOrEmpty(ouoLink))
            {
                linksContent = shinkinLink + Environment.NewLine + "or" + Environment.NewLine + ouoLink;
            }
            else if (!string.IsNullOrEmpty(shinkinLink) && string.IsNullOrEmpty(ouoLink))
            {
                linksContent = shinkinLink;
            }
            else if (string.IsNullOrEmpty(shinkinLink) && !string.IsNullOrEmpty(ouoLink))
            {
                linksContent = ouoLink;
            }

            var content = string.Format(@"{5} ({0}) [{1}/{2}][多空]

[color=green][b]【影片名稱】：[/b][/color]{5} ({0})

[color=green][b]【出演女優】：[/b][/color]{6}

[color=green][b]【檔案大小】：[/b][/color]{1}

[color=green][b]【影片時間】：[/b][/color]120 Min

[color=green][b]【檔案格式】：[/b][/color]{2}

[color=green][b]【下載空間】：[/b][/color]Mega & {8}

[color=green][b]【有／無碼】：[/b][/color]{7}

[color=green][b]【圖片預覽】：[/b][/color]
{3}

[color=green][b]【檔案載點】：[/b][/color]
[hide]
{4}
[/hide]

==

", title, fileSize, fileFormat, imageCode, linksContent, videoInfoTw.Title, videoInfoTw.Actresses, censored, SecondHostName);

            content = RemoveTitle(title, content);

            File.AppendAllText(outputPath_lh, content);
            File.AppendAllText(outputPath_l, content.Replace(Environment.NewLine + "[hide]", string.Empty).Replace("[/hide]" + Environment.NewLine, string.Empty));

    //        var blogLinks = ShinkIn.GetEnabled
    //? string.Format("<a href=\"{0}\">{0}</a>", cashLinks) : cashLinks;

    //        content = string.Format("{0} ({1})", videoInfoTw.Title, title) + Environment.NewLine + Environment.NewLine +
    //                "<div style='text-align: center;'>" +
    //                imageCodeBlog +
    //                "</div>" +
    //                string.Format("Download(Mega.nz, {0}) : < br />< !--more-- > ", SecondHostName) +
    //                blogLinks
    //                + Environment.NewLine + Environment.NewLine +
    //                "==" + Environment.NewLine + Environment.NewLine;

    //        File.AppendAllText(outputPath_blog, content);
        }

        private string RemoveTitle(string title, string content)
        {
            if (title.Contains("heydouga") || title.Contains("TokyoHot") || title.Contains("zipang") ||
                title.Contains("detail") || title.Contains("h0930") || title.Contains("h4610") || title.Contains("c0930"))
            {
                return content.Replace(string.Format(" ({0})", title), string.Empty);
            }
            return content;
        }

        private void GenerateWestern(string ouoLink, string shinkLink, string fileSize, string fileFormat, string imageCode)
        {
            shinkLink = shinkLink.Replace("shink.in", "shink.me");

            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\west.txt";
            var content = string.Format(@"{0}

Size: {1}
Format: {2}

[color=green][b]Download (Mega.nz & {3})：[/b][/color]
[url]{4}[/url]
or
[url]{5}[/url]

==

", imageCode, fileSize, fileFormat, SecondHostName, shinkLink, ouoLink);

            File.AppendAllText(outputPath, content);
        }

        private void GenerateJavLibrary(string shortLink, string fileSize, string fileFormat, string imageCode)
        {
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\JavLib.txt";
            var content = string.Format(@"{0}

Size: {1}
Format: {2}

[color=green][b]Download (Mega.nz & {4})：[/b][/color]
[url]{3}[/url]

[b][color=#cc0000]nanamiyusa's Collection[/color] [/b]: [url=http://blog.epc-jav.com]Erotic Public Cloud[/url]

==

", imageCode, fileSize, fileFormat, shortLink, SecondHostName);

            File.AppendAllText(outputPath, content);
        }

        public static void WriteSignature(string filePath)
        {
            if (!File.Exists(filePath)) return;
            var lines = File.ReadLines(filePath);
            if (lines.Count() <= 3) return;
            var content = lines.Take(lines.Count() - 2).ToList();

            var signature = @"[b][color=#cc0000]nanamiyusa's Collection[/color] [/b]: [url=http://blog.epc-jav.com]Erotic Public Cloud[/url]";
            content.Add(signature);
            File.WriteAllLines(filePath, content);
        }
    }
}