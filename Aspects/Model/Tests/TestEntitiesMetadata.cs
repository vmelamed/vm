using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Diagnostics;
using vm.Aspects.Validation;

namespace vm.Aspects.Model.Tests
{
    abstract class TestXEntityMetadata
    {
        [Dump(0)]
        [NonemptyStringValidator]
        public object Name { get; set; }

        [Dump(1)]
        [OptionalStringLengthValidator(30)]
        public object StringProperty { get; set; }

        [Dump(-1)]
        [NotBeforeValidator("1900-01-01T00:00:00.0000000Z")]
        public object Created { get; set; }

        [Dump(-2)]
        [NotBeforeValidator("1900-01-01T00:00:00.0000000Z")]
        [PropertyComparisonValidator("Created", ComparisonOperator.GreaterThanEqual)]
        public object Updated { get; set; }
    }

    abstract class TestEntityMetadata
    {
        [Dump(0)]
        [NonemptyStringValidator]
        public object Name { get; set; }

        [Dump(1)]
        [OptionalStringLengthValidator(30)]
        public object StringProperty { get; set; }

        [Dump(RecurseDump=ShouldDump.Skip)]
        public object XEntity { get; set; }

        [Dump(RecurseDump=ShouldDump.Skip)]
        public object Value { get; set; }

        [Dump(2)]
        public object XEntity_Id { get; set; }

        [Dump(3)]
        public object Value_Id { get; set; }

        [Dump(-1)]
        [NotBeforeValidator("1900-01-01T00:00:00.0000000Z")]
        public object Created { get; set; }

        [Dump(-2)]
        [NotBeforeValidator("1900-01-01T00:00:00.0000000Z")]
        [PropertyComparisonValidator("Created", ComparisonOperator.GreaterThanEqual)]
        public object Updated { get; set; }
    }

    abstract class TestEntity1Metadata
    {
    }

    abstract class TestEntity2Metadata
    {
        [Dump(RecurseDump=ShouldDump.Skip)]
        public virtual ICollection<TestValue> InternalValues { get; set; }
    }

    public class TestValueMetadata
    {
        [OptionalStringLengthValidator(10)]
        [Dump(0)]
        public virtual string Name { get; set; }

        [Dump(RecurseDump=ShouldDump.Skip)]
        public virtual TestEntity Entity { get; set; }

        [Dump(1)]
        public virtual TestEntity2 Entity2 { get; set; }
        public virtual long? Entity2_Id { get; set; }
    }
}
