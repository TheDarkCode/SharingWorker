using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;

namespace SharingWorkerTest.VideoInfo
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

        [TestMethod]
        public async Task TestCaribbeancompr()
        {
            var info = await SharingWorker.Video.VideoInfo.GetVideoInfo_caribpr("071317_001-caribpr");
            Trace.WriteLine(info.Title);
            info = await SharingWorker.Video.VideoInfo.GetVideoInfo_caribpr("071317_002-caribpr");
            Trace.WriteLine(info.Title);
            info = await SharingWorker.Video.VideoInfo.GetVideoInfo_caribpr("080117_001-caribpr");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestCaribbeancom()
        {
            var info = await SharingWorker.Video.VideoInfo.GetVideoInfo_carib("081217-478");
            Trace.WriteLine(info.Title);
            info = await SharingWorker.Video.VideoInfo.GetVideoInfo_carib("081017-006");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestDmm()
        {
            var info = await SharingWorker.Video.VideoInfo.GetVideoInfo_Dmm("SNIS908");
            Trace.WriteLine(info.Title);
            info = await SharingWorker.Video.VideoInfo.GetVideoInfo_Dmm("SNIS918");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }
    }
}
