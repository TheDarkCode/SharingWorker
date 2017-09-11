using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.FileHost;

namespace SharingWorkerTest.FileHost
{
    [TestClass, TestCategory("FileHost - UploadGIG")]
    public class UploadGIGTest
    {
        public UploadGIGTest()
        {
            UploadGIG.User = "fikovixilo@p33.org";
            UploadGIG.Password = "1qaz2wsx";
        }
        
        [TestMethod]
        public async Task TestLogin()
        {
            Assert.IsTrue(await UploadGIG.LogIn());
        }

        [TestMethod]
        public async Task TestGetLinks()
        {
            Assert.IsTrue(await UploadGIG.LogIn());
            var links = await UploadGIG.GetLinks("MISM");
            foreach (var link in links)
            {
                Trace.WriteLine(link);
            }
            Assert.IsTrue(links.Any());
        }
    }
}
