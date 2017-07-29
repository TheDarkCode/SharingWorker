﻿using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;

namespace VideoTest.Info
{
    [TestClass, TestCategory("Video Info")]
    public class VideoInfoTest
    {
        [TestMethod]
        public async Task TestRealStreetAngels()
        {
            var info = await RealStreetAngels.GetInfo("m407_misa");
            Trace.WriteLine(info.Title);
            info = await RealStreetAngels.GetInfo("m410_hibiki");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }
    }
}