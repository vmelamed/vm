using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace vm.Aspects
{
    /// <summary>
    /// Class LambdaExpressionExtensions.
    /// </summary>
    public static class LambdaExpressionExtensions
    {
        /// <summary>
        /// Gets the string name of a property out of a lambda expression like this: <c>(SomeType a) =&gt; a.Property</c>.
        /// </summary>
        /// <typeparam name="TPrincipal">The type of the principal.</typeparam>
        /// <typeparam name="TAssociated">The type of the associated.</typeparam>
        /// <param name="lambda">The lambda expression.</param>
        /// <returns>The name of the property as a string.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="lambda" /> is not a lambda expression with a body of a single property selection expression</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="lambda" /> is <see langword="null" />.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static string GetMemberName<TPrincipal, TAssociated>(
            this Expression<Func<TPrincipal, TAssociated>> lambda)
        {
            if (lambda == null)
                throw new ArgumentNullException(nameof(lambda));

            var memberExpression = lambda.Body as MemberExpression;

            if (memberExpression == null)
            {
                if (lambda.Body is UnaryExpression unaryExpression)
                    memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression == null ||
                !(memberExpression.Member is PropertyInfo  ||
                  memberExpression.Member is FieldInfo     ||
                  memberExpression.Member is EventInfo))
                throw new ArgumentException(@"The argument must be a lambda expression with a body of a single property or field or event selection expression, e.g.
    a => a.Name", nameof(lambda));

            return memberExpression.Member.Name;
        }
    }
}
