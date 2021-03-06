﻿using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharingWorkerTest.VideoCover
{
    [TestClass, TestCategory("Video Cover")]
    public class VideoCoverTest
    {
        [TestMethod]
        public async Task TestDmm()
        {
            var url = await SharingWorker.Video.VideoCover.QueryDmmImage("MIAE057");
            Trace.WriteLine(url);
            url = await SharingWorker.Video.VideoCover.QueryDmmImage("SNIS909");
            Trace.WriteLine(url);
            Assert.IsTrue(!string.IsNullOrEmpty(url));
        }

	    [TestMethod]
	    public async Task Test1pon()
	    {
		    var url = await SharingWorker.Video.VideoCover.Get1ponCover("051218_686-1pon");
		    Trace.WriteLine(url);
		    Assert.IsTrue(!string.IsNullOrEmpty(url));
	    }
	}
}
