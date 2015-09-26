using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Validation.Tests
{
    public static class Helper
    {
        public static void TestValidator<TAttr, T>(
            TestContext testContext,
            T value,
            bool isValid) where TAttr : ValidatorAttribute, new()
        {
            TestValidator(testContext, new TAttr(), value, isValid);
        }

        public static void TestValidator<T>(
            TestContext testContext,
            ValidatorAttribute attribute,
            T value,
            bool isValid)
        {
            TestValidator<T>(testContext, attribute, value, isValid, false);
            TestValidator<T>(testContext, attribute, value, !isValid, true);
        }

        static void TestValidator<T>(
            TestContext testContext,
            ValidatorAttribute attribute,
            T value,
            bool isValid,
            bool negated)
        {
            var results = new ValidationResults();

            try
            {
                var wrappedAttribute = new PrivateObject(attribute);

                if (negated)
                    wrappedAttribute.SetProperty("Negated", true);

                var validator = wrappedAttribute.Invoke("DoCreateValidator", typeof(T)) as Validator;

                Assert.IsNotNull(validator);

                var validatorAccessor = new PrivateObject(validator);

                validatorAccessor.Invoke(
                    "DoValidate",
                    value,
                    null,
                    null,
                    results);

                Assert.AreEqual(isValid, results.IsValid);
            }
            catch (UnitTestAssertException)
            {
                throw;
            }
            catch (Exception x)
            {
                testContext.WriteLine("{0}", x.DumpString());
                throw;
            }
            finally
            {
                testContext.WriteLine("{0}", results.DumpString());
            }
        }
    }
}
