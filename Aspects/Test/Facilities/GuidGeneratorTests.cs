using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;

namespace vm.Aspects.Tests.Facilities
{
    [TestClass]
    public class GuidGeneratorTests : GenericIGuidGeneratorTests
    {
        protected override IGuidGenerator GetGenerator() => new GuidGenerator();
    }
}
