using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;

namespace SharingWorkerTest.VideoInfo
{
    [TestClass, TestCategory("SiroutoDouga Info")]
    public class SiroutoDougaTest
    {
        [TestMethod]
        public async Task TestSIRO()
        {
            var info = await SiroutoDouga.GetInfo("SIRO-3146");
            Trace.WriteLine(info.Title);
            info = await SiroutoDouga.GetInfo("SIRO-1992");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task Test200GANA()
        {
            var info = await SiroutoDouga.GetInfo("200GANA-602");
            Trace.WriteLine(info.Title);
            info = await SiroutoDouga.GetInfo("200GANA-1301");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task Test259LUXU()
        {
            var info = await SiroutoDouga.GetInfo("259LUXU-683");
            Trace.WriteLine(info.Title);
            info = await SiroutoDouga.GetInfo("259LUXU-748");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task Test261ARA()
        {
            var info = await SiroutoDouga.GetInfo("261ARA-157");
            Trace.WriteLine(info.Title);
            info = await SiroutoDouga.GetInfo("261ARA-197");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task Test300MIUM()
        {
            var info = await SiroutoDouga.GetInfo("300MIUM-081");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task Test277DCV()
        {
            var info = await SiroutoDouga.GetInfo("277DCV-053");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }
    }
}
