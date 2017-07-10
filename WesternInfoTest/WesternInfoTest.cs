using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;

namespace WesternInfoTest
{
    [TestClass]
    public class WesternInfoTest
    {
        [TestMethod]
        public async Task TestZTOD()
        {
            var info = await WesternInfo.GetZTOD("ZTOD_21712");
            Trace.WriteLine(info.Title);
            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestMOFOS()
        {
            var info = await WesternInfo.GetMOFOS("MOFOS_hot-blonde-fucks");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetMOFOS("MOFOS_late-rent-leads");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestTeamSkeet()
        {
            var info = await WesternInfo.GetTeamSkeet("TS_tickets_to_the_ass");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetTeamSkeet("TS_anal_seduction");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }
    }
}
