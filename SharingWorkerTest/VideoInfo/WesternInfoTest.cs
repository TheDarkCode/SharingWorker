using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.Video;
using SharingWorker.Video.Western;

namespace SharingWorkerTest.VideoInfo
{
    [TestClass, TestCategory("Western Info")]
    public class WesternInfoTest
    {

        private async Task Test(IVideoInfo info, string id)
        {
            var videoInfo = await info.GetInfo(id);
            Trace.WriteLine(videoInfo.Title);

            Assert.IsTrue(!string.IsNullOrEmpty(videoInfo.Title));
        }

        [TestMethod]
        public async Task TestLegalPorno()
        {
            var info = new LegalPorno();
            await Test(info, "SZ1726");
            await Test(info, "GIO432");
        }

        [TestMethod]
        public async Task TestZTOD()
        {
            var info = new ZTOD();
            await Test(info, "ZTOD_lauren-phillips-digs-salsa-also-and-spanish-dick");
        }

        [TestMethod]
        public async Task TestMOFOS()
        { 
            var info = new MOFOS();
            await Test(info, "MOFOS_hot-blonde-fucks");
            await Test(info, "MOFOS_late-rent-leads");
        }

        [TestMethod]
        public async Task TestTeamSkeet()
        {
            var info = new TeamSkeet();
            await Test(info, "TS_country_biscuit_gets");
            await Test(info, "TS_gotta_work_for");
        }

        [TestMethod]
        public async Task TestPassionHD()
        {
            var info = new PassionHD();
            await Test(info, "phd_namaste-stepsis");
            await Test(info, "phd_poolside-pampering");
        }

        [TestMethod]
        public async Task TestHOLED()
        {
            var info = new HOLED();
            await Test(info, "HOLED_blonde-teen-gape");
            await Test(info, "HOLED_best-friends-anal");
        }

        [TestMethod]
        public async Task TestTeenErotica()
        {
            var info = new TeenErotica();
            await Test(info, "TE_GroupSexIs");
        }

        [TestMethod]
        public async Task TestJulesJordan()
        {
            var info = new JulesJordan();
            await Test(info, "JJ_TeenAssIsOpen");
            await Test(info, "JJ_ThisRussianHacks");
        }

        [TestMethod]
        public async Task TestJulesJordanVideo()
        {
            var info = new JulesJordanVideo();
            await Test(info, "JJV_874_EllaNova");
        }

        [TestMethod]
        public async Task TestSPYFAM()
        {
            var info = new SPYFAM();
            await Test(info, "SPYFAM_dad-busy-watching-game-step-siblings-busy-fucking");
            await Test(info, "SPYFAM_step-brother-blackmails-lesbian-step-sister-to-do-3-some");
        }

        [TestMethod]
        public async Task TestNubileFilms()
        {
            var info = new NubileFilms();
            await Test(info, "NubileFilms_45652");
            await Test(info, "NubileFilms_45640");
        }

        [TestMethod]
        public async Task TestEvilAngel()
        {
            var info = new EvilAngel();
            await Test(info, "EA_126041");
            await Test(info, "EA_126051");
        }

        [TestMethod]
        public async Task TestNaughtyAmerica()
        {
            var info = new NaughtyAmerica();
            await Test(info, "NA_lexi-lovell-22971");
            await Test(info, "NA_brett-rossi-stella-cox-21573");
        }

        [TestMethod]
        public async Task TestHardX()
        {
            var info = new HardX();
            await Test(info, "HX_121257");
        }

        [TestMethod]
        public async Task TestDarkX()
        {
            var info = new DarkX();
            await Test(info, "DX_127066");
        }

        [TestMethod]
        public async Task TestAnalized()
        {
            var info = new Analized();
            await Test(info, "Analized_Zoey-Madelyn-Monroe-Part1");
        }

        [TestMethod]
        public async Task TestVIXEN()
        {
            var info = new VIXEN();
            await Test(info, "VIXEN_he-loves-my-big-butt");
            await Test(info, "VIXEN_my-girlfriend-and-i-tried-a-threesome");
        }

        [TestMethod]
        public async Task TestBangBros()
        {
            var info = new BangBros();
            await Test(info, "BB_3406665");
            await Test(info, "BB_3406627");
        }

        [TestMethod]
        public async Task TestBLACKED()
        {
            var info = new BLACKED();
            await Test(info, "BLACKED_i-like-it-kinky");
            await Test(info, "BLACKED_ive-never-done-this-before");
        }

        [TestMethod]
        public async Task TestLUBED()
        {
            var info = new LUBED();
            await Test(info, "LUBED_meshing-together");
            await Test(info, "LUBED_wet-brunette");
        }

        [TestMethod]
        public async Task TestPOVD()
        {
            var info = new POVD();
            await Test(info, "POVD_morning-workout");
            await Test(info, "POVD_in-the-city-with-lily");
        }

        [TestMethod]
        public async Task TestSPIZOO()
        {
            var info = new SPIZOO();
            await Test(info,"SPIZOO_Riley-Reyes-College-Fan");
            await Test(info, "SPIZOO_The-Ring-Fight");
        }

        [TestMethod]
        public async Task TestHushPass()
        {
            var info = new HushPass();
            await Test(info, "HushPass_young-blonde-haley-reamed-inside-out");
            await Test(info, "HushPass_blonde-riley-reyes-takes-a-cock-stuffing");
        }

        [TestMethod]
        public async Task TestDDFNetwork()
        {
            var info = new DDFNetwork();
            await Test(info, "DDF_20486");
            await Test(info, "DDF_21125");
        }

        [TestMethod]
        public async Task TestBANG()
        {
            var info = new BANG();
            await Test(info, "BANG_5941a5fb5cb3c563ac3c9a4f");
            await Test(info, "BANG_581fa894d947081de9414811");
        }

        [TestMethod]
        public async Task TestNubilesPorn()
        {
            var info = new NubilesPorn();
            await Test(info, "Nubiles-Porn_48120");
            await Test(info, "Nubiles-Porn_40188");
        }

        [TestMethod]
        public async Task TestCherryPimps()
        {
            var info = new CherryPimps();
            await Test(info, "CherryPimps_15525-natashastarr");
            await Test(info, "CherryPimps_15920-whitneywright");
        }

        [TestMethod]
        public async Task TestPIMP()
        {
            var info = new PIMP();
            await Test(info, "PIMP_15472-emberstone");
            await Test(info, "PIMP_15471-alexagrace");
        }

        [TestMethod]
        public async Task TestBlackOnBlondes()
        {
            var info = new BlacksOnBlondes();
            await Test(info, "BlacksOnBlondes_riley_reid_melissa_moore");
            await Test(info, "BlacksOnBlondes_cadence_lux_riley_reyes");
        }

        [TestMethod]
        public async Task TestGloryHole()
        {
            var info = new GloryHole();
            await Test(info, "GloryHole_carmen_valentina_daizy_cooper");
            await Test(info, "GloryHole_riley_reyes");
        }
    }
}
