using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using vm.Aspects.Diagnostics;
using vm.Aspects.Validation;

namespace vm.Aspects.Model
{
    abstract class MoneyMetadata
    {
        [Dump(0)]
        public object Value { get; set; }

        [Dump(1)]
        [IgnoreNulls]
        [RegexValidator(RegularExpression.RexCurrencyIsoCode)]
        public object Currency { get; set; }
    }
}