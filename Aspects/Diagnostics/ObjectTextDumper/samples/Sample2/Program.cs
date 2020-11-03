using System;

namespace vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2
{
    class MyClass
    {
        [Dump(0)]
        public virtual string StringProperty { get; set; } = "myClass.StringProperty";
        [Dump(2)]                               // or [Dump(Order=2)]
        public bool BoolProperty { get; set; } = true;
        [Dump(1)]
        public int IntProperty { get; set; } = 3;
        [Dump(-1)]
        public Guid GuidProperty { get; set; } = Guid.NewGuid();
        [Dump(2)]
        public Uri UriProperty { get; set; }
        [Dump(false)]
        public string Unimportant { get; set; } = "whatever";
    }

    class MyClassDescendant : MyClass
    {
        [Dump(5)]
        public override string StringProperty { get; set; } = "myClassDescendant.StringProperty";
    }

    class MyClass1
    {
        [Dump(0, Mask = true/*, MaskValue = "------"*/)]
        public virtual string SSN { get; set; } = "123456789";
        [Dump(2)]                               // or [Dump(Order=2)]
        public bool BoolProperty { get; set; } = true;
        [Dump(1)]
        public int IntProperty { get; set; } = 3;
        [Dump(-1)]
        public Guid GuidProperty { get; set; } = Guid.NewGuid();
        [Dump(2)]
        public Uri UriProperty { get; set; }
        [Dump(false)]
        public string Unimportant { get; set; } = "whatever";
        [Dump(MaxLength = 25, LabelFormat = "{0,-12} (Truncated) = ")]
        public string Description { get; set; } = "This is one very very very very very long description";
        [Dump(ValueFormat = "{0:o}")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    class Program
    {
        static void Main(string[] args)
        {
            DumpFormat.Type = "{0}:";
            Console.WriteLine(new MyClass().DumpString());
            Console.WriteLine(new MyClassDescendant().DumpString());
            Console.WriteLine(new MyClass1().DumpString());

            Console.WriteLine(new MyClass1().DumpString());

            Console.Write("Press any key to finish...");
            Console.ReadKey(true);
        }
    }
}
