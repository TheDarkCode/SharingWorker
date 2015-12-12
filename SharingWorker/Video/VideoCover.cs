using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SharingWorker.Video
{
    public static class VideoCover
    {
        public static async Task<bool> GetSnapshot(string sourcePath, string outputName)
        {
            var tempPath = Path.Combine(Path.GetDirectoryName(sourcePath), Path.GetFileNameWithoutExtension(sourcePath) + ".jpg");

            var processInfo = new ProcessStartInfo
            {
                FileName = @"mtn/mtn.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = string.Format("-c 4 -r 7 -w 1280 -P -o .jpg \"{0}\"", sourcePath)
            };

            return await Task.Run(() =>
            {
                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                        process.WaitForExit();

                    var outputPath = outputName + ".jpg";
                    if (File.Exists(tempPath) && !File.Exists(outputPath))
                    {
                        File.Move(tempPath, outputPath);
                        return true;
                    }
                    return false;
                }
            });
        }

        public static async Task<bool> GetCover(string fileName)
        {
            var coverUrls = await GetCoverImageUrl(fileName);
            if (!coverUrls.Any()) return false;

            var ret = true;

            for (int i = 0; i < coverUrls.Count; i++)
            {
                if (!fileName.Contains("mesubuta") && !fileName.Contains("mura") && (fileName.Last() == 'A' || fileName.Last() == 'a')) fileName = fileName.Substring(0, fileName.Length - 1);
                var outputPath = i == 0 ? fileName + "pl.jpg" : fileName + string.Format("pl{0}.jpg", i);
                
                using (var client = new WebClient())
                {
                    try
                    {
                        await client.DownloadFileTaskAsync(coverUrls[i], outputPath);
                        ret &= File.Exists(outputPath);
                    }
                    catch (Exception ex)
                    {
                        App.Logger.Error(ex.Message);
                        ret = false;
                    }
                }
            }
            return ret;
        }

        private static async Task<List<string>> GetCoverImageUrl(string fileName)
        {
            if (fileName.Contains("caribpr"))
            {
                return new List<string> { string.Format("http://www.caribbeancompr.com/moviepages/{0}/images/l_l.jpg", fileName.Replace("-caribpr", string.Empty)) };
            }
            if (fileName.Contains("carib"))
            {
                return new List<string> { string.Format("http://www.caribbeancom.com/moviepages/{0}/images/l_l.jpg", fileName.Replace("-carib", string.Empty)) };
            }
            if (fileName.Contains("1pon"))
            {
                return new List<string> { await Get1ponCover(fileName) };
            }
            if (fileName.Contains("heyzo"))
            {
                var ret = fileName.Replace("heyzo_hd_", string.Empty).Replace("_full", string.Empty);
                return new List<string> { string.Format("http://www.heyzo.com/contents/3000/{0}/images/player_thumbnail.jpg", ret) };
            }
            if (fileName.Contains("10mu"))
            {
                return new List<string> { string.Format("http://www.10musume.com/moviepages/{0}/images/str.jpg", fileName.Replace("-10mu", string.Empty)) };
            }
            if (fileName.Contains("paco"))
            {
                return new List<string> { string.Format("http://www.pacopacomama.com/moviepages/{0}/images/l_hd.jpg", fileName.Replace("-paco", string.Empty)) };
            }
            if (fileName.Contains("gachi"))
            {
                return new List<string> { string.Format("http://www.gachinco.com/guests/{0}/{0}_mainstr.jpg", fileName) };
            }
            if (fileName.Contains("mura"))
            {
                return new List<string> { string.Format("http://www.muramura.tv/moviepages/{0}/images/str.jpg", fileName.Replace("-mura", string.Empty)) };
            }
            if (fileName.Contains("mesubuta"))
            {
                return new List<string> { string.Format("http://www.mesubuta.net/gallery/{0}/images/swf_f.jpg", fileName.Replace("-mesubuta", string.Empty)) };
            }
            if (fileName.Contains("TokyoHot"))
            {
                return new List<string>(await GetTokyoHotCover(fileName.Replace("TokyoHot_", string.Empty)));
            }

            return new List<string> { await QueryDmmImage(fileName) };
        }

        private static async Task<List<string>> GetTokyoHotCover(string num)
        {
            var url = "http://www.tokyo-hot.com/j/new_j.html";

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                using (var message = await client.GetAsync(url))
                {
                    var ret = new List<string>();
                    var response = await message.Content.ReadAsStringAsync();
                    var numStart = response.IndexOf(num, 0, StringComparison.Ordinal);
                    if (numStart < 0) return ret;
                    var start = response.IndexOf("/new/new", numStart, StringComparison.Ordinal);
                    var end = response.IndexOf(".jpg", start, StringComparison.Ordinal) + 4;
                    var image = end - start <= 0 ? string.Empty : response.Substring(start, end - start);
                    var cover1 = "http://www.tokyo-hot.com" + image;
                    var cover2 = cover1.Replace(".jpg", "b.jpg");
                    ret.Add(cover1);
                    ret.Add(cover2);
                    return ret;
                }
            }
        }

        private static async Task<string> QueryDmmImage(string id)
        {
            if (id.Last() == 'A' || id.Last() == 'a') id = id.Substring(0, id.Length - 1);
            if (id.Last() == 'B' || id.Last() == 'b') return string.Empty;

            var url = string.Format("http://www.javlibrary.com/tw/vl_searchbyid.php?keyword=\"{0}\"", id);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                using (var message = await client.GetAsync(url))
                {
                    var response = await message.Content.ReadAsStringAsync();
                    var start = response.IndexOf("http://pics.dmm.co.jp/", 0, StringComparison.Ordinal);
                    if (start < 0) return string.Empty;
                    var end = response.IndexOf(".jpg", start, StringComparison.Ordinal) + 4;
                    return end - start <= 0 ? string.Empty : response.Substring(start, end - start);
                }
            }
        }

        private static async Task<string> Get1ponCover(string id)
        {
            var idParts = id.Split('_');
            if (idParts.Length > 1)
            {
                id = idParts[1];
            }

            var url = string.Format("http://www.1pondo.tv/dyn/ren/movie_details/{0}.json", id.Replace("-1pon", string.Empty));

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetStringAsync(url);
                dynamic js = JsonConvert.DeserializeObject(response);
                return Convert.ToString(js.ThumbHigh);
            }
        }
    }
}
