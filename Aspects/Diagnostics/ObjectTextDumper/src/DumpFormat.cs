using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Class DumpFormat exposes most of the format strings used by the dumper so that they can be changed programmatically.
    /// </summary>
    public static class DumpFormat
    {
        static DumpFormat()
        {
            Reset();
        }

        /// <summary>
        /// Resets the properties to their default values.
        /// </summary>
        public static void Reset()
        {
            CyclicalReference     = Resources.CyclicalReferenceDesignator;
            Delegate              = Resources.DelegateDumpFormat;
            Enum                  = Resources.EnumDumpFormat;
            EnumFlag              = Resources.EnumFlagDumpFormat;
            EnumFlagPrefix        = Resources.EnumFlagsDumpBeginFormat;
            EnumFlagSeparator     = Resources.EnumFlagsSeparator;
            EnumFlagSuffix        = Resources.EnumFlagsDumpEndFormat;
            GenericParam          = Resources.GenericParamFormat;
            IndexerIndexType      = Resources.IndexerIndexDumpFormat;
            MemberInfoMemberType  = Resources.MemberInfoMemberTypeDumpFormat;
            MethodInfo            = Resources.MethodInfoDumpFormat;
            MethodParameter       = Resources.MethodParameterFormat;
            DefaultPropertyLabel  = Resources.PropertyLabelFormat;
            SequenceDumpTruncated = Resources.SequenceDumpTruncatedFormat;
            SequenceType          = Resources.SequenceTypeFormat;
            SequenceTypeName      = Resources.SequenceTypeNameFormat;
            Type                  = Resources.TypeDumpFormat;
            TypeInfo              = Resources.TypeInfoFormat;
            Value                 = Resources.ValueFormat;
            CSharpDumpLabel       = Resources.CSharpDumpLabelFormat;
        }

        /// <summary>
        /// Gets or sets the format of the string which designates a cyclical reference - an object that has already been duped.
        /// Parameters: 0 - type name, 1 - reference to the dumped object
        /// Default: &quot;{0} (see {1} above)&quot;
        /// </summary>
        public static string CyclicalReference { get; set; } = Resources.CyclicalReferenceDesignator;

        /// <summary>
        /// Gets or sets the dump format for delegate objects.
        /// Parameters: 0 - declaring type name, 1 - declaring type namespace, 2 - assembly qualified name of the declaring type, 3 - the name of the method and 4 - access modifier (e.g. 'static ').
        /// Default: &quot;{4}{0}.{3}&quot;
        /// </summary>
        public static string Delegate { get; set; } = Resources.DelegateDumpFormat;

        /// <summary>
        /// Gets or sets the dump format for enum values.
        /// Parameters: 0 - declaring type name, 1 - declaring type namespace, 2 - assembly qualified name of the declaring type and 3 - the value.
        /// Default: &quot;{0}.{3}&quot;
        /// </summary>
        public static string Enum { get; set; } = Resources.EnumDumpFormat;

        /// <summary>
        /// Gets or sets the dump format for enum flag values.
        /// Parameters: 0 - declaring type name, 1 - declaring type namespace, 2 - assembly qualified name of the declaring type and 3 - the value.
        /// Default: &quot;{3}&quot;
        /// </summary>
        public static string EnumFlag { get; set; } = Resources.EnumFlagDumpFormat;

        /// <summary>
        /// Gets or sets the dump format to prefix the list of enum flags (<see cref="Enum"/>) separated by <see cref="EnumFlagSeparator"/>.
        /// </summary>
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type,
        /// Default: &quot;{0}: (&quot;
        public static string EnumFlagPrefix { get; set; } = Resources.EnumFlagsDumpBeginFormat;

        /// <summary>
        /// Gets or sets the dump format to prefix the list of enum flags (<see cref="Enum"/>) separated by &quot; | &quot;.
        /// </summary>
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type,
        /// Default: &quot; | &quot;
        public static string EnumFlagSeparator { get; set; } = Resources.EnumFlagsSeparator;

        /// <summary>
        /// Gets or sets the dump format to prefix the list of enum flags
        /// </summary>
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type,
        /// Default: &quot;)&quot;
        public static string EnumFlagSuffix { get; set; } = Resources.EnumFlagsDumpEndFormat;

        /// <summary>
        /// Gets or sets the dump format for generic parameters.
        /// Parameters: 0 - declaring type name, 1 - declaring type namespace, 2 - assembly qualified name of the declaring type
        /// Default: &quot;{0}&quot;
        /// </summary>
        public static string GenericParam { get; set; } = Resources.GenericParamFormat;

        /// <summary>
        /// Gets or sets the dump format for the type of the indexers' indexes.
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type
        /// Default: &quot;{0}&quot;
        /// </summary>
        public static string IndexerIndexType { get; set; } = Resources.IndexerIndexDumpFormat;

        /// <summary>
        /// Gets or sets the dump format for objects of type <seealso cref="System.Reflection.MemberInfo"/>.
        /// Parameters: 0 - value
        /// Default: &quot;{0}&quot;
        /// </summary>
        public static string MemberInfoMemberType { get; set; } = Resources.MemberInfoMemberTypeDumpFormat;

        /// <summary>
        /// Gets or sets the dump format <see cref="MethodInfo"/>.
        /// Parameters: 0 - return type name, 1 - return type namespace, 2 - assembly qualified name of the return type,
        /// 3 - declaring type name, 4 - declaring type namespace, 5 - assembly qualified name of the declaring type, 6 - method name.
        /// Default: &quot;{0} {3}.{6}&quot;
        /// </summary>
        public static string MethodInfo { get; set; } = Resources.MethodInfoDumpFormat;

        /// <summary>
        /// Gets or sets the dump format for method parameters.
        /// Parameters: 0 - parameter type name, 1 - parameter type namespace, 2 - assembly qualified name of the parameter type, 3 - parameter name.
        /// Default: &quot;{0} {3}&quot;
        /// </summary>
        public static string MethodParameter { get; set; } = Resources.MethodParameterFormat;

        /// <summary>
        /// Gets or sets the dump format for a property dump label.
        /// Parameters: 0 - property name.
        /// Default: &quot;{0,-24} = &quot;
        /// </summary>
        public static string DefaultPropertyLabel { get; set; } = Resources.PropertyLabelFormat;

        /// <summary>
        /// Gets or sets the dump format for the string designating that a sequence dump has been truncated.
        /// Parameters: 0 - the maximum number of dumped sequence elements.
        /// Default: &quot;... dumped the first {0} elements.&quot;
        /// </summary>
        public static string SequenceDumpTruncated { get; set; } = Resources.SequenceDumpTruncatedFormat;

        /// <summary>
        /// Gets or sets the dump format for the type of a sequence. Note that by the time this format is used, the sequence type name,
        /// its generic parameters and dimension are already printed using <see cref="SequenceTypeName"/>.
        /// Parameters: 0 - sequence type name, 1 - sequence type namespace, 2 - assembly qualified name of the sequence type,
        /// Default: &quot;({2})&quot;
        /// </summary>
        public static string SequenceType { get; set; } = Resources.SequenceTypeFormat;

        /// <summary>
        /// Gets or sets the dump format for the type of a sequence.
        /// Parameters: 0 - sequence type name, 1 - number of elements in the sequence.
        /// Default: &quot;{0}[{1}]: &quot;
        /// </summary>
        public static string SequenceTypeName { get; set; } = Resources.SequenceTypeNameFormat;

        /// <summary>
        /// Gets or sets the dump format for a type.
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type,
        /// Default: &quot;{0} ({2}): &quot;
        /// </summary>
        public static string Type { get; set; } = Resources.TypeDumpFormat;

        /// <summary>
        /// Gets or sets the dump format for dumped values of type <see cref="Type"/>.
        /// Parameters: 0 - type name, 1 - type namespace, 2 - assembly qualified name of the type,
        /// Default: &quot;{2}&quot;
        /// </summary>
        public static string TypeInfo { get; set; } = Resources.TypeInfoFormat;

        /// <summary>
        /// Gets or sets the dump format for a value.
        /// Parameters: 0 - the value to be dumped.
        /// Default: &quot;{0}&quot;
        /// </summary>
        public static string Value { get; set; } = Resources.ValueFormat;

        /// <summary>
        /// Gets or sets the C# expression dump label.
        /// </summary>
        public static string CSharpDumpLabel { get; set; } = Resources.CSharpDumpLabelFormat;
    }
}
