using System;

namespace vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample1
{
    class MyClass
    {
        public bool BoolProperty { get; set; }
        public int IntProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public Uri UriProperty { get; set; }

        public override string ToString() => this.DumpString();
    }

    class Program
    {
        static void Main()
        {
            int anInt = 5;
            var anObject = new MyClass
            {
                BoolProperty = true,
                IntProperty  = 3,
                GuidProperty = Guid.Empty,
            };

#if false
            // Initial fully spelled-out version:
            using (var writer = new StringWriter())
            {
                var dumper = new ObjectTextDumper(writer);

                // dump a primitive value:
                dumper.Dump(anInt);
                // dump complex value:
                dumper.Dump(anObject);
                Console.WriteLine(writer.GetStringBuilder().ToString());
            }
#elif false
            // Alternative #1. Use the DumpText() extension method:
            using (var writer = new StringWriter())
            {
                anInt.DumpText(writer);
                anObject.DumpText(writer);
                Console.WriteLine(writer.GetStringBuilder().ToString());
            }
#elif true
            // Alternative #2. Use the DumpString() extension method:
            Console.WriteLine(anInt.DumpString());
            Console.WriteLine(anObject.DumpString());
#elif false
            // Alternative #3. Override MyClass.ToString()
            Console.WriteLine(anInt.DumpString());
            Console.WriteLine(anObject.ToString());
#endif

            Console.Write("Press any key to finish...");
            Console.ReadKey(true);
        }
    }
}
