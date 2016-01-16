using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Diagnostics;
using vm.Aspects.Validation;

namespace vm.Aspects.Model.EFRepository.HiLoIdentity
{
    abstract class HiLoIdentityGeneratorMetadata
    {
        [Dump(0)]
        [StringLengthValidator(HiLoIdentityGenerator.EntitySetNameMaxLength)]
        [RegexValidator(RegularExpression.RexCSharpIdentifier)]
        public object EntitySetName { get; set; }

        [Dump(1)]
        [NonnegativeValidator]
        public object HighValue { get; set; }

        [Dump(2)]
        [NonnegativeValidator]
        public object LowValue { get; set; }

        [Dump(3)]
        [NonnegativeValidator]
        public object MaxLowValue { get; set; }

        [Dump(4)]
        public object HasIdentity { get; set; }
    }
}