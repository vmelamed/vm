using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;

namespace vm.Aspects.Tests.Facilities
{
    /// <summary>
    /// Summary description for TestGuidGeneratorTests
    /// </summary>
    [TestClass]
    public class TestGuidGeneratorTests : GenericIGuidGeneratorTests
    {
        protected override IGuidGenerator GetGenerator() => new TestGuidGenerator();

        [TestMethod]
        public void NonDefaultGeneratorTest()
        {
            var target = new TestGuidGenerator(1000, 10);

            Assert.AreEqual(new Guid(1000, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.StartGuid);
            Assert.AreEqual(new Guid(1000, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NextGuid);
            Assert.AreEqual(new Guid(1000, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NewGuid());

            Assert.AreEqual(new Guid(1010, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NextGuid);
            Assert.AreEqual(new Guid(1010, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NewGuid());
            Assert.AreEqual(new Guid(1020, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NextGuid);

            target.Reset();

            Assert.AreEqual(new Guid(1000, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.StartGuid);
            Assert.AreEqual(new Guid(1000, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NextGuid);
            Assert.AreEqual(new Guid(1000, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NewGuid());

            Assert.AreEqual(new Guid(1010, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NextGuid);
            Assert.AreEqual(new Guid(1010, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NewGuid());
            Assert.AreEqual(new Guid(1020, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, }), target.NextGuid);
        }
    }
}
