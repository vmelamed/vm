using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    [MetadataType(typeof(Class1Metadata))]
    partial class Class1
    {
        public int Property { get; set; }
        public string Property2 { get; set; }
        public string Property3 { get; set; }
        string Property4 { get; set; }
        int _field { get; set; }
        int foo();
    }
}
