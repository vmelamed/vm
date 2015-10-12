using vm.Aspects.Diagnostics;

namespace ConsoleApplication1
{
    abstract class Class1Metadata
    {
        [Dump(0)]
        public object Property { get; set; }

        [Dump(1)]
        public object Property2 { get; set; }

        [Dump(2)]
        public object Property3 { get; set; }

        [Dump(3)]
        public object Property4 { get; set; }

        [Dump(4)]
        public object _field { get; set; }
    }
}