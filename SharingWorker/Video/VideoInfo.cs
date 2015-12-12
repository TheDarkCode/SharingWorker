﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SharingWorker.Video
{
    public enum QueryLang { TW, EN }

    public class VideoInfo
    {
        public string Title;
        public string Actresses;

        public static string GetNormalizedName(string fileName)
        {
            if (fileName.Contains("caribpr"))
            {
                const string find = "-caribpr";
                return fileName.Substring(0, fileName.IndexOf(find) + find.Length);
            }
            if (fileName.Contains("carib"))
            {
                const string find = "-carib";
                return fileName.Substring(0, fileName.IndexOf(find) + find.Length);
            }
            if (fileName.Contains("1pon"))
            {
                const string find = "-1pon";
                return fileName.Substring(0, fileName.IndexOf(find) + find.Length);
            }
            if (fileName.Contains("heyzo"))
            {
                return fileName.Replace("_full", string.Empty);
            }
            if (fileName.Contains("10mu"))
            {
                const string find = "-10mu";
                return fileName.Substring(0, fileName.IndexOf(find) + find.Length);
            }
            if (fileName.Contains("paco"))
            {
                const string find = "-paco";
                return fileName.Substring(0, fileName.IndexOf(find) + find.Length);
            }
            if (fileName.Contains("mura"))
            {
                const string find = "-mura";
                return fileName.Substring(0, fileName.IndexOf(find) + find.Length);
            }
            if (fileName.Contains("mesubuta"))
            {
                const string find = "-mesubuta";
                return fileName.Substring(0, fileName.IndexOf(find) + find.Length);
            }
            return fileName;
        }

        public static string AddPartName(string fileName, string normalizedName)
        {
            var part = "";
            for (int i = 1; i < 4; i++)
            {
                var whole = "whole" + i;
                var hd = "hd" + i;
                var high = "high_" + i;
                if (fileName.IndexOf(whole) > 0)
                {
                    part = i.ToString();
                    break;
                }
                if (fileName.IndexOf(hd) > 0)
                {
                    part = i.ToString();
                    break;
                }
                if (fileName.IndexOf(high) > 0)
                {
                    part = i.ToString();
                    break;
                }
            }
            return normalizedName + part;
        }

        public static async Task<VideoInfo> QueryVideoInfo(string id, QueryLang lang)
        {
            try
            {
                if (id.Contains("1pon"))
                {
                    return await GetVideoInfo_1pon(id, lang);
                }
                if (id.Contains("caribpr"))
                {
                    return await GetVideoInfo_caribpr(id, lang);
                }
                if (id.Contains("carib"))
                {
                    return await GetVideoInfo_carib(id, lang);
                }
                if (id.Contains("heyzo"))
                {
                    return await GetVideoInfo_heyzo(id, lang);
                }
                if (id.Contains("TokyoHot"))
                {
                    return await GetVideoInfo_tokyohot(id, lang);
                }
                if (id.Contains("10mu"))
                {
                    return await GetVideoInfo_10mu(id, lang);
                }
                if (id.Contains("paco"))
                {
                    return await GetVideoInfo_paco(id, lang);
                }
                if (id.Contains("gachi"))
                {
                    return await GetVideoInfo_gachi(id, lang);
                }
                if (id.Contains("mura"))
                {
                    return await GetVideoInfo_mura(id, lang);
                }
                if (id.Contains("mesubuta"))
                {
                    return await GetVideoInfo_mesubuta(id, lang);
                }
                if (char.IsDigit(id, 0) || id.Contains("XXX-AV"))
                {
                    return new VideoInfo {Title = "", Actresses = ""};
                }
            }
            catch (Exception)
            {
                return new VideoInfo { Title = "", Actresses = "" };
            }

            return await GetVideoInfo_Dmm(id, lang);
        }

        private static async Task<VideoInfo> GetVideoInfo_Dmm(string id, QueryLang lang)
        {
            var url = string.Empty;
            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.javlibrary.com/tw/vl_searchbyid.php?keyword=\"{0}\"", id);
                    break;
                case QueryLang.EN:
                    url = string.Format("http://www.javlibrary.com/en/vl_searchbyid.php?keyword=\"{0}\"", id);
                    break;
            }

            var ret = new VideoInfo { Title = "", Actresses = "" };

            try
            {
                using (var handler = new HttpClientHandler())
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = new TimeSpan(0,5,0);
                    using (var message = await client.GetAsync(url))
                    {
                        var response = await message.Content.ReadAsStringAsync();
                        var start = response.IndexOf("<title>", 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start = response.IndexOf(" ", start, StringComparison.Ordinal) + 1;
                            var end = response.IndexOf(" - JAVLibrary", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Title = end - start <= 0 ? string.Empty : response.Substring(start, end - start);
                            }
                        }

                        foreach (var actStart in response.AllIndexesOf("<a href=\"vl_star.php?s="))
                        {
                            start = response.IndexOf(">", actStart, StringComparison.Ordinal) + 1;
                            var end = response.IndexOf("</a>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Actresses += end - start <= 0
                                    ? string.Empty
                                    : string.Format("{0} ", response.Substring(start, end - start));
                            }
                        }
                        ret.Actresses = ret.Actresses.TrimEnd(' ');
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.ErrorException("Failed to get info from JAVLibrary", ex);
            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_tokyohot(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("TokyoHot_", string.Empty);

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.tokyo-hot.com/j/new_j.html");
                    using (var handler = new HttpClientHandler())
                    using (var client = new HttpClient(handler))
                    {
                        var response = await client.GetByteArrayAsync(url);
                        var responseString = Encoding.GetEncoding("shift_jis").GetString(response, 0, response.Length - 1);

                        var start = responseString.IndexOf(num, 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start = responseString.IndexOf("東京熱の", start, StringComparison.Ordinal) + 4;
                            var end = responseString.IndexOf("『", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Actresses = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start).Replace("&", string.Empty);

                                start = end + 1;
                                end = responseString.IndexOf("』", start, StringComparison.Ordinal);
                                if (end >= 0)
                                {
                                    ret.Title = end - start <= 0 ? string.Empty : string.Format("Tokyo Hot {0} {1} {2}", num, responseString.Substring(start, end - start), ret.Actresses);
                                }
                            }
                        }
                    }
                    break;
                case QueryLang.EN:
                    url = string.Format("http://www.tokyo-hot.com/e/new_video000_e.html");
                    using (var handler = new HttpClientHandler())
                    using (var client = new HttpClient(handler))
                    {
                        var response = await client.GetByteArrayAsync(url);
                        var responseString = Encoding.GetEncoding("iso-8859-1").GetString(response, 0, response.Length - 1);

                        var start = responseString.IndexOf(string.Format("./{0}", num), 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start = start + 1;
                            var end = responseString.IndexOf("e.html", start, StringComparison.Ordinal) + 6;
                            if (end < 0) return ret;
                            var url1 = end - start <= 0 ? string.Empty : "http://www.tokyo-hot.com/e" + responseString.Substring(start, end - start);
                            if (string.IsNullOrEmpty(url1)) return ret;

                            var numPage = await client.GetByteArrayAsync(url1);
                            var numPageString = Encoding.GetEncoding("iso-8859-1").GetString(numPage, 0, numPage.Length - 1);

                            start = numPageString.IndexOf("<title>", 0, StringComparison.Ordinal);
                            end = numPageString.IndexOf("</title>", start, StringComparison.Ordinal);
                            if (end - start > 0)
                            {
                                start += 7;
                                ret.Actresses = numPageString.Substring(start, end - start);
                                ret.Title = string.Format("Tokyo Hot {0} - {1}", num, ret.Actresses);
                            }
                        }
                    }
                    break;
            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_heyzo(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("heyzo_hd_", string.Empty).Replace("_full", string.Empty);

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.heyzo.com/moviepages/{0}/index.html", num);
                    break;
                case QueryLang.EN:
                    url = string.Format("http://en.heyzo.com/moviepages/{0}/index.html", num);
                    break;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                using (var message = await client.GetAsync(url))
                {
                    var response = await message.Content.ReadAsStringAsync();
                    var start = response.IndexOf("<title>", 0, StringComparison.Ordinal);

                    switch (lang)
                    {
                        case QueryLang.TW:
                            if (start >= 0)
                            {
                                start = start + 7;
                                var end = response.IndexOf(" ", start, StringComparison.Ordinal);
                                if (end >= 0)
                                {
                                    ret.Actresses = end - start <= 0 ? string.Empty : response.Substring(start, end - start);

                                    start = end + 1;
                                    var end1 = response.IndexOf(" - 無修正動画 HEYZO</title>", start, StringComparison.Ordinal);
                                    if (end1 >= 0)
                                    {
                                        ret.Title = end1 - start <= 0 ? string.Empty : string.Format("HEYZO {0} {1} {2}", num, response.Substring(start, end1 - start), ret.Actresses);
                                    }
                                }
                            }
                            break;
                        case QueryLang.EN:
                            if (start >= 0)
                            {
                                start = start + 7;
                                var end = response.IndexOf(" - HEYZO</title>", start, StringComparison.Ordinal);
                                if (end >= 0)
                                {
                                    ret.Title = end - start <= 0 ? string.Empty : string.Format("HEYZO {0} {1}", num, response.Substring(start, end - start));                                   
                                }
                            }
                            break;
                    }
                    
                }
            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_1pon(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };

            var idParts = id.Split('_');
            if (idParts.Length > 1)
            {
                id = idParts[1];
            }

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.1pondo.tv/dyn/ren/movie_details/{0}.json", id.Replace("-1pon", string.Empty));
                    break;
                case QueryLang.EN:
                    url = string.Format("http://en.1pondo.tv/eng/moviepages/{0}/index.htm", id.Replace("-1pon", string.Empty));
                    break;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetStringAsync(url);

                switch (lang)
                {
                    case QueryLang.TW:
                        dynamic js = JsonConvert.DeserializeObject(response);
                        var title = Convert.ToString(js.Title);
                        var actresses = Convert.ToString(js.Actor);
                        if (!string.IsNullOrEmpty(actresses))
                            actresses = actresses.Replace(",", " ");

                        ret.Actresses = actresses;
                        ret.Title = string.Format("{0} {1}", title, actresses);
                        break;
                    case QueryLang.EN:
                        //if (start >= 0)
                        //{
                        //    start = responseString.IndexOf(":: ", start, StringComparison.Ordinal) + 3;
                        //    var end = responseString.IndexOf("</title>", start, StringComparison.Ordinal);
                        //    if (end >= 0)
                        //    {
                        //        ret.Actresses = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                        //        ret.Title = string.Format("{0} - {1}", id, ret.Actresses);
                        //    }
                        //}
                        break;
                }
            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_carib(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.caribbeancom.com/moviepages/{0}/index.html", id.Replace("-carib", string.Empty));
                    break;
                case QueryLang.EN:
                    url = string.Format("http://en.caribbeancom.com/eng/moviepages/{0}/index.html", id.Replace("-carib", string.Empty));
                    break;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                using (var message = await client.GetAsync(url))
                {
                    var response = await message.Content.ReadAsStringAsync();
                    
                    var start = response.IndexOf("<title>", 0, StringComparison.Ordinal);
                    switch(lang)
                    {
                        case QueryLang.TW:

                            var search = "<h1 itemprop=\"name\">";
                            var titleStart = response.IndexOf(search, start, StringComparison.Ordinal);
                            if (titleStart >= 0)
                            {
                                titleStart = titleStart + search.Length;
                                var end = response.IndexOf("</h1>", titleStart, StringComparison.Ordinal);
                                if (end >= 0)
                                {
                                    ret.Title = end - titleStart <= 0 ? string.Empty : response.Substring(titleStart, end - titleStart);

                                    start = start + 7;
                                    end = response.IndexOf(" カリビアンコム </title>", start, StringComparison.Ordinal);
                                    if (end >= 0)
                                    {
                                        var all = end - start <= 0 ? string.Empty : response.Substring(start, end - start);
                                        ret.Actresses = all.Replace(ret.Title, string.Empty).TrimEnd(' ');
                                        if(!string.IsNullOrEmpty(ret.Actresses))
                                            ret.Title += string.Format(" {0}", ret.Actresses);
                                    }
                                }
                            }
                            break;
                        case QueryLang.EN:
                            if (start >= 0)
                            {
                                start = response.IndexOf(":: ", start, StringComparison.Ordinal) + 3;
                                var end = response.IndexOf("</h2>", start, StringComparison.Ordinal);
                                if (end >= 0)
                                {
                                    ret.Actresses = end - start <= 0 ? string.Empty : response.Substring(start, end - start);
                                    ret.Title = string.Format("{0} - {1}", id, ret.Actresses);
                                }
                            }
                            break;
                    }
                }
            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_caribpr(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };

            switch (lang)
            {
                case QueryLang.TW:
                case QueryLang.EN:
                    url = string.Format("http://www.caribbeancompr.com/moviepages/{0}/index.html", id.Replace("-caribpr", string.Empty));
                    break;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.GetEncoding("euc-jp").GetString(response, 0, response.Length - 1);

                var start = responseString.IndexOf("<title>", 0, StringComparison.Ordinal);

                switch (lang)
                {
                    case QueryLang.TW:
                        if (start >= 0)
                        {
                            var find = "カリビアンコム プレミアム ";
                            start = responseString.IndexOf(find, start, StringComparison.Ordinal) + find.Length;
                            var end = responseString.IndexOf("(", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Actresses = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);

                                start = end + 1;
                                end = responseString.IndexOf(")</title>", start, StringComparison.Ordinal);
                                if (end >= 0)
                                {
                                    ret.Title = end - start <= 0 ? string.Empty : string.Format("{0} {1}",responseString.Substring(start, end - start), ret.Actresses);
                                }
                            }
                        }
                        break;
                    case QueryLang.EN:
                        ret.Title = id;
                        break;
                }

            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_10mu(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.10musume.com/moviepages/{0}/index.html", id.Replace("-10mu", string.Empty));
                    break;
                case QueryLang.EN:
                    url = string.Format("http://en.10musume.com/eng/moviepages/{0}/index.html", id.Replace("-10mu", string.Empty));
                    break;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                int start, end;

                switch (lang)
                {
                    case QueryLang.TW:
                        var response = await client.GetByteArrayAsync(url);
                        var responseString = Encoding.GetEncoding("euc-jp").GetString(response, 0, response.Length - 1);

                        start = responseString.IndexOf("<title>", 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start = responseString.IndexOf("天然むすめ", start, StringComparison.Ordinal);
                            end = responseString.IndexOf("</title>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start).Replace("  ", " ");

                                start = responseString.IndexOf("タイトル：</em>", end, StringComparison.Ordinal);
                                if (start < 0) return ret;
                                start += 10;
                                end = responseString.IndexOf("</li>", start, StringComparison.Ordinal);
                                if (end >= 0)
                                {
                                    var videoName = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                                    ret.Actresses = ret.Title.Replace("天然むすめ ", string.Empty).Replace(videoName, string.Empty).TrimStart(' ');
                                }
                            }
                        }
                        break;
                    case QueryLang.EN:
                        using (var message = await client.GetAsync(url))
                        {
                            var responseEn = await message.Content.ReadAsStringAsync();
                            start = responseEn.IndexOf("<title>", 0, StringComparison.Ordinal);
                            if (start >= 0)
                            {
                                start += 7;
                                end = responseEn.IndexOf("</title>", start, StringComparison.Ordinal);
                                if (end >= 0)
                                {
                                    ret.Title = end - start <= 0 ? string.Empty : responseEn.Substring(start, end - start).Replace(" Japanese Amateur Girls", "");
                                }
                            }
                        }
                        break;
                }
            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_paco(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.pacopacomama.com/moviepages/{0}/index.html", id.Replace("-paco", string.Empty));
                    using (var handler = new HttpClientHandler())
                    using (var client = new HttpClient(handler))
                    {
                        var response = await client.GetByteArrayAsync(url);
                        var responseString = Encoding.GetEncoding("euc-jp").GetString(response, 0, response.Length - 1);

                        var start = responseString.IndexOf("<title>", 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start += 7;
                            var end = responseString.IndexOf("</title>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                end--;
                                ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);

                                start = responseString.IndexOf("名前:", end, StringComparison.Ordinal);
                                if (start < 0) return ret;
                                start = responseString.IndexOf("html\">", start, StringComparison.Ordinal) + 6;
                                end = responseString.IndexOf("</a>", start, StringComparison.Ordinal);
                                if (end - start > 0 && end - start < 31)
                                {
                                    ret.Actresses = responseString.Substring(start, end - start);
                                    ret.Title += string.Format(" {0}", ret.Actresses);
                                }
                            }
                        }
                    }
                    break;
                case QueryLang.EN:
                    url = string.Format("http://en.pacopacomama.com/eng/moviepages/{0}/index.html", id.Replace("-paco", string.Empty));
                    using (var handler = new HttpClientHandler())
                    using (var client = new HttpClient(handler))
                    {
                        var response = await client.GetByteArrayAsync(url);
                        var responseString = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);

                        var start = responseString.IndexOf("<title>", 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start += 7;
                            var end = responseString.IndexOf("</title>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                end--;
                                ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start).Replace("::", "-");
                            }
                        }
                    }
                    break;
            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_gachi(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.gachinco.com/moviepages/{0}/index.html", id);
                    break;
                case QueryLang.EN:
                    url = string.Format("http://en.gachinco.com/moviepages/{0}/index.html", id);
                    break;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.GetEncoding("euc-jp").GetString(response, 0, response.Length - 1);

                const string search = "<h2 class=\"main_title_bar\" style=\"width:620px;\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = 0;
                    switch (lang)
                    {
                        case QueryLang.TW:
                            end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Title = end - start <= 0 ? string.Empty : "ガチん娘！ " + responseString.Substring(start, end - start).Replace("　", " ");

                                end = responseString.IndexOf("　", start, StringComparison.Ordinal);
                                if (end < 0) return ret;
                                ret.Actresses = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                            }
                            break;
                        case QueryLang.EN:
                            end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Title = end - start <= 0 ? string.Empty : string.Format("GACHINCO.COM - {0}", responseString.Substring(start, end - start));
                            }
                            break;
                    }
                }
            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_mura(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.muramura.tv/moviepages/{0}/index.html", id.Replace("-mura", string.Empty));
                    break;
                case QueryLang.EN:
                    url = string.Format("http://en.muramura.tv/eng/moviepages/{0}/index.html", id.Replace("-mura", string.Empty));
                    break;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.GetEncoding("euc-jp").GetString(response, 0, response.Length - 1);

                var start = responseString.IndexOf("<h1>", 0, StringComparison.Ordinal);

                switch (lang)
                {
                    case QueryLang.TW:
                        if (start >= 0)
                        {
                            start = responseString.IndexOf("</script>", start, StringComparison.Ordinal) + 9;
                            var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start).TrimStart(Environment.NewLine.ToCharArray());

                                start = responseString.IndexOf("名前：</em>", end, StringComparison.Ordinal);
                                if (start >= 0)
                                {
                                    start += 9;
                                    start = responseString.IndexOf(">", start, StringComparison.Ordinal);
                                    if (start >= 0)
                                    {
                                        start += 1;
                                        end = responseString.IndexOf("</a>", start, StringComparison.Ordinal);
                                        if (end >= 0)
                                        {
                                            ret.Actresses = end - start <= 0 && end - start > 25 ? string.Empty : responseString.Substring(start, end - start);
                                            ret.Title += string.Format(" {0}", ret.Actresses);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case QueryLang.EN:
                        if (start >= 0)
                        {
                            start += 4;
                            var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start).Replace("&nbsp;", " ");
                            }
                        }
                        break;
                }

            }
            return ret;
        }

        private static async Task<VideoInfo> GetVideoInfo_mesubuta(string id, QueryLang lang)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };

            switch (lang)
            {
                case QueryLang.TW:
                    url = string.Format("http://www.mesubuta.net/moviepages/{0}/index.html", id.Replace("-mesubuta", string.Empty));
                    break;
                case QueryLang.EN:
                    url = string.Format("http://en.mesubuta.net/moviepages/{0}/index.html", id.Replace("-mesubuta", string.Empty));
                    break;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.GetEncoding("euc-jp").GetString(response, 0, response.Length - 1);

                var start = responseString.IndexOf("<th>タイトル</th>", 0, StringComparison.Ordinal);

                switch (lang)
                {
                    case QueryLang.TW:
                        if (start >= 0)
                        {
                            start = responseString.IndexOf("<td>", start, StringComparison.Ordinal) + 4;
                            var end = responseString.IndexOf("</td>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Title = end - start <= 0 ? string.Empty : "メス豚 " + responseString.Substring(start, end - start);

                                start = responseString.IndexOf("<th>なまえ</th>", end, StringComparison.Ordinal);
                                start = responseString.IndexOf("<td>", start, StringComparison.Ordinal);
                                if (start >= 0)
                                {
                                    start += 4;
                                    end = responseString.IndexOf(" ", start, StringComparison.Ordinal);
                                    if (end >= 0)
                                    {
                                        ret.Actresses = end - start <= 0 && end - start > 25 ? string.Empty : responseString.Substring(start, end - start);
                                        ret.Title += string.Format(" {0}", ret.Actresses);
                                    }
                                }
                            }
                        }
                        break;
                    case QueryLang.EN:
                        start = responseString.IndexOf("<th>Name</th>", 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start = responseString.IndexOf("<td>", start, StringComparison.Ordinal) + 4;
                            var end = responseString.IndexOf("</td>", start, StringComparison.Ordinal);
                            if (end >= 0)
                            {
                                ret.Title = end - start <= 0 ? string.Empty : string.Format("{0} - {1}", id, responseString.Substring(start, end - start));
                            }
                        }
                        break;
                }

            }
            return ret;
        }
    }
}
