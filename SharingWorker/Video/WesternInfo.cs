using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharingWorker.Video
{
    static class WesternInfo
    {
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
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);

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
                        ret.Actresses = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        start = end + search.Length;
                        end = responseString.IndexOf(" -", start, StringComparison.Ordinal);
                        ret.Title = end - start <= 0 ? string.Empty : HttpUtility.HtmlDecode(responseString.Substring(start, end - start));
                        
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
                var response = await client.GetByteArrayAsync(url);
                var responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                var search = "<h1 class=\"caption-title\"><a href=\"#\">";
                var start = responseString.IndexOf(search, 0, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start = start + search.Length;
                    var end = responseString.IndexOf("</a>", start, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        ret.Title = end - start <= 0 ? string.Empty : responseString.Substring(start, end - start);
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
    }
}
