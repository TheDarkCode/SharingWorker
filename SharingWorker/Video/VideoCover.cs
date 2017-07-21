using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using NLog;

namespace SharingWorker.Video
{
    public static class VideoCover
    {
        public static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

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
            for (int i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    if (!fileName.EndsWith("hd")) continue;
                }
                else
                {
                    if (!fileName.EndsWith(string.Format("hd{0}", i))) continue;
                }

                var rmIdx = fileName.LastIndexOf("_");
                if (rmIdx > 0)
                {
                    fileName = fileName.Remove(rmIdx);
                    break;
                }
            }
            
            var coverUrls = await GetCoverImageUrl(fileName);
            if (!coverUrls.Any()) return false;

            var ret = true;

            for (int i = 0; i < coverUrls.Count; i++)
            {
                if (!fileName.Contains("mesubuta") && !fileName.Contains("mura") && !fileName.Contains("1000giri") && (fileName.Last() == 'A' || fileName.Last() == 'a'))
                    fileName = fileName.Substring(0, fileName.Length - 1);

                if (!fileName.Contains("carib") && (fileName.Last() == 'B' || fileName.Last() == 'b'))
                    continue;

                var outputPath = i == 0 ? fileName + "pl.jpg" : fileName + string.Format("pl{0}.jpg", i);
                if (File.Exists(outputPath)) continue;
                
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

        public static async Task<List<string>> GetCoverImageUrl(string fileName)
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
            if (fileName.Contains("h4610") || fileName.Contains("H4610"))
            {
                return new List<string> { string.Format("http://www.h4610.com/moviepages/{0}/images/movie.jpg", fileName.Substring(6)) };
            }
            if (fileName.Contains("h0930") || fileName.Contains("H0930"))
            {
                return new List<string> { string.Format("http://www.h0930.com/moviepages/{0}/images/movie.jpg", fileName.Substring(6)) };
            }
            if (fileName.Contains("c0930") || fileName.Contains("C0930"))
            {
                return new List<string> { string.Format("http://www.c0930.com/moviepages/{0}/images/movie.jpg", fileName.Substring(6)) };
            }
            if (fileName.Contains("av-sikou"))
            {
                return new List<string> { string.Format("http://www.av-sikou.com/contents/{0}/images/movie.jpg", fileName.Replace("av-sikou_", string.Empty)) };
            }
            if (fileName.Contains("heydouga"))
            {
                fileName = fileName.Replace("heydouga-", string.Empty).Replace("-", "/");
                return new List<string> { string.Format("http://image01.heydouga.com/contents/{0}/player_thumb.jpg", fileName) };
            }
            if (fileName.Contains("1000giri"))
            {
                return new List<string> { string.Format("http://www.1000giri.net/gallery/{0}/images/swf_f.jpg", fileName.Replace("1000giri-", string.Empty)) };
            }
            if (fileName.StartsWith("SIRO-"))
            {
                fileName = fileName.Replace("-", string.Empty).ToLower();
                return new List<string> { string.Format("http://sirouto-douga.1000.tv/img/capture/{0}/{0}_b0.jpg", fileName) };
            }
            if (fileName.StartsWith("200GANA-"))
            {
                fileName = fileName.Replace("200GANA-", "gana");
                return new List<string> { string.Format("http://sirouto-douga.1000.tv/img/capture/{0}/{0}_b0.jpg", fileName) };
            }
            if (fileName.StartsWith("259LUXU-"))
            {
                fileName = fileName.Replace("259LUXU-", "lux");
                return new List<string> { string.Format("http://sirouto-douga.1000.tv/img/capture/{0}/{0}_b0.jpg", fileName) };
            }
            if (fileName.StartsWith("261ARA-"))
            {
                fileName = fileName.Replace("261ARA-", "ara");
                return new List<string> { string.Format("http://sirouto-douga.1000.tv/img/capture/{0}/{0}_b0.jpg", fileName) };
            }

            return new List<string> { await QueryDmmImage(fileName) };
        }

        private static async Task<List<string>> GetTokyoHotCover(string num)
        {
            var url = string.Format("http://www.tokyo-hot.com/product/?q={0}", num);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                using (var message = await client.GetAsync(url))
                {
                    var ret = new List<string>();
                    var response = await message.Content.ReadAsStringAsync();
                    const string find = "class=\"rm\"><img src=\"";
                    var start = response.IndexOf(find, 0, StringComparison.Ordinal);
                    if (start < 0) return ret;
                    start = start + find.Length;

                    var end = response.IndexOf(".jpg", start, StringComparison.Ordinal) + 4;
                    var image = end - start <= 0 ? string.Empty : response.Substring(start, end - start);
                    image = image.Replace("220x124", "820x462");
                    ret.Add(image);
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
            var url = string.Format("http://www.1pondo.tv/dyn/ren/movie_details/movie_id/{0}.json", id.Replace("-1pon", string.Empty));

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                try
                {
                    var response = await client.GetStringAsync(url);
                    dynamic js = JsonConvert.DeserializeObject(response);
                    return Convert.ToString(js.ThumbHigh);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(ex.Message, ex);
                    return string.Empty;
                }
            }
        }
    }
}
