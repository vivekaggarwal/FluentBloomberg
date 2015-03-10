using BloombergApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BloombergAPI.Tests
{
    [TestClass]
    public class BloombergServiceTests
    {
        IBloombergService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new BloombergService();
        }

        [TestMethod]
        public void TestLookupSingleSecurityAndField()
        {
            var result = _service.Lookup("VOD LN Equity", "PX_LAST");
            Assert.IsNotNull(result);
        }
    }
}
