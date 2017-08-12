using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;

namespace VideoTest.Info
{
    [TestClass, TestCategory("Western Info")]
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

        [TestMethod]
        public async Task TestHOLED()
        {
            var info = await WesternInfo.GetHOLED("HOLED_blonde-teen-gape");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetHOLED("HOLED_best-friends-anal");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestTeenErotica()
        {
            var info = await WesternInfo.GetTeenErotica("TE_GroupSexIs");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestJulesJordan()
        {
            var info = await WesternInfo.GetJulesJordan("JJ_TeenAssIsOpen");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetJulesJordan("JJ_ThisRussianHacks");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestJulesJordanVideo()
        {
            var info = await WesternInfo.GetJulesJordanVideo("JJV_874_EllaNova");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestSPYFAM()
        {
            var info = await WesternInfo.GetSPYFAM("SPYFAM_dad-busy-watching-game-step-siblings-busy-fucking");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetSPYFAM("SPYFAM_step-brother-blackmails-lesbian-step-sister-to-do-3-some");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestNubileFilms()
        {
            var info = await WesternInfo.GetNubileFilms("NubileFilms_45652");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetNubileFilms("NubileFilms_45640");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestEvilAngel()
        {
            var info = await WesternInfo.GetEvilAngel("EA_126041");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetEvilAngel("EA_126051");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestNaughtyAmerica()
        {
            var info = await WesternInfo.GetNaughtyAmerica("NA_lexi-lovell-22971");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetNaughtyAmerica("NA_brett-rossi-stella-cox-21573");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestHardX()
        {
            var info = await WesternInfo.GetHardX("HX_121257");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestAnalized()
        {
            var info = await WesternInfo.GetAnalized("Analized_Zoey-Madelyn-Monroe-Part1");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestVIXEN()
        {
            var info = await WesternInfo.GetVIXEN("VIXEN_he-loves-my-big-butt");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetVIXEN("VIXEN_my-girlfriend-and-i-tried-a-threesome");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestBangBros()
        {
            var info = await WesternInfo.GetBangBros("BB_3406665");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetBangBros("BB_3406627");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestBLACKED()
        {
            var info = await WesternInfo.GetBLACKED("BLACKED_i-like-it-kinky");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetBLACKED("BLACKED_ive-never-done-this-before");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }
    }
}
