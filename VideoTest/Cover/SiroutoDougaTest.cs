using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;

namespace VideoTest.Cover
{
    [TestClass, TestCategory("SiroutoDouga Cover")]
    public class SiroutoDougaTest
    {
        [TestMethod]
        public void TestSIRO()
        {
            var urls = SiroutoDouga.GetCoverUrls("SIRO-3146");
            foreach (var url in urls) Trace.WriteLine(url);
            Assert.IsTrue(urls.Any());
        }

        [TestMethod]
        public void Test261ARA()
        {
            var urls = SiroutoDouga.GetCoverUrls("261ARA-197");
            foreach (var url in urls) Trace.WriteLine(url);
            Assert.IsTrue(urls.Any());
        }
    }
}
