using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using vm.Aspects.Facilities;

namespace vm.Aspects.Model.Tests
{
    [MetadataType(typeof(TestXEntityMetadata))]
    public partial class TestXEntity : DomainEntity<long, string>
    {
        public override string Key => Name;

        public virtual string Name { get; set; }

        public virtual string StringProperty { get; set; }

        public virtual DateTime Created { get; set; }

        public virtual DateTime Updated { get; set; }

        public virtual TestXEntity SetUpdated(
            DateTime? dateTime = null)
        {
            Updated = dateTime ?? Facility.Clock.UtcNow;
            return this;
        }
    }

    [MetadataType(typeof(TestEntityMetadata))]
    public partial class TestEntity : DomainEntity<long, string>
    {
        public override string Key => Name;

        public virtual string Name { get; set; }

        public virtual string StringProperty { get; set; }

        public virtual TestXEntity XEntity { get; set; }

        public virtual long? XEntityId { get; set; }

        public virtual TestValue Value { get; set; }

        public virtual long? ValueId { get; set; }

        public virtual DateTime Created { get; set; }

        public virtual DateTime Updated { get; set; }

        public virtual TestEntity SetUpdated(
            DateTime? dateTime = null)
        {
            Updated = dateTime ?? Facility.Clock.UtcNow;
            return this;
        }
    }

    [MetadataType(typeof(TestEntity1Metadata))]
    public partial class TestEntity1 : TestEntity
    {
        public const string Discriminator = "TestEntity1";
    }

    [MetadataType(typeof(TestEntity2Metadata))]
    public partial class TestEntity2 : TestEntity1
    {
        public new const string Discriminator = "TestEntity2";

        public virtual ICollection<TestValue> InternalValues { get; set; }
    }

    [MetadataType(typeof(TestValueMetadata))]
    public partial class TestValue : DomainValue<long>
    {
        public virtual string Name { get; set; }

        public virtual TestEntity Entity { get; set; }

        public virtual TestEntity2 Entity2 { get; set; }
        public virtual long? Entity2Id { get; set; }
    }
}
