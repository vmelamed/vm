using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Class DumpFormat exposes most of the format strings used by the dumper so that they can be changed programmaticly.
    /// </summary>
    public static class DumpFormat
    {
        static string _cyclicalReference;
        static string _delegate;
        static string _enum;
        static string _genericParam;
        static string _indexerIndexType;
        static string _memberInfoMemberType;
        static string _methodInfo;
        static string _methodParameter;
        static string _defaultPropertyLabel;
        static string _sequenceDumpTruncated;
        static string _sequenceType;
        static string _sequenceTypeName;
        static string _type;
        static string _typeInfo;
        static string _value;

        static DumpFormat()
        {
            Reset();
        }

        /// <summary>
        /// Resets the properties to their default values.
        /// </summary>
        public static void Reset()
        {
            _cyclicalReference     = Resources.CyclicalReferenceDesignator;
            _delegate              = Resources.DelegateDumpFormat;
            _enum                  = Resources.EnumDumpFormat;
            _genericParam          = Resources.GenericParamFormat;
            _indexerIndexType      = Resources.IndexerIndexDumpFormat;
            _memberInfoMemberType  = Resources.MemberInfoMemberTypeDumpFormat;
            _methodInfo            = Resources.MethodInfoDumpFormat;
            _methodParameter       = Resources.MethodParameterFormat;
            _defaultPropertyLabel  = Resources.PropertyLabelFormat;
            _sequenceDumpTruncated = Resources.SequenceDumpTruncatedFormat;
            _sequenceType          = Resources.SequenceTypeFormat;
            _sequenceTypeName      = Resources.SequenceTypeNameFormat;
            _type                  = Resources.TypeDumpFormat;
            _typeInfo              = Resources.TypeInfoFormat;
            _value                 = Resources.ValueFormat;
        }

        /// <summary>
        /// Gets or sets the format of the string which designates a cyclical reference - an object that has already been duped.
        /// Parameters: 0 - type name, 1 - type namespace and 2 - assembly qualified name of the type.
        /// Default: &quot;{0} (see above)&quot;
        /// </summary>
        public static string CyclicalReference
        {
            get { return _cyclicalReference; }
            set { _cyclicalReference = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for delegate objects.
        /// Parameters: 0 - declaring type name, 1 - declaring type namespace, 2 - assembly qualified name of the declaring type, 3 - the name of the method and 4 - access modifier (e.g. 'static ').
        /// Default: &quot;{4}{0}.{3}&quot;
        /// </summary>
        public static string Delegate
        {
            get { return _delegate; }
            set { _delegate = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for enum values.
        /// Parameters: 0 - declaring type name, 1 - declaring type namespace, 2 - assembly qualified name of the declaring type and 3 - the value.
        /// Default: &quot;{0}.{3}&quot;
        /// </summary>
        public static string Enum
        {
            get { return _enum; }
            set { _enum = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for generic parameters.
        /// Parameters: 0 - declaring type name, 1 - declaring type namespace, 2 - assembly qualified name of the declaring type
        /// Default: &quot;{0}&quot;
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param", Justification="Too late to fix - interface is published")]
        public static string GenericParam
        {
            get { return _genericParam; }
            set { _genericParam = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for the type of the indexers' indexes.
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type
        /// Default: &quot;{0}&quot;
        /// </summary>
        public static string IndexerIndexType
        {
            get { return _indexerIndexType; }
            set { _indexerIndexType = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for objects of type <seealso cref="System.Reflection.MemberInfo"/>.
        /// Parameters: 0 - value
        /// Default: &quot;{0}&quot;
        /// </summary>
        public static string MemberInfoMemberType
        {
            get { return _memberInfoMemberType; }
            set { _memberInfoMemberType = value; }
        }

        /// <summary>
        /// Gets or sets the dump format <see cref="MethodInfo"/>.
        /// Parameters: 0 - return type name, 1 - return type namespace, 2 - assembly qualified name of the return type, 
        /// 3 - declaring type name, 4 - declaring type namespace, 5 - assembly qualified name of the declaring type, 6 - method name.
        /// Default: &quot;{0} {3}.{6}&quot;
        /// </summary>
        public static string MethodInfo
        {
            get { return _methodInfo; }
            set { _methodInfo = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for method parameters.
        /// Parameters: 0 - parameter type name, 1 - parameter type namespace, 2 - assembly qualified name of the parameter type, 3 - parameter name.
        /// Default: &quot;{0} {3}&quot;
        /// </summary>
        public static string MethodParameter
        {
            get { return _methodParameter; }
            set { _methodParameter = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for a property dump label.
        /// Parameters: 0 - property name.
        /// Default: &quot;{0,-24} = &quot;
        /// </summary>
        public static string DefaultPropertyLabel
        {
            get { return _defaultPropertyLabel; }
            set { _defaultPropertyLabel = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for the string designating that a sequence dump has been truncated.
        /// Parameters: 0 - the maximum number of dumped sequence elements.
        /// Default: &quot;... dumped the first {0} elements.&quot;
        /// </summary>
        public static string SequenceDumpTruncated
        {
            get { return _sequenceDumpTruncated; }
            set { _sequenceDumpTruncated = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for the type of a sequence.
        /// Parameters: 0 - sequence type name, 1 - number of elements in the sequence.
        /// Default: &quot;{0}[{1}]: &quot;
        /// </summary>
        public static string SequenceTypeName
        {
            get { return _sequenceTypeName; }
            set { _sequenceTypeName = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for the type of a sequence. Note that by the time this format is used, the sequence type name, 
        /// its generic parameters and dimension are already printed using <see cref="SequenceTypeName"/>.
        /// Parameters: 0 - sequence type name, 1 - sequence type namespace, 2 - assembly qualified name of the sequence type, 
        /// Default: &quot;({2})&quot;
        /// </summary>
        public static string SequenceType
        {
            get { return _sequenceType; }
            set { _sequenceType = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for a type.
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type, 
        /// Default: &quot;{0} ({2}): &quot;
        /// </summary>
        public static string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for dumped values of type <see cref="Type"/>.
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type, 
        /// Default: &quot;{2}&quot;
        /// </summary>
        public static string TypeInfo
        {
            get { return _typeInfo; }
            set { _typeInfo = value; }
        }

        /// <summary>
        /// Gets or sets the dump format for a value.
        /// Parameters: 0 - the value to be dumped.
        /// Default: &quot;{0}&quot;
        /// </summary>
        public static string Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}
