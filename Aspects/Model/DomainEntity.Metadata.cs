﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model
{
    abstract class DomainEntityMetadata
    {
        [NotMapped]
        [Dump(0)]
        public object HasIdentity { get; set; }

        [NotMapped]
        [Dump(1)]
        public object Key { get; set; }

        [Key]
        [Column(Order = 0)]
        [Dump(2)]
        public object Id { get; set; }
    }
}