using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Model
{
    abstract class MoneyMetadata
    {
        [Dump(0)]
        public object Value { get; set; }

        [IgnoreNulls]
        [RegexValidator(RegularExpression.RexCurrencyIsoCode)]
        [Dump(1)]
        public object Currency { get; set; }
    }
}