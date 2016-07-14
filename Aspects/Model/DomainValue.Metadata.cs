using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model
{
    abstract class DomainValueMetadata
    {
        [Key]
        [Column(Order = 0)]
        [Dump(0)]
        public object Id { get; set; }
    }
}