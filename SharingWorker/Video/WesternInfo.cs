using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SharingWorker.Video
{
    static class WesternInfo
    {
        private static Dictionary<string, Func<string, Task<VideoInfo>>> westernTasks = new Dictionary<string, Func<string, Task<VideoInfo>>>()
        {
            { "lp_", GetLegalPorno },
            { "BZ_", GetBrazzers },
            { "BB_", GetBangBros },
            { "21Naturals_", Get21Members },
            { "21naturals_", Get21Members },
            { "21Sextury_", Get21Members },
            { "21sextury_", Get21Members },
            { "KINK_", GetKINK },
            { "NA_", GetNaughtyAmerica },
            { "TUSHY_", GetTUSHY },
            { "phd_", GetPassionHD },
            { "RK_", GetRealityKings },
            { "Babes_", GetBabes },
            { "ZTOD_", GetZTOD },
            { "HX_", GetHardX },
            { "MOFOS_", GetMOFOS },
            { "TS_", GetTeamSkeet },
            { "HOLED_", GetHOLED },
            { "TE_", GetTeenErotica },
            { "JJ_", GetJulesJordan },
            { "JJV_", GetJulesJordanVideo },
            { "SPYFAM_", GetSPYFAM },
            { "NubileFilms_", GetNubileFilms },
            { "EA_", GetEvilAngel },
            { "Analized_", GetAnalized },
            { "VIXEN_", GetVIXEN },
            { "BLACKED_", GetBLACKED },
            { "LUBED_", GetLUBED },
            { "SPIZOO_", GetSPIZOO },
            { "HushPass_", GetHushPass },
        };

        public static bool Match(string id)
        {
            return westernTasks.Keys.Any(id.StartsWith);
        }

        public static async Task<VideoInfo> GetInfo(string id)
        {
            var task = westernTasks.First(p => id.StartsWith(p.Key)).Value;
            var info = await task(id);
            info.HideId = true;
            return info;
        }

        public static async Task<VideoInfo> GetLegalPorno(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("lp_", string.Empty);

            url = string.Format("https://www.legalporno.com/search/?query={0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<title>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf(" -\n LegalPorno", start, StringComparison.Ordinal);
                    if (end < 0) return ret;
                    
                    var title = responseString.Substring(start, end - start).TrimStart('\n').TrimEnd();
                    title = HttpUtility.HtmlDecode(title);
                    ret.Actresses = string.Empty;
                    ret.Title = string.Format("[LegalPorno]{0}", title);
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetBrazzers(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("BZ_", string.Empty);
            
            url = string.Format("http://www.brazzers.com/scenes/view/id/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h1 class=\"scene-title\" itemprop=\"name\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("<span class", start, StringComparison.Ordinal);                  
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "itemprop=\"name\" title=\"";
                        foreach(var searchStart in responseString.AllIndexesOf(search))
                        {
                            if (searchStart < 0) continue;
                            var aStart = searchStart + search.Length;
                            var aEnd = responseString.IndexOf("\"", aStart, StringComparison.Ordinal);
                            ret.Actresses += string.Format("{0}, ", responseString.Substring(aStart, aEnd - aStart));
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");
                        
                        ret.Title = string.Format("[BRAZZERS] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetBangBros(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("BB_", string.Empty);

            url = string.Format("https://bangbrothers.com/video{0}/a", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);
                
                var search = "<div class=\"ps-vdoHdd\"><h1>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "Cast:";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</div>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        foreach (var nameStart in namesStr.AllIndexesOf("\">"))
                        {
                            var aStart = nameStart + 2;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[BangBros] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> Get21Members(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var brand = id.Split('_')[0];
            var num = id.Split('_')[1];
            
            url = string.Format("http://21eroticanal.21naturals.com/en/join/{0}/", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h2 class=\"title\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf(": ", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                        start = end + 2;
                        end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);

                        ret.Actresses = responseString.Substring(start, end - start);
                        ret.Title = string.Format("[{0}] {1} - {2}", brand, ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetKINK(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("KINK_", string.Empty);

            url = string.Format("https://www.kink.com/shoot/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h1 class=\"shoot-title\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start).TrimEnd();
                        search = "<span class=\"names\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start = start + search.Length + 3;
                        end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);
                        var namesStr = responseString.Substring(start, end - start);
                        foreach (var nameStart in namesStr.AllIndexesOf("\">"))
                        {
                            var aStart = nameStart + 2;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");
                        
                        ret.Title = string.Format("[KINK] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetNaughtyAmerica(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("NA_", string.Empty);

            url = string.Format("http://tour.naughtyamerica.com/scene/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<title>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    search = " in ";
                    var end = responseString.IndexOf(search, start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        start = end + search.Length;
                        end = responseString.IndexOf(" -", start, StringComparison.Ordinal);
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        start = responseString.IndexOf("<h1>", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");


                        ret.Title = string.Format("[NaughtyAmerica] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetTUSHY(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("TUSHY_", string.Empty);

            url = string.Format("https://www.tushy.com/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);
                
                var search = "<h1 class=\"caption-title\"><a href=\"#\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</a>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        start = responseString.IndexOf("Featuring", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("&amp;", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "class=\"ajaxable\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");
                        
                        ret.Title = string.Format("[TUSHY] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetPassionHD(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("phd_", string.Empty);

            url = string.Format("https://passion-hd.com/video/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<meta content=\"Watch ";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    search = " in ";
                    var end = responseString.IndexOf(search, start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Actresses = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start).Replace(" &", ",");
                        start = end + search.Length;
                        end = responseString.IndexOf(" updated on", start, StringComparison.Ordinal);
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        ret.Title = string.Format("[passion-hd] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetRealityKings(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("RK_", string.Empty);

            url = string.Format("http://www.realitykings.com/tour/video/watch/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<title>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf(" - The Official", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                        start = responseString.IndexOf("<h4>", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("</h4>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[RealityKings] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetBabes(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("Babes_", string.Empty);

            url = string.Format("https://www.babes.com/tour/videos/view/id/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h1>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h1>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        search = "<h2 class=\"video-bar__models\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", HttpUtility.HtmlDecode(actress));
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[Babes] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetZTOD(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("ZTOD_", string.Empty);

            url = string.Format("http://www.ztod.com/videos/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<title>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);

                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf(" Featuring", start, StringComparison.Ordinal);

                    if (end < 0)
                    {
                        response = await client.GetByteArrayAsync(url);
                        responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);
                        start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                        if (start >= 0)
                        {
                            start = start + search.Length;
                            end = responseString.IndexOf(" Featuring", start, StringComparison.Ordinal);
                        }
                    }

                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        search = "Pornstars:<span class=\"txt-highlight\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", HttpUtility.HtmlDecode(actress));
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[ZTOD] {0}", ret.Title);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetHardX(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("HX_", string.Empty);

            url = string.Format("http://www.hardx.com/en/search/{0}?query={0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<div class=\"tlcTitle\">";
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += search.Length;
                    search = "\">";
                    start = responseString.IndexOf(search, start, StringComparison.OrdinalIgnoreCase);
                    start += search.Length;

                    var end = responseString.IndexOf("</a>", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "<div class=\"tlcActors\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        search = "</span>";
                        start = responseString.IndexOf(search, start, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</div>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", HttpUtility.HtmlDecode(actress));
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[HardX] {0} - {1}", title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetMOFOS(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("MOFOS_", string.Empty);

            url = string.Format("https://www.mofos.com/tour/search/?q={0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = string.Format("/tour/scene/{0}", num);
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    var end = responseString.IndexOf("\"", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var videoUrl = responseString.Substring(start, end - start);
                        videoUrl = string.Format("https://www.mofos.com{0}", videoUrl);

                        responseString = await client.GetStringAsync(videoUrl);
                        search = "<h1 class=\"title\">";
                        start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                        if (start >= 0)
                        {
                            start = start + search.Length;
                            end = responseString.IndexOf("</h1>", start, StringComparison.OrdinalIgnoreCase);
                            var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                            search = "<h2>";
                            start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                            start += search.Length;
                            end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);

                            var namesStr = responseString.Substring(start, end - start);
                            search = "<a title=\"Watch ";
                            foreach (var nameStart in namesStr.AllIndexesOf(search))
                            {
                                var aStart = nameStart + search.Length;
                                var aEnd = namesStr.IndexOf(" Profile", nameStart, StringComparison.Ordinal);
                                var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                                ret.Actresses += string.Format("{0}, ", HttpUtility.HtmlDecode(actress));
                            }
                            if (ret.Actresses != null)
                                ret.Actresses = ret.Actresses.RemoveEnd(", ");

                            ret.Title = string.Format("[MOFOS] {0} - {1}", title, ret.Actresses);
                        }
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetTeamSkeet(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("TS_", string.Empty);

            url = string.Format("http://www.teamskeet.com/t1/search/results/");
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("searchType", ""),
                    new KeyValuePair<string, string>("page", "search"),
                    new KeyValuePair<string, string>("query", num),
                    new KeyValuePair<string, string>("x", "12"),
                    new KeyValuePair<string, string>("y", "12"),
                });
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                
                var search = string.Format("http://www.teamskeet.com/t1/trailer/view/{0}", num);
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    var end = responseString.IndexOf("/", start + search.Length, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var videoUrl = responseString.Substring(start, end - start);
                        
                        responseString = await client.GetStringAsync(videoUrl);
                        search = "<title>";
                        start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                        if (start >= 0)
                        {
                            start = start + search.Length;
                            end = responseString.IndexOf("</title>", start, StringComparison.OrdinalIgnoreCase);

                            var titleStrings = responseString.Substring(start, end - start).Split('|');
                            if (titleStrings.Length < 2) return ret;

                            var title = HttpUtility.HtmlDecode(titleStrings[1].Trim());
                            ret.Actresses = HttpUtility.HtmlDecode(titleStrings[0].Trim());

                            ret.Title = string.Format("[TeamSkeet] {0} - {1}", title, ret.Actresses);
                        }
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetHOLED(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("HOLED_", string.Empty);

            url = string.Format("https://holed.com/video/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);
                
                var search = "<h3 class=\"text-primary\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h3>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        start = responseString.IndexOf("<p>", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("</p>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[HOLED] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetTeenErotica(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("TE_", string.Empty);
            var queryStr = new StringBuilder();
            for (var i = 0; i < num.Length; i++)
            {
                if (i > 0 && char.IsUpper(num[i])) queryStr.Append(" ");
                queryStr.Append(num[i]);
            }

            url = string.Format("http://teenerotica.xxx/videos/?q={0}", queryStr.ToString().Replace(" ", "+"));
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);
                
                var search = string.Format("<h2>{0}", queryStr);
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += 4;
                    var end = responseString.IndexOf("</h2>", start, StringComparison.OrdinalIgnoreCase);
                    var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                    search = "<div class=\"updateModels\">";
                    start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                    start += search.Length;
                    end = responseString.IndexOf("</div>", start, StringComparison.Ordinal);

                    var namesStr = responseString.Substring(start, end - start);
                    search = "\">";
                    foreach (var nameStart in namesStr.AllIndexesOf(search))
                    {
                        var aStart = nameStart + search.Length;
                        var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                        var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                        ret.Actresses += string.Format("{0}, ", actress);
                    }
                    if (ret.Actresses != null)
                        ret.Actresses = ret.Actresses.RemoveEnd(", ");

                    ret.Title = string.Format("[TeenErotica] {0} - {1}", title, ret.Actresses);
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetJulesJordan(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("JJ_", string.Empty);
            var queryStr = new StringBuilder();
            for (var i = 0; i < num.Length; i++)
            {
                if (i > 0 && char.IsUpper(num[i])) queryStr.Append(" ");
                queryStr.Append(num[i]);
            }

            url = string.Format("https://www.julesjordan.com/trial/search.php?query={0}", HttpUtility.UrlEncode(queryStr.ToString()));
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);
                var search = "<!-- Title -->";
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += search.Length;
                    search = "\">";
                    start = responseString.IndexOf(search, start, StringComparison.OrdinalIgnoreCase);
                    start += search.Length;
                    var end = responseString.IndexOf("</a>", start, StringComparison.OrdinalIgnoreCase);
                    var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                    search = "<span class=\"update_models\">";
                    start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                    start += search.Length;
                    end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);

                    var namesStr = responseString.Substring(start, end - start);
                    search = "\">";
                    foreach (var nameStart in namesStr.AllIndexesOf(search))
                    {
                        var aStart = nameStart + search.Length;
                        var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                        var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                        ret.Actresses += string.Format("{0}, ", actress);
                    }
                    if (ret.Actresses != null)
                        ret.Actresses = ret.Actresses.RemoveEnd(", ");

                    ret.Title = string.Format("[JulesJordan] {0}", title);
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetJulesJordanVideo(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var nums = id.Replace("JJV_", string.Empty).Split('_');
            if (nums.Count() < 2) return ret;

            var queryStr = nums[0];
            var actStr = new StringBuilder();
            for (var i = 0; i < nums[1].Length; i++)
            {
                if (i > 0 && char.IsUpper(nums[1][i])) actStr.Append(" ");
                actStr.Append(nums[1][i]);
            }
            ret.Actresses = actStr.ToString();

            url = string.Format("http://www.julesjordanvideo.com/movie/{0}/index.html", queryStr);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<strong>";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</strong>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                        ret.Title = string.Format("[JulesJordan] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetSPYFAM(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("SPYFAM_", string.Empty);

            url = string.Format("https://spyfam.com/video/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h3 class=\"text-primary\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h3>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                        start = responseString.IndexOf("<p>", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("</p>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[SPYFAM] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetNubileFilms(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("NubileFilms_", string.Empty);

            url = string.Format("http://nubilefilms.com/video/watch/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<span class=\"wp-title videotitle\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                        search = "<span class=\"featuring-modelname model\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0} ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.TrimEnd();

                        ret.Title = string.Format("[NubileFilms] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetEvilAngel(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("EA_", string.Empty);

            url = string.Format("http://www.evilangel.com/en/search/{0}?query={0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<div class=\"tlcTitle\">";
                var start = responseString.IndexOf(search, 0, StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    start += search.Length;
                    search = "\">";
                    start = responseString.IndexOf(search, start, StringComparison.OrdinalIgnoreCase);
                    start += search.Length;

                    var end = responseString.IndexOf("</a>", start, StringComparison.OrdinalIgnoreCase);
                    if (end >= 0)
                    {
                        var title = HttpUtility.HtmlDecode(responseString.Substring(start, end - start));

                        search = "<div class=\"tlcActors\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        search = "</span>";
                        start = responseString.IndexOf(search, start, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</div>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", HttpUtility.HtmlDecode(actress));
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[EvilAngel] {0} - {1}", title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetAnalized(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("Analized_", string.Empty);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            url = string.Format("http://analized.com/updates/{0}.html", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<div class=\"title_bar trailer_title\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf("</div>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
                        search = "<span class=\"update_models\">";
                        start = responseString.IndexOf(search, end, StringComparison.Ordinal);
                        start += search.Length;
                        end = responseString.IndexOf("</span>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[Analized] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetVIXEN(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("VIXEN_", string.Empty);

            url = string.Format("https://www.vixen.com/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h1 class=\"caption-title\"><a href=\"#\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf("</a>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        start = responseString.IndexOf("Featuring", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("&amp;", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "class=\"ajaxable\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[VIXEN] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetBLACKED(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("BLACKED_", string.Empty);

            url = string.Format("https://www.blacked.com/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);
                
                var search = "<h1 class=\"caption-title\"><a href=\"#\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += search.Length;
                    var end = responseString.IndexOf("</a>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        start = responseString.IndexOf("Featuring", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("&amp;", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "class=\"ajaxable\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[BLACKED] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetLUBED(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("LUBED_", string.Empty);

            url = string.Format("https://lubed.com/video/{0}", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<h3 class=\"text-primary\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</h3>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        start = responseString.IndexOf("<p>", end, StringComparison.Ordinal);
                        end = responseString.IndexOf("</p>", start, StringComparison.Ordinal);

                        var namesStr = responseString.Substring(start, end - start);
                        search = "\">";
                        foreach (var nameStart in namesStr.AllIndexesOf(search))
                        {
                            var aStart = nameStart + search.Length;
                            var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                            var actress = namesStr.Substring(aStart, aEnd - aStart).TrimEnd();
                            ret.Actresses += string.Format("{0}, ", actress);
                        }
                        if (ret.Actresses != null)
                            ret.Actresses = ret.Actresses.RemoveEnd(", ");

                        ret.Title = string.Format("[LUBED] {0} - {1}", ret.Title, ret.Actresses);
                    }
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetSPIZOO(string id)
        {
            var url = string.Empty;
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("SPIZOO_", string.Empty);

            url = string.Format("http://www.spizoo.com/updates/{0}.html", num);
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var responseString = await client.GetStringAsync(url);

                var search = "<div class=\"col-sm-7\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start < 0) return ret;
                search = "<h2>";
                start = responseString.IndexOf(search, start, StringComparison.Ordinal);
                if (start < 0) return ret;
                start += search.Length;
                var end = responseString.IndexOf("</h2>", start, StringComparison.Ordinal);
                if (end >= 0)
                {
                    ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                    start = responseString.IndexOf("<p class=\"lead\">Featuring:", end, StringComparison.Ordinal);
                    end = responseString.IndexOf("</p>", start, StringComparison.Ordinal);

                    var namesStr = responseString.Substring(start, end - start);
                    search = "'>";
                    foreach (var nameStart in namesStr.AllIndexesOf(search))
                    {
                        var aStart = nameStart + search.Length;
                        var aEnd = namesStr.IndexOf("</a>", nameStart, StringComparison.Ordinal);
                        var actress = namesStr.Substring(aStart, aEnd - aStart).Replace(". ", string.Empty);
                        ret.Actresses += string.Format("{0}, ", actress);
                    }
                    if (ret.Actresses != null)
                        ret.Actresses = ret.Actresses.RemoveEnd(", ");

                    ret.Title = string.Format("[SPIZOO] {0} - {1}", ret.Title, ret.Actresses);
                }
            }
            return ret;
        }

        public static async Task<VideoInfo> GetHushPass(string id)
        {
            var ret = new VideoInfo { Title = "", Actresses = "" };
            var num = id.Replace("HushPass_", string.Empty);

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36");
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                client.DefaultRequestHeaders.Referrer = new Uri(string.Format("https://hushpass.com/video/{0}.html", num));

                var content = new StringContent(string.Format("{{\"method\":\"JSONP\",\"headers\":{{\"Content-Type\":\"application/x-www-form-urlencoded;charset=utf-8;\"}},\"slug\":\"{0}.html\"}}", num), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://hushpass.com/administrator/admin/hushpass/api/getVideobyUrl", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic result = JObject.Parse(responseString);
                ret.Title = result.video.title;
                ret.Actresses = result.video.scenes_artist_name;
                ret.Title = string.Format("[HushPass] {0} - {1}", ret.Title, ret.Actresses);
            }
            return ret;
        }
    }
}
