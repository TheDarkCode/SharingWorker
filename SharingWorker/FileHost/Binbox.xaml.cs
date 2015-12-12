using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using WebKit;

namespace SharingWorker.FileHost
{
    public partial class Binbox : UserControl
    {
        public enum ApiUrlType { Main, Backup, Restore }

        public static bool GetEnabled;
        public static bool CheckEnabled;

        private static readonly string mainApiUrl = ((NameValueCollection)ConfigurationManager.GetSection("Binbox"))["ApiUrl"];
        private static readonly string backupApiUrl = ((NameValueCollection)ConfigurationManager.GetSection("Binbox"))["BackupApiUrl"];
        private static readonly string restoreApiUrl = ((NameValueCollection)ConfigurationManager.GetSection("Binbox"))["RestoreApiUrl"];
        //private static readonly string user = ((NameValueCollection)ConfigurationManager.GetSection("Binbox"))["User"];
        //private static readonly string password = ((NameValueCollection)ConfigurationManager.GetSection("Binbox"))["Password"];

        private static WebKitBrowser browser;
        private static string scriptResult;
        private static bool scriptFinished;
        //private static CookieContainer cookies;

        private enum ApiType { Encode, Decode }

        public Binbox()
        {
            InitializeComponent();
            browser = new WebKitBrowser {AllowNavigation = true};
            WinFormHost.Child = browser;

            var url = new Uri(AppDomain.CurrentDomain.BaseDirectory + "binbox.api", UriKind.RelativeOrAbsolute).AbsoluteUri;
            browser.Navigate(url);
        }

        public static async Task<string> GetEncodedLink(string title, string text, ApiUrlType type)
        {
            scriptFinished = false;
            var apiUrl = "";

            switch (type)
            {
                case ApiUrlType.Main:
                    apiUrl = mainApiUrl;
                    break;
                case ApiUrlType.Backup:
                    apiUrl = backupApiUrl;
                    break;
                case ApiUrlType.Restore:
                    apiUrl = restoreApiUrl;
                    break;
            }
            
            var jsPath = GenerateApiScript(apiUrl, ApiType.Encode, title, text);
            var js = File.ReadAllText(jsPath);
            browser.GetScriptManager.ConsoleMessageAdded += GetScriptManager_ConsoleMessageAdded;
            browser.GetScriptManager.EvaluateScript(js);
            
            await Task.Run(() =>
            {
                while (!scriptFinished)
                    SpinWait.SpinUntil(() => false, 1);
            });

            return scriptResult;
        }

        public static async Task<string> GetDecodedContent(string id, string password)
        {
            scriptFinished = false;
            var jsPath = GenerateApiScript(mainApiUrl, ApiType.Decode, null, null, id, password);
            var js = File.ReadAllText(jsPath);
            browser.GetScriptManager.ConsoleMessageAdded += GetScriptManager_ConsoleMessageAdded;
            browser.GetScriptManager.EvaluateScript(js);

            await Task.Run(() =>
            {
                while (!scriptFinished)
                    SpinWait.SpinUntil(() => false, 1);
            });

            return scriptResult;
        }

        private static string GenerateApiScript(string apiUrl, ApiType apiType, string title = null, string text = null, string id = null, string password = null)
        {
            var js = "";
            switch (apiType)
            {
                case ApiType.Encode:
                    js = string.Format(@"var BB = new Binbox.API('{0}');
BB.create({1}
	title: '{9}',
	text: '{10}',
{2}, function(result)
{3}
	if(result.ok) 
	{4}
        console.log('https://binbox.io/' + result.id + '#' + result.salt);
	{5}
	else 
	{6}
		console.log(result.error);
	{7}
{8});
", apiUrl, "{", "}", "{", "{", "}", "{", "}", "}", title, text);
                    break;
                case ApiType.Decode:
                    js = string.Format(@"var BB = new Binbox.API('{0}');
var password = '{1}';
BB.retrieve('{2}', function(result)
{3}
	if(!result.ok)
	{4}
		return;
	{5}
	var text = result.decrypt(password);
	console.log(text);
{6});	
", apiUrl, password, id, "{", "{", "}", "}");
                    break;
            }

            var path = AppDomain.CurrentDomain.BaseDirectory + "temp.js";
            File.WriteAllText(path, js, Encoding.UTF8);
            return path;
        }

        static void GetScriptManager_ConsoleMessageAdded(object sender, ConsoleEventArgs e)
        {
            scriptResult = e.Message;
            browser.GetScriptManager.ConsoleMessageAdded -= GetScriptManager_ConsoleMessageAdded;
            scriptFinished = true;
        }
        
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (browser != null)
                browser.Dispose();
            if (WinFormHost != null)
                WinFormHost.Dispose();
        }

        //public async Task<bool> LogIn()
        //{
        //    cookies = new CookieContainer();

        //    using (var handler = new HttpClientHandler { CookieContainer = cookies })
        //    using (var client = new HttpClient(handler))
        //    {
        //        client.BaseAddress = new Uri("http://binbox.io");

        //        var validate = "";
        //        using (var response = await client.GetAsync("/login"))
        //        {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            var start = result.IndexOf("http://binbox.io/login/submit", 0);
        //            if (start < 0) return false;
        //            var search = "<input type=\"hidden\" name=\"validate\" value=\"";
        //            start = result.IndexOf(search, start);
        //            if (start < 0) return false;
        //            start = start + search.Length;
        //            var end = result.IndexOf("\"", start);
        //            if (end < 0 || end - start < 0 || end - start > 32) return false;
        //            validate = result.Substring(start, end - start);
        //        }

        //        var content = new FormUrlEncodedContent(new[]
        //        {
        //            new KeyValuePair<string, string>("validate", validate),
        //            new KeyValuePair<string, string>("username", user),
        //            new KeyValuePair<string, string>("password", password),
        //        });

        //        using (var response = await client.PostAsync("/login/submit", content))
        //        {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            if (result.IndexOf("logout", StringComparison.OrdinalIgnoreCase) >= 0)
        //                return true;
        //        }
        //    }
        //    return false;
        //}

        //public async Task<bool> SearchTitle()
        //{
        //    cookies = new CookieContainer();

        //    using (var handler = new HttpClientHandler { CookieContainer = cookies })
        //    using (var client = new HttpClient(handler))
        //    {
        //        client.BaseAddress = new Uri("http://binbox.io");

        //        var validate = "";
        //        using (var response = await client.GetAsync("/login"))
        //        {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            var start = result.IndexOf("http://binbox.io/login/submit", 0);
        //            if (start < 0) return false;
        //            var search = "<input type=\"hidden\" name=\"validate\" value=\"";
        //            start = result.IndexOf(search, start);
        //            if (start < 0) return false;
        //            start = start + search.Length;
        //            var end = result.IndexOf("\"", start);
        //            if (end < 0 || end - start < 0 || end - start > 32) return false;
        //            validate = result.Substring(start, end - start);
        //        }

        //        var content = new FormUrlEncodedContent(new[]
        //        {
        //            new KeyValuePair<string, string>("validate", validate),
        //            new KeyValuePair<string, string>("username", user),
        //            new KeyValuePair<string, string>("password", password),
        //        });

        //        using (var response = await client.PostAsync("/login/submit", content))
        //        {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            if (result.IndexOf("logout", StringComparison.OrdinalIgnoreCase) >= 0)
        //                return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
