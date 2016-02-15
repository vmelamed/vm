using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Class Money represents monetary value.
    /// </summary>
    [Serializable]
    public sealed class Money : ICloneable, IEquatable<Money>, IComparable<Money>, IComparable, IFormattable, ISerializable
    {
        #region Properties
        /// <summary>
        /// Gets the monetary value represented by the instance - the amount of currency.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets the ISO 4217 three letter currency code.
        /// </summary>
        /// <remarks>
        /// The value of this property should be a valid three letter currency code from ISO 4217.
        /// The class verifies only that the currency is 3 alpha characters long and does not check if the characters
        /// represent an actual currency - this is outside the scope of this class.
        /// </remarks>
        public string Currency { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Money" /> class.
        /// </summary>
        /// <param name="value">The monetary value represented by the instance - the amount of currency.</param>
        /// <param name="currency">The ISO 4217 three letter currency code.</param>
        public Money(
            decimal value,
            string currency = null)
        {
            Contract.Requires<ArgumentException>(string.IsNullOrWhiteSpace(currency) || RegularExpression.CurrencyIsoCode.IsMatch(currency), "The argument " + nameof(currency) + " does not represent a valid currency code.");

            IMoneyDefaults defaults = ServiceLocator.Current.GetInstance<IMoneyDefaults>();

            if (string.IsNullOrWhiteSpace(currency))
                currency = defaults.Currency;

            Value = decimal.Round(value, defaults.Decimals(currency), defaults.Rounding(currency));
            Currency = currency;
        }

        /// <summary>
        /// Deserializes a new instance of the <see cref="Money"/> struct.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The streaming context.</param>
        private Money(
            SerializationInfo info,
            StreamingContext context)
        {
            Contract.Requires<ArgumentNullException>(info != null, nameof(info));

            Value = (decimal)info.GetValue("Value", typeof(decimal));
            Currency = (string)info.GetValue("Currency", typeof(string));
        }
        #endregion

        #region IEquatable<Money> Members
        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="Money"/> to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object and the <paramref name="other"/> have equal values 
        /// of their properties <see cref="Value"/> and <see cref="Currency"/>; otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(Money)"/>, <see cref="Equals(object)"/> methods, the overloaded <c>operator==</c> and 
        /// <c>operator!=</c> test for value identity.
        /// </remarks>
        public bool Equals(
            Money other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Value == other.Value &&
                   Currency == other.Currency;
        }
        #endregion

        /// <summary>
        /// Determines whether this <see cref="Money"/> instance is equal to the specified <see cref="object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> reference to compare with this <see cref="Money"/> object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object and the <paramref name="obj"/> have equal values 
        /// of their properties <see cref="Value"/> and <see cref="Currency"/>; otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(Money)"/>, <see cref="Equals(object)"/> methods, the overloaded <c>operator==</c> and 
        /// <c>operator!=</c> test for value identity.
        /// </remarks>
        public override bool Equals(object obj) => Equals(obj as Money);

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="Money"/> and its derived types.
        /// </summary>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            hashCode = Constants.HashMultiplier * hashCode + Currency.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + Value.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="Money"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are considered to be equal (<see cref="Equals(Money)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Money left, Money right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="Money"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(Money)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Money left, Money right) => !(left == right);

        #region ICloneable
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A money instance identical to this.</returns>
        public object Clone()
        {
            return new Money(Value, Currency);
        }
        #endregion

        #region ICompareable<Money>
        /// <summary>
        /// Compares the current instance to a another instance.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>
        /// 0 if the two instances are equal, negative value if this instance is less than <paramref name="other"/> and
        /// positive value if this instance is greater than <paramref name="other"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="other"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="other"/> is of different currency than this instance.</exception>
        public int CompareTo(
            Money other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The currencies are different.", nameof(other));

            if (Value == other.Value)
                return 0;
            else
            {
                if (Value > other.Value)
                    return 1;
                else
                    return -1;
            }
        }
        #endregion

        #region ICompareable
        /// <summary>
        /// Compares the current instance to an object.
        /// </summary>
        /// <param name="obj">The instance to compare to.</param>
        /// <returns>
        /// 0 if the two instances are equal, negative value if this instance is less than <paramref name="obj"/> and
        /// positive value if this instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="obj"/> is not of type <see cref="Money"/> or is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="obj"/> is of different currency than this instance.</exception>
        public int CompareTo(
            object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Money money = obj as Money;

            if (money == null)
                throw new ArgumentException("The comparand must be of type Money.", nameof(obj));

            return CompareTo(money);
        }
        #endregion

        #region IFormattable
        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="format">The format string to use.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>System.String.</returns>
        public string ToString(
            string format,
            IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ISerializable
        /// <summary>
        /// GetsSerializes the structure's data.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The streaming context context.</param>
        public void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
        {
            info.AddValue("Value", Value);
            info.AddValue("Currency", Currency);
        }
        #endregion

        #region Operations
        /// <summary>
        /// Implements the operation unary plus.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="operand"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The operation returns a new instance identical to this.
        /// </remarks>
        public static Money Plus(
            Money operand)
        {
            Contract.Requires<ArgumentNullException>(operand != null, nameof(operand));
            Contract.Ensures(Contract.Result<Money>() != null);

            return new Money(
                        operand.Value,
                        operand.Currency);
        }

        /// <summary>
        /// Implements the operation arithmetic negate represented by unary '-'.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// The operation returns a new instance with <see cref="Value"/> equal to the negated this' <see cref="Value"/> and rounded.
        /// </remarks>
        public static Money Negate(
            Money operand)
        {
            Contract.Requires<ArgumentNullException>(operand != null, nameof(operand));
            Contract.Ensures(Contract.Result<Money>() != null);

            return new Money(
                        -operand.Value,
                        operand.Currency);
        }

        /// <summary>
        /// Implements the operation addition.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will have <see cref="Value"/> equal to the sum of the <see cref="Value"/>-s of the operands; 
        /// will have the same currency as the operands; 
        /// the result value will be rounded.
        /// </remarks>
        public static Money Add(
            Money left,
            Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));
            Contract.Requires<ArgumentException>(string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase), "The currencies are different.");
            Contract.Ensures(Contract.Result<Money>() != null);

            return new Money(
                        left.Value + right.Value,
                        left.Currency);
        }

        /// <summary>
        /// Implements the operation subtraction.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will have <see cref="Value"/> equal to the difference between the <see cref="Value"/>-s of the operands; the result value will be rounded.
        /// </remarks>
        public static Money Subtract(
            Money left,
            Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));
            Contract.Requires<ArgumentException>(string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase), "The currencies are different.");
            Contract.Ensures(Contract.Result<Money>() != null);

            return new Money(
                        left.Value - right.Value,
                        left.Currency);
        }

        /// <summary>
        /// Implements the operation division (ratio) represented by the '/' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will be equal to the division of the two moneys (i.e. will be the ratio of the two moneys).
        /// </remarks>
        public static decimal Divide(
            Money left,
            Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));
            Contract.Requires<DivideByZeroException>(right.Value != 0M, "The divisor is 0.");
            Contract.Requires<ArgumentException>(string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase), "The currencies are different.");

            return left.Value / right.Value;
        }

        /// <summary>
        /// Implements the operation division of money object by decimal number represented by the '/' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will be equal to the division of the value of the left operand by the right number; the result value will be rounded. 
        /// </remarks>
        public static Money Divide(
            Money left,
            decimal right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<DivideByZeroException>(right != 0M, "The divisor is 0.");
            Contract.Ensures(Contract.Result<Money>() != null);

            return new Money(
                        left.Value / right,
                        left.Currency);
        }

        /// <summary>
        /// Implements the operation money object modulo decimal number represented by the '%' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will be equal to the reminder of the division of the value of the left operand by the right number, the result value will be rounded.
        /// </remarks>
        public static Money Mod(
            Money left,
            decimal right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<DivideByZeroException>(right != 0M, "The divisor is 0.");
            Contract.Ensures(Contract.Result<Money>() != null);

            return new Money(
                        left.Value % right,
                        left.Currency);
        }
        #endregion

        #region Overloaded operators
        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="InvalidOperationException">If the objects are of different currencies.</exception>
        public static bool operator >(
            Money left,
            Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));

            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="InvalidOperationException">If the objects are of different currencies.</exception>
        public static bool operator <(
            Money left,
            Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));

            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="InvalidOperationException">If the objects are of different currencies.</exception>
        public static bool operator >=(Money left, Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));

            return !(left < right);
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="InvalidOperationException">If the objects are of different currencies.</exception>
        public static bool operator <=(Money left, Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));

            return !(left > right);
        }

        /// <summary>
        /// Implements the operation unary '+'.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="operand"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The operation returns a new instance identical to this.
        /// </remarks>
        public static Money operator +(Money operand)
        {
            Contract.Requires<ArgumentNullException>(operand != null, nameof(operand));
            Contract.Ensures(Contract.Result<Money>() != null);

            return Plus(operand);
        }

        /// <summary>
        /// Implements the operation arithmetic negate represented by unary '-'.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// The operation returns a new instance with <see cref="Value"/> equal to the negated this' <see cref="Value"/> and rounded.
        /// </remarks>
        public static Money operator -(Money operand)
        {
            Contract.Requires<ArgumentNullException>(operand != null, nameof(operand));
            Contract.Ensures(Contract.Result<Money>() != null);

            return Negate(operand);
        }

        /// <summary>
        /// Implements the operation addition represented by the binary '+' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will have <see cref="Value"/> equal to the sum of the <see cref="Value"/>-s of the operands; 
        /// will have the same currency as the operands; 
        /// the result value will be rounded.
        /// </remarks>
        public static Money operator +(Money left, Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));
            Contract.Requires<ArgumentException>(string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase), "The currencies are different.");
            Contract.Ensures(Contract.Result<Money>() != null);

            return Add(left, right);
        }

        /// <summary>
        /// Implements the operation subtraction represented by the binary '-' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will have <see cref="Value"/> equal to the difference between the <see cref="Value"/>-s of the operands; the result value will be rounded.
        /// </remarks>
        public static Money operator -(Money left, Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));
            Contract.Requires<ArgumentException>(string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase), "The currencies are different.");
            Contract.Ensures(Contract.Result<Money>() != null);

            return Subtract(left, right);
        }

        /// <summary>
        /// Implements the operation division (ratio) represented by the '/' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will be equal to the division of the two moneys (i.e. will be the ratio of the two moneys).
        /// </remarks>
        public static decimal operator /(Money left, Money right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));
            Contract.Requires<DivideByZeroException>(right.Value != 0M, "The divisor is 0.");
            Contract.Requires<ArgumentException>(string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase), "The currencies are different.");

            return Divide(left, right);
        }

        /// <summary>
        /// Implements the operation division of money object by decimal number represented by the '/' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will be equal to the division of the value of the left operand by the right number; the result value will be rounded. 
        /// </remarks>
        public static Money operator /(Money left, decimal right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<DivideByZeroException>(right != 0M, "The divisor is 0.");
            Contract.Ensures(Contract.Result<Money>() != null);

            return Divide(left, right);
        }

        /// <summary>
        /// Implements the operation money object modulo decimal number represented by the '%' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will be equal to the reminder of the division of the value of the left operand by the right number, the result value will be rounded.
        /// </remarks>
        public static Money operator %(Money left, decimal right)
        {
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<DivideByZeroException>(right != 0M, "The divisor is 0.");
            Contract.Ensures(Contract.Result<Money>() != null);

            return Mod(left, right);
        }
        #endregion
    }
}
