using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.FileHost;
using SharingWorker.MailHost;

namespace SharingWorkerTest.Mega
{
    [TestClass, TestCategory("Mega.nz Signup")]
    public class SignupTest
    {
        [TestMethod]
        public async Task TestNada()
        {
            var nada = new Nada();
            MEGA.Password = "2wsx3EDC";
            var account = await MEGA.CreateNewAccount(nada);
            if (!string.IsNullOrEmpty(account))
                Trace.WriteLine(account);
            Assert.IsTrue(!string.IsNullOrEmpty(account));
        }
    }
}
