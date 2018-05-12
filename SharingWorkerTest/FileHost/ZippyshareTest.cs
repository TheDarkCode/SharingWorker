using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.FileHost;

namespace SharingWorkerTest.FileHost
{
	[TestClass, TestCategory("FileHost - Zippyshare")]
	public class ZippyshareTest
	{
		private readonly Zippyshare share = new Zippyshare();

		public ZippyshareTest()
		{
			share.User = "";
			share.Password = "";

			if (string.IsNullOrEmpty(share.User) || string.IsNullOrEmpty(share.Password))
			{
				throw new ArgumentNullException("User/Password");
			}
		}

		[TestMethod]
		public async Task TestLogin()
		{
			await share.LogIn();
			Assert.IsTrue(share.LoggedIn);
		}

		[TestMethod]
		public async Task TestGetLinks()
		{
			await share.LogIn();
			Assert.IsTrue(share.LoggedIn);

			var links = await share.GetLinks("MEYD320");
			foreach (var link in links)
			{
				Trace.WriteLine(link);
			}
			Assert.IsTrue(links.Any());
		}
	}
}
