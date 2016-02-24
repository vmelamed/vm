using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Facilities;

namespace vm.Aspects.Tests.Facilities
{
    [TestClass]
    public class ClockTests : GenericIClockTests
    {
        protected override IClock GetClock() => new Clock();
    }
}
