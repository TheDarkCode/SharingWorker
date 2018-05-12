using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace SharingWorker.FileHost
{
	[Export(typeof(IFileHost))]
	class Zippyshare : PropertyChangedBase, IFileHost
	{
		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
		private CookieContainer cookies;

		public string Name => "Zippyshare";
		public string User { get; set; }
		public string Password { get; set; }

		private bool enabled;
		public bool Enabled
		{
			get { return enabled; }
			set
			{
				enabled = value;
				NotifyOfPropertyChange(() => Enabled);
			}
		}
		private bool loggedIn;
		public bool LoggedIn
		{
			get { return loggedIn; }
			set
			{
				loggedIn = value;
				NotifyOfPropertyChange(() => LoggedIn);
			}
		}

		public bool LoadConfig()
		{
			var config = ConfigurationManager.GetSection("Zippyshare") as NameValueCollection;
			if (config == null) return false;

			try
			{
				Enabled = bool.Parse(config["Enabled"]);
				User = config["User"];
				Password = config["Password"];
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task LogIn()
		{
			cookies = new CookieContainer();

			try
			{
				using (var handler = new HttpClientHandler { CookieContainer = cookies })
				using (var client = new HttpClient(handler))
				{
					client.DefaultRequestHeaders.Add("User-Agent",
						"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36");

					client.DefaultRequestHeaders.Referrer = new Uri("https://www.zippyshare.com/");
					client.DefaultRequestHeaders.ExpectContinue = false;

					var content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("login", User),
						new KeyValuePair<string, string>("pass", Password),
					});

					using (var response = await client.PostAsync("https://www.zippyshare.com/services/login", content))
					{
						var result = await response.Content.ReadAsStringAsync();
						LoggedIn = result.IndexOf("logout", StringComparison.OrdinalIgnoreCase) > 0;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to login to {this.GetType().Name}.", ex);
			}
		}

		public async Task<List<string>> GetLinks(string filename, bool withFileName = false)
		{
			try
			{
				using (var handler = new HttpClientHandler { CookieContainer = cookies })
				using (var client = new HttpClient(handler))
				{
					client.DefaultRequestHeaders.Add("User-Agent",
						"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36");

					client.DefaultRequestHeaders.Referrer = new Uri("https://www.zippyshare.com/sites/priv/fileManager.jsp");
					client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
					client.DefaultRequestHeaders.Add("Accept", "text/html, */*; q=0.01");
					client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
					client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
					client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
					
					var content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("page", "0"),
						new KeyValuePair<string, string>("dir", "0"),
						new KeyValuePair<string, string>("sort", string.Empty),
						new KeyValuePair<string, string>("search", filename),
					});

					using (var response = await client.PostAsync("https://www.zippyshare.com/fragments/myAccount/filetable.jsp", content))
					{
						var links = new List<string>();
						var result = await response.Content.ReadAsStringAsync();

						const string startPattern = "\"><a class=\"name\" href=\"//";
						foreach (var searchStart in result.AllIndexesOf(startPattern))
						{
							var start = searchStart + startPattern.Length;
							var end = result.IndexOf("\"", start, StringComparison.Ordinal);
							var link = result.Substring(start, end - start);
							if (link.Length > 50)
							{
								throw new NotSupportedException($"Bad link: [{link}]");
							}

							links.Add($"https://{link}");
						}
						return links;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to get links from {this.GetType().Name}.", ex);
				return Enumerable.Empty<string>().ToList();
			}
		}
	}
}
