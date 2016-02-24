using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;

namespace vm.Aspects.Tests.Facilities
{
    [TestClass]
    public class TestClockTests : GenericIClockTests
    {
        protected override IClock GetClock() => new TestClock();

        [TestMethod]
        public void NonDefaultClockTest()
        {
            var clock = new TestClock(new DateTime(1, 1, 1, 1, 0, 0), new TimeSpan(0, 0, 2));

            Assert.AreEqual(new DateTime(1, 1, 1, 1, 0, 0), clock.UtcNow);
            Assert.AreEqual(new DateTime(1, 1, 1, 1, 0, 2), clock.UtcNow);

            clock.Reset();
            Assert.AreEqual(new DateTime(1, 1, 1, 1, 0, 0), clock.UtcNow);

            clock.StartTime = new DateTime(1000, 1, 1, 1, 0, 0);
            Assert.AreEqual(new DateTime(1000, 1, 1, 1, 0, 0), clock.UtcNow);
            Assert.AreEqual(new DateTime(1000, 1, 1, 1, 0, 2), clock.UtcNow);
            Assert.AreEqual(new DateTime(1000, 1, 1, 1, 0, 4), clock.NextTime);
            Assert.AreEqual(new DateTime(1000, 1, 1, 1, 0, 4), clock.UtcNow);
        }
    }
}
