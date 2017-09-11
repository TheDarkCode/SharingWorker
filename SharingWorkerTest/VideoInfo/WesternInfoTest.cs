using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;

namespace SharingWorkerTest.VideoInfo
{
    [TestClass, TestCategory("Western Info")]
    public class WesternInfoTest
    {
        [TestMethod]
        public async Task TestLegalPorno()
        {
            var info = await WesternInfo.GetLegalPorno("SZ1726");
            Trace.WriteLine(info.Title);
            info = await WesternInfo.GetLegalPorno("GIO432");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestZTOD()
        {
            var info = await WesternInfo.GetZTOD("ZTOD_lauren-phillips-digs-salsa-also-and-spanish-dick");
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
            var info = await WesternInfo.GetTeamSkeet("TS_country_biscuit_gets");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetTeamSkeet("TS_gotta_work_for");
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
        public async Task TestDarkX()
        {
            var info = await WesternInfo.GetDarkX("DX_127066");
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

        [TestMethod]
        public async Task TestLUBED()
        {
            var info = await WesternInfo.GetLUBED("LUBED_meshing-together");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetLUBED("LUBED_wet-brunette");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestSPIZOO()
        {
            var info = await WesternInfo.GetSPIZOO("SPIZOO_Riley-Reyes-College-Fan");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetSPIZOO("SPIZOO_The-Ring-Fight");
            Trace.WriteLine(info.Title);
            
            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestHushPass()
        {
            var info = await WesternInfo.GetHushPass("HushPass_young-blonde-haley-reamed-inside-out");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetHushPass("HushPass_blonde-riley-reyes-takes-a-cock-stuffing");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }

        [TestMethod]
        public async Task TestDDFNetwork()
        {
            var info = await WesternInfo.GetDDFNetwork("DDF_20486");
            Trace.WriteLine(info.Title);

            info = await WesternInfo.GetDDFNetwork("DDF_21125");
            Trace.WriteLine(info.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(info.Title));
        }
    }
}
