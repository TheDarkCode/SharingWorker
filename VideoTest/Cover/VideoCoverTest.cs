using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;

namespace VideoTest.Cover
{
    [TestClass, TestCategory("Video Cover")]
    public class VideoCoverTest
    {
        [TestMethod]
        public async Task TestDmm()
        {
            var url = await VideoCover.QueryDmmImage("MIAE057");
            Trace.WriteLine(url);
            url = await VideoCover.QueryDmmImage("SNIS909");
            Trace.WriteLine(url);
            Assert.IsTrue(!string.IsNullOrEmpty(url));
        }
    }
}
