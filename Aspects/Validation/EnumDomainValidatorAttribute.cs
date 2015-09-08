using System;
using System.Diagnostics.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class EnumDomainValidatorAttribute. Creates a validator which tests if the enum value (or nullable enum) to which it is applied 
    /// is from the domain of possible values for this enum or possibly <see langword="null"/>. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Method |
        AttributeTargets.Parameter,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class EnumDomainValidatorAttribute : ValueValidatorAttribute
    {
        /// <summary>
        /// Creates the <see cref="T:Microsoft.Practices.EnterpriseLibrary.Validation.Validator" /> described by the attribute object 
        /// providing validator specific information.
        /// </summary>
        /// <param name="targetType">The type of object that will be validated by the validator.</param>
        /// <returns>The created <see cref="T:Microsoft.Practices.EnterpriseLibrary.Validation.Validator" />.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="targetType"/> is not enum or nullable enum.</exception>
        protected override Validator DoCreateValidator(
            Type targetType)
        {
            Contract.Ensures(Contract.Result<Validator>() != null);

            if (targetType == null)
                throw new ArgumentNullException("targetType");

            if (!targetType.IsEnum  &&  !(targetType.IsGenericType  &&
                                          targetType.GetGenericTypeDefinition()==typeof(Nullable<>)  &&
                                          targetType.GetGenericArguments()[0].IsEnum))
                throw new ArgumentException("This validator can be applied to enum or nullable enum types only.", "targetType");

            // create the validator with reflection
            return (Validator)typeof(EnumDomainValidator<>)
                                .MakeGenericType(targetType)
                                .GetConstructor(new Type[] { typeof(string), typeof(string), typeof(bool) })
                                .Invoke(new object[] { GetMessageTemplate(), Tag, Negated });
        }
    }
}
