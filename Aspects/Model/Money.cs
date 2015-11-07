using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Class Money represents monetary value.
    /// </summary>
    [Serializable]
    public partial struct Money : ICloneable, IEquatable<Money>, IComparable<Money>, IComparable, IFormattable, ISerializable
    {
        /// <summary>
        /// The default currency is the good old US dollar.
        /// </summary>
        public const string DefaultCurrency = "USD";

        /// <summary>
        /// The default number of digits to the right of the decimal point.
        /// </summary>
        public const int DefaultDecimals = 3;

        #region Properties
        /// <summary>
        /// Gets the monetary value represented by the instance - the amount of currency.
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// Gets the ISO 4217 three letter currency code.
        /// </summary>
        /// <remarks>
        /// The value of this property should be a valid three letter currency code from ISO 4217.
        /// The class verifies only that the currency is 3 alpha characters long and does not check if the characters
        /// represent an actual currency - this is outside the scope of this class.
        /// </remarks>
        public string Currency { get; }

        /// <summary>
        /// Gets maximal number of digits to the right of the decimal point maintained by the class.
        /// </summary>
        public int Decimals { get; } 
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Money" /> class.
        /// </summary>
        /// <param name="value">The monetary value represented by the instance - the amount of currency.</param>
        /// <param name="currency">The ISO 4217 three letter currency code.</param>
        /// <param name="decimals">The maximal number of decimal digits maintained by the class.</param>
        public Money(
            decimal value,
            string currency = DefaultCurrency,
            int decimals = DefaultDecimals)
        {
            Contract.Requires<ArgumentException>(RegularExpression.CurrencyIsoCode.IsMatch(currency), "The argument " + nameof(currency) + " does not represent a valid currency code.");
            Contract.Requires<ArgumentException>(decimals >= 0, "The argument " + nameof(currency) + " cannot be negative.");

            Value = value;
            Currency = currency;
            Decimals = decimals;
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
            Decimals = (int)info.GetValue("Decimals", typeof(int));
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
        /// Note that the method does not test the <see cref="Decimals"/> properties because even if these properties are different but
        /// the <see cref="Value"/> and <see cref="Currency"/> are equal, the money is the same. In other words $1.32==$1.3200.
        /// </remarks>
        public bool Equals(
            Money other)
        {
            return Value    == other.Value &&
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
        /// Note that the method does not test the <see cref="Decimals"/> properties because even if these properties are different but
        /// the <see cref="Value"/> and <see cref="Currency"/> are equal, the money is the same. In other words $1.32==$1.3200.
        /// </remarks>
        public override bool Equals(object obj) => obj is Money ? Equals((Money)obj) : false;

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="Money"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Money"/> instance.</returns>
        /// <remarks>The property <see cref="Decimals"/> does not participate in calculating the hash value, because even if these properties are different but
        /// the <see cref="Value"/> and <see cref="Currency"/> are equal, the money is the same and their hash should be the same too. In other words $1.32==$1.3200.
        /// </remarks>
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
        public static bool operator == (Money left, Money right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Money"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(Money)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator != (Money left, Money right) => !(left == right);

        #region Public Methods
        /// <summary>
        /// Creates a new money objects, that has <see cref="Value"/> equal to the <see cref="Validation"/> of this instance but
        /// rounded to the specified number of digits after the decimal point applying the specified midpoint rounding method.
        /// The default rounding is the so called banking rounding, e.g. if we round to two decimals, then 1.535 is rounded to 1.54 and 1.525 is rounded to 1.52.
        /// </summary>
        /// <param name="decimals">The decimals.</param>
        /// <param name="midpointRounding">The midpoint rounding.</param>
        /// <returns>vm.Aspects.Model.Money.</returns>
        public Money Round(
                    int decimals = DefaultDecimals,
                    MidpointRounding midpointRounding = MidpointRounding.ToEven)
        {
            return new Money(
                        decimal.Round(Value, decimals, midpointRounding),
                        Currency,
                        decimals);
        }
        #endregion

        #region ICloneable
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A money instance identical to this.</returns>
        public object Clone()
        {
            return new Money(Value, Currency, Decimals);
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
            CompatibleCurrency(other);

            if (Value == other.Value)
                return 0;
            else
                if (Value > other.Value)
                    return 1;
                else
                    return -1;
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
            Contract.Requires<ArgumentException>(obj is Money, "The comparand must be of type Money.");

            return CompareTo((Money)obj);
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
            Contract.Requires<ArgumentNullException>(info != null, nameof(info));

            info.AddValue("Value", Value);
            info.AddValue("Currency", Currency);
            info.AddValue("Decimals", Decimals);
        }
        #endregion

        #region Operators
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
            CompatibleCurrencies(left, right);

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
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="InvalidOperationException">If the objects are of different currencies.</exception>
        public static bool operator >=(
            Money left,
            Money right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="InvalidOperationException">If the objects are of different currencies.</exception>
        public static bool operator <=(
            Money left,
            Money right)
        {
            return left.CompareTo(right) <= 0;
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
        public static Money operator +(
            Money operand)
        {
            return operand;
        }

        /// <summary>
        /// Implements the operation arithmetic negate represented by unary '-'.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// The operation returns a new instance with <see cref="Value"/> equal to the negated this' <see cref="Value"/>.
        /// </remarks>
        public static Money operator -(
            Money operand)
        {
            return new Money(
                        -operand.Value,
                        operand.Currency,
                        operand.Decimals);
        }

        /// <summary>
        /// Implements the operation addition represented by the binary '+' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will have <see cref="Value"/> equal to the sum of the <see cref="Value"/>-s of the operands; will have the same currency as the operands; and
        /// if the two operands have different <see cref="Decimals"/> properties, the result will have the bigger <see cref="Decimals"/>.
        /// </remarks>
        public static Money operator +(
            Money left,
            Money right)
        {
            CompatibleCurrencies(left, right);

            return new Money(
                        left.Value + right.Value,
                        left.Currency,
                        Math.Max(left.Decimals, right.Decimals));
        }

        /// <summary>
        /// Implements the operation subtraction represented by the binary '-' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will have <see cref="Value"/> equal to the difference between the <see cref="Value"/>-s of the operands; will have the same currency as the operands; and
        /// if the two operands have different <see cref="Decimals"/> properties, the result will have the bigger <see cref="Decimals"/>.
        /// </remarks>
        public static Money operator -(
            Money left,
            Money right)
        {
            CompatibleCurrencies(left, right);

            return new Money(
                        left.Value - right.Value,
                        left.Currency,
                        Math.Max(left.Decimals, right.Decimals));
        }

        /// <summary>
        /// Implements the operation division (ratio) represented by the '/' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will be equal to the division of the two moneys (will be the ratio of the two moneys).
        /// </remarks>
        public static decimal operator /(
            Money left,
            Money right)
        {
            Contract.Requires<DivideByZeroException>(right.Value != 0M, "The divisor is 0.");
            CompatibleCurrencies(left, right);

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
        /// The result object will be equal to the division of the value of the left operand by the right number. 
        /// The <see cref="Decimals"/> property of the result will reflect the true scale of the result. If you need to adjust the scale to different scale
        /// call the method <see cref="Round(int, MidpointRounding)"/> on the result. E.g.
        /// <code>
        /// <![CDATA[
        /// var money = new Money(1.543M);  // 1.543 USD
        /// var div = money / 1.333;        // 1.157539... USD
        /// var rounded = div.Round();      // 1.158 USD
        /// ]]>
        /// </code>
        /// </remarks>
        public static Money operator /(
            Money left,
            decimal right)
        {
            Contract.Requires<DivideByZeroException>(right != 0M, "The divisor is 0.");

            var div = left.Value / right;
            var divBits = (uint[])(object)decimal.GetBits(div);
            var decimals = checked((int)((divBits[3] >> 16) & 31));

            return new Money(
                        div,
                        left.Currency,
                        decimals);
        }

        /// <summary>
        /// Implements the operation money object modulo decimal number represented by the '%' operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        /// <exception cref="InvalidOperationException">If the operands are of different currencies.</exception>
        /// <remarks>
        /// The result object will be equal to the reminder of the division of the value of the left operand by the right number, rounded using 
        /// <see cref="MidpointRounding.ToEven"/> method, a.k.a. "banking rounding".
        /// Note that it can be negative or positive. This operation gives basically the "absolute error" of the '/' operator on the same operands;
        /// or the error from lost of precision.
        /// </remarks>
        public static Money operator %(
            Money left,
            decimal right)
        {
            Contract.Requires<DivideByZeroException>(right != 0M, "The divisor is 0.");

            var div = left.Value % right;
            var divBits = (uint[])(object)decimal.GetBits(div);
            var decimals = checked((int)((divBits[3] >> 16) & 31));

            return new Money(
                        div - decimal.Round(left.Value / right, left.Decimals, MidpointRounding.ToEven),
                        left.Currency,
                        decimals);
        } 
        #endregion

        [ContractAbbreviator]
        void CompatibleCurrency(
            Money other)
        {
            Contract.Requires<InvalidOperationException>(string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase), "The currencies are different.");
        }

        [ContractAbbreviator]
        static void CompatibleCurrencies(
            Money left,
            Money right)
        {
            Contract.Requires<InvalidOperationException>(string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase), "The currencies are different.");
        }
    }
}
