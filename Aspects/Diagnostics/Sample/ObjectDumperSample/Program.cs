using System;
using vm.Aspects;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Diagnostics.ObjectDumper.Sample
{
    [Dump(DumpNullValues = ShouldDump.Skip)]
    class MyClass
    {
        [Dump(2)]                               // or [Dump(Order=2)]
        public bool BoolProperty
        { get; set; }

        [Dump(1)]
        public int IntProperty
        { get; set; }

        [Dump(-1)]
        public Guid GuidProperty
        { get; set; }

        [Dump(0)]
        public Uri UriProperty
        { get; set; }

        public override string ToString() => this.DumpString();
    }

    [Dump(2, MaxDepth = 3)]
    class MyClassDescendant : MyClass
    {
        [Dump(0)]
        public string StringProperty
        { get; set; }

        [Dump(1, MaxLength = 25)]
        public string Description
        { get; set; }

        [Dump(2)]
        public ComplexType Associate
        { get; set; }
    }

    class ComplexType
    {
        [Dump(0)]
        public string Key
        { get; set; }

        public Guid UniqueId
        { get; set; }

        [Dump(-1)]
        public ComplexType Other
        { get; set; }
    }

    class Program
    {
        static void Main()
        {
            var anObject = new MyClassDescendant
            {
                StringProperty = "StringProperty",
                //Description    = "This is one very very very very very long description",
                BoolProperty = true,
                IntProperty = 3,
                GuidProperty = Guid.NewGuid(),
                //CreatedAt      = DateTime.Now,
                UriProperty = new Uri("https://github.com/vmelamed/vm"),
                Associate = new ComplexType
                {
                    Key = "IL.TX.2013-09-10:20:23:34.85930",
                    UniqueId = Guid.NewGuid(),
                    Other = new ComplexType
                    {
                        Key = "IL.TX.2013-09-10:20:23:34.85931",
                        UniqueId = Guid.NewGuid(),
                        Other = new ComplexType
                        {
                            Key = "IL.TX.2013-09-10:20:23:34.85932",
                            UniqueId = Guid.NewGuid(),
                            Other = new ComplexType
                            {
                                Key = "IL.TX.2013-09-10:20:23:34.85933",
                                UniqueId = Guid.NewGuid(),
                                Other = new ComplexType
                                {
                                    Key = "IL.TX.2013-09-10:20:23:34.85934",
                                    UniqueId = Guid.NewGuid(),
                                    Other = new ComplexType
                                    {
                                        Key = "IL.TX.2013-09-10:20:23:34.85935",
                                        UniqueId = Guid.NewGuid(),
                                        Other = null,
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Console.WriteLine(anObject);

            Console.ReadKey(true);
        }
    }
}
