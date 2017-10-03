using System;
using System.Diagnostics.CodeAnalysis;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Enum ShouldDump specifies the possible options for whether to dump or not specific element.
    /// </summary>
    public enum ShouldDump
    {
        /// <summary>
        /// Follow the default setting for the element.
        /// </summary>
        Default,
        /// <summary>
        /// Dump the element.
        /// </summary>
        Dump,
        /// <summary>
        /// Do not dump the element.
        /// </summary>
        Skip,
    }

    /// <summary>
    /// The attribute properties control certain aspects of the objects' dump (including primitive ones) and their properties (if any), 
    /// including the items of sequences.  
    /// </summary>
    /// <remarks>
    /// Note that some properties of this attribute are applicable to classes, struct-s and properties (e.g. <see cref="DumpNullValues"/> or 
    /// <see cref="RecurseDump"/>), and others to properties only. An instance of a class or struct may be associated with two <c>DumpAttributes</c>: one 
    /// coming from the type or the meta data type of the instance (attribute applied on the class or the meta data class definition) and one from the 
    /// instance itself - e.g. from a property containing the instance or explicitly passed to the <see cref="ObjectTextDumper.Dump"/>. In these 
    /// cases not <c>null</c> class applicable properties from the instance attribute take precedence over the class attribute properties.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The display positional parameter is equal to Skip named parameter.")]
    [AttributeUsage(
        AttributeTargets.Struct    |
        AttributeTargets.Class     |
        AttributeTargets.Field     |
        AttributeTargets.Property  |
        AttributeTargets.Parameter |
        AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    public sealed class DumpAttribute : Attribute, IEquatable<DumpAttribute>, ICloneable
    {
        #region Constant instances.
        /// <summary>
        /// The default dump attribute applied to instances and properties with no <c>DumpAttribute</c> specified.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Justification = "We want it visible so the users can redefine its default property values.")]
        public static DumpAttribute Default { get; } = new DumpAttribute();
        #endregion

        string _maskValue;
        string _labelFormat;
        string _valueFormat;

        #region Default values for some properties
        /// <summary>
        /// The default maximum depth of aggregated objects dumping: 10. The depth is counted from the outermost instance.
        /// </summary>
        public const int DefaultMaxDepth = 10;

        /// <summary>
        /// The default maximum number of arrays' and collections' elements to be dumped: 10.
        /// </summary>
        public const int DefaultMaxElements = 10;

        /// <summary>
        /// The default dump order for properties: <c>int.MaxValue</c>, i.e. dump properties with unspecified order in alphabetical order before the 
        /// properties with negative order.
        /// </summary>
        public const int DefaultOrder = int.MaxValue;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DumpAttribute" /> class with default values.
        /// </summary>
        public DumpAttribute()
        {
            Order       = DefaultOrder;
            MaxDepth    = DefaultMaxDepth;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpAttribute" /> class with explicit dump order.
        /// </summary>
        /// <param name="order">
        /// The dump order.
        /// </param>
        public DumpAttribute(int order)
        {
            Order       = order;
            MaxDepth    = DefaultMaxDepth;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpAttribute" /> class and specifies whether to dump the class or property or not.
        /// </summary>
        /// <param name="display">
        /// If set to <c>true</c> dumps the class or property; otherwise does not dump it.
        /// </param>
        public DumpAttribute(bool display)
        {
            Skip        = display ? ShouldDump.Dump : ShouldDump.Skip;
            Order       = DefaultOrder;
            MaxDepth    = DefaultMaxDepth;
        }
        #endregion

        #region Properties applicable to classes, structs and properties.
        /// <summary>
        /// Gets or sets a value indicating whether to dump or skip properties with <c>null</c> value.
        /// </summary>
        /// <remarks>
        /// Applies to classes, struct-s and properties.
        /// </remarks>
        public ShouldDump DumpNullValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the element should be recursively dumped.
        /// Default value is <c>RecurseDump.Dump</c> - recurse into.
        /// <para>
        /// When applied to a class with value of <c>RecurseDump.Skip</c> it will suppress dumping 
        /// the class's properties with exception of the property specified by the property 
        /// <see cref="DefaultProperty"/> of this attribute.
        /// </para><para>
        /// When applied to a property of non-sequence type with a value of <c>RecurseDump.Skip</c>
        /// it will suppress dumping of the associated instance.
        /// </para><para>
        /// When applied to a property of sequence type the attribute will suppress dumping the sequence's items.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Applicable to classes, struct-s and properties of class, struct or sequence types.
        /// </remarks>
        public ShouldDump RecurseDump { get; set; }

        /// <summary>
        /// Gets or sets the name of the only property (the representative property) which should be dumped
        /// if <see cref="RecurseDump"/> is set to <c>RecurseDump.Skip</c>.
        /// </summary>
        /// <remarks>
        /// Applicable to classes and struct-s only.
        /// </remarks>
        public string DefaultProperty { get; set; }

        /// <summary>
        /// This property is applicable only to the outermost class or struct and gets or sets the maximum depth of nested instances to be dumped. 
        /// The default maximum depth is 10.
        /// </summary>
        /// <remarks>
        /// Applicable to classes and struct-s at the top level of dumping recursion only.
        /// </remarks>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Gets or sets a flag whether to enumerate the elements in the object, if it implements IEnumerable.
        /// The default for arrays and sequences of the FCL is <see cref="ShouldDump.Dump"/> and for custom classes - <see cref="ShouldDump.Skip"/>.
        /// <seealso cref="DefaultMaxElements"/>.
        /// </summary>
        public ShouldDump Enumerate { get; set; }
        #endregion

        #region Properties applicable to properties only
        /// <summary>
        /// Specifies the dump order of a property. The properties are dumped in the following order: first are dumped properties with non-negative 
        /// <c>Order</c> in ascending order (0, 1, 2, etc.), then are dumped properties with unspecified <c>Order</c> (because their default order is 
        /// int.MaxValue); then are dumped the base class properties and in the end are dumped the properties with negative order in descending order (-1, 
        /// -2, etc.). Properties with equal <c>Order</c>-s are dumped in alphabetical order.
        /// </summary>
        /// <remarks>
        /// Applicable to properties only.
        /// </remarks>
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property to which this attribute is applied to should be skipped in the dump. 
        /// The default for public elements is <c>ShouldDump.Dump</c> and for protected or private <c>ShouldDump.Skip</c>.
        /// </summary>
        /// <remarks>
        /// Applicable to properties only.
        /// </remarks>
        public ShouldDump Skip { get; set; }

        /// <summary>
        /// Gets or sets a flag whether the actual value should be masked. Use it for properties whose values should not be logged, e.g. passwords 
        /// and PII fields (SSN, DL#, etc.)
        /// </summary>
        /// <remarks>
        /// Applicable to properties only.
        /// </remarks>
        public bool Mask { get; set; }

        /// <summary>
        /// Gets or sets the string value which should be output in the dump instead of the actual value. Use it for properties whose values should 
        /// not be logged, e.g. passwords and PII fields (SSN, DL#, etc.) The default is &quot;******&quot;.
        /// </summary>
        /// <remarks>
        /// Applicable to properties only.
        /// </remarks>
        public string MaskValue
        {
            get
            {
                return _maskValue ?? Resources.MaskInLogs;
            }
            set
            {
                _maskValue = value;
            }
        }

        /// <summary>
        /// If the property is of string type, gets or sets the maximum number of characters to be dumped from the value.
        /// Non-positive numbers (including 0 - the default) will dump the entire string no matter how long the string might be. 
        /// <para>
        /// For arrays the default value of 0 means to dump no-more than the first <seealso cref="DefaultMaxElements"/> (ten) elements.
        /// A negative value (e.g. -1) will dump all elements and positive value will dump no more than the first <c>MaxLength</c> elements.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Applicable to properties of string or sequence types only.
        /// </remarks>
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the format of the label of the properties. The default is: &quot;{0,-24} = &quot; 
        /// where the placeholder 0 will be replaced by the name of the property.
        /// </summary>
        /// <remarks>
        /// Applicable to properties only.
        /// </remarks>
        public string LabelFormat
        {
            get { return _labelFormat ?? DumpFormat.DefaultPropertyLabel; }
            set { _labelFormat = value; }
        }

        /// <summary>
        /// Applies mostly to properties of basic types (primitives, enum, string, Guid, DateTime, DateTimeOffset, TimeSpan, Uri.)
        /// Gets or sets the format string that should be applied to the value for dumping. The default is &quot;{0}&quot;.
        /// For complex types the <see cref="ObjectTextDumper"/> recognizes special value for this property - &quot;ToString()&quot; in this case the returned value  
        /// of the property's method <see cref="Object.ToString"/> is inserted in the underlying text writer.
        /// </summary>
        public string ValueFormat
        {
            get { return _valueFormat ?? DumpFormat.Value; }
            set { _valueFormat = value; }
        }

        /// <summary>
        /// Gets or sets a dump class which has a method that implements custom formatting of the property's value. 
        /// The <see cref="ObjectTextDumper"/> searches for a method specified by the attribute property <see cref="DumpMethod"/> 
        /// in the class specified by this property and invokes it in order to output the text 
        /// representation of the property to which it is applied. If <see cref="DumpMethod"/> is not specified
        /// the <see cref="ObjectTextDumper"/> assumes that the name of the dump method is &quot;Dump&quot;.
        /// </summary>
        /// <seealso cref="DumpMethod"/>
        /// <remarks>
        /// Applicable to properties only.
        /// </remarks>
        public Type DumpClass { get; set; }

        /// <summary>
        /// Gets or sets the name of the dump method in the class specified by <see cref="DumpClass"/>. The dump method implements custom formatting of the property's value. 
        /// The method must be static, public, have a return type of <see cref="String"/> and must take a single parameter of type or a base type of the property. 
        /// If the <see cref="DumpClass"/> is not specified then the <see cref="ObjectTextDumper"/> will look for a parameterless instance method by the same name in the
        /// property's class or a static method with parameter the type or a base type of the property in the property's class, base class or the metadata class.
        /// </summary>
        /// <remarks>
        /// Applicable to properties only.
        /// </remarks>
        public string DumpMethod { get; set; }
        #endregion

        /// <summary>
        /// When overridden in a derived class, indicates whether the value of this instance is the default value for the derived class.
        /// </summary>
        /// <returns><see langword="true"/> if this instance is the default attribute for the class; otherwise, <see langword="false"/>.</returns>
        public override bool IsDefaultAttribute()
            => this == Default;

        #region ICloneable Members
        object ICloneable.Clone() => Clone();
        #endregion

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>DumpAttribute.</returns>
        public DumpAttribute Clone()
        {
            return new DumpAttribute
            {
                Order           = Order,
                DumpNullValues  = DumpNullValues,
                Skip            = Skip,
                RecurseDump     = RecurseDump,
                DefaultProperty = DefaultProperty,
                Mask            = Mask,
                MaskValue       = MaskValue,
                MaxLength       = MaxLength,
                MaxDepth        = MaxDepth,
                LabelFormat     = LabelFormat,
                ValueFormat     = ValueFormat,
            };
        }

        #region Identity rules implementation.
        // Two attributes are equal if their properties are equal by value.

        #region IEquatable<DumpAttribute> Members
        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="DumpAttribute"/> to compare with this object.</param>
        /// <returns>
        /// <c>false</c> if <paramref name="other"/> is equal to <c>null</c>, otherwise
        /// <c>true</c> if <paramref name="other"/> refers to <c>this</c> object, otherwise
        /// <c>true</c> if all the properties of the current object and the <paramref name="other"/> are equal by value.
        /// </returns>
        public bool Equals(DumpAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(other, null))
                return false;

            return Order           == other.Order            &&
                   DumpNullValues  == other.DumpNullValues   &&
                   Skip            == other.Skip             &&
                   RecurseDump     == other.RecurseDump      &&
                   DefaultProperty == other.DefaultProperty  &&
                   Mask            == other.Mask             &&
                   MaskValue       == other.MaskValue        &&
                   MaxLength       == other.MaxLength        &&
                   MaxDepth        == other.MaxDepth         &&
                   LabelFormat     == other.LabelFormat      &&
                   ValueFormat     == other.ValueFormat;
        }
        #endregion

        /// <summary>
        /// Determines whether this <see cref="DumpAttribute"/> instance is equal to the specified <see cref="System.Object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> reference to compare with this <see cref="DumpAttribute"/> object.</param>
        /// <returns>
        /// <c>false</c> if <paramref name="obj"/> is equal to <c>null</c>, otherwise
        /// <c>true</c> if <paramref name="obj"/> refers to <c>this</c> object, otherwise
        /// <c>true</c> if <paramref name="obj"/> <i>is an instance of</i> <see cref="DumpAttribute"/> and 
        /// properties of the current object and the <paramref name="obj"/> are equal by value; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => Equals(obj as DumpAttribute);

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="DumpAttribute"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DumpAttribute"/> instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            hashCode = Constants.HashMultiplier * hashCode + Order.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + DumpNullValues.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + Skip.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + RecurseDump.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + (DefaultProperty?.GetHashCode() ?? 0);
            hashCode = Constants.HashMultiplier * hashCode + Mask.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + MaskValue.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + MaxLength.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + MaxDepth.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + LabelFormat.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + ValueFormat.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="DumpAttribute"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the objects are considered to be equal (<see cref="M:IEquatable.Equals{DumpAttribute}"/>);
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool operator ==(DumpAttribute left, DumpAttribute right) => ReferenceEquals(left, null)
                                                                                    ? ReferenceEquals(right, null)
                                                                                    : left.Equals(right);

        /// <summary>
        /// Compares two <see cref="DumpAttribute"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the objects are not considered to be equal (<see cref="M:IEquatable.Equals{DumpAttribute}"/>);
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool operator !=(DumpAttribute left, DumpAttribute right) => !(left==right);
        #endregion
    }
}
