﻿using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharingWorker.UrlShortening;

namespace SharingWorkerTest
{
    [TestClass, TestCategory("Url Shortening")]
    public class UrlShorteningTest
    {
        [TestMethod]
        public async Task TestOkeIo()
        {
            var shortener = new OkeIo();
            var result = await shortener.GetLink("https://www.blogger.com");
            Assert.IsTrue(!string.IsNullOrEmpty(result));
        }
    }
}
