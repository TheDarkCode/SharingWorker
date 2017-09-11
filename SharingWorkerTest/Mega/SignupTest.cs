using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.FileHost;
using SharingWorker.MailHost;

namespace SharingWorkerTest.Mega
{
    [TestClass, TestCategory("Mega.nz Signup")]
    public class SignupTest
    {
        private async Task TestSignup(IMailHost host)
        {
            MEGA.Password = "2wsx3EDC";
            var account = await MEGA.CreateNewAccount(host);
            if (!string.IsNullOrEmpty(account))
                Trace.WriteLine(account);
            Assert.IsTrue(!string.IsNullOrEmpty(account));
        }

        [TestMethod]
        public async Task TestNada()
        {
            var mailHost = new Nada();
            await TestSignup(mailHost);
        }

        [TestMethod]
        public async Task TestTempMailRu()
        {
            var mailHost = new TempMailRu();
            await TestSignup(mailHost);
        }
    }
}
