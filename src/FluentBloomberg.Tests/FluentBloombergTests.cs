using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluentBloomberg.Tests
{
    [TestClass]
    public class FluentBloombergTests
    {
        IFluentBloomberg _fBB;

        [TestInitialize]
        public void Setup()
        {
            _fBB = new FluentBloomberg();
        }

        [TestMethod]
        public void TestMethod1()
        {
            var result = _fBB.Security("VOD LN Equity")
                .Field("PX_LAST")
                .Lookup();

            Assert.IsNotNull(result);
        }
    }
}
