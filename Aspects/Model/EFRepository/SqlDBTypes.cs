namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Defines the textual values of some SQL Server data types for use in entities and values configuration classes.
    /// </summary>
    public static class SqlDBTypes
    {
        /// <summary>
        /// The SQL Server datetime2 type.
        /// </summary>
        public const string DateTime        = "datetime2";

        /// <summary>
        /// The SQL Server rowversion type.
        /// </summary>
        public const string Version         = "rowversion";

        /// <summary>
        /// The SQL Server unlimited Unicode text field - nvarchar(max).
        /// </summary>
        public const string UnlimitedText   = "nvarchar(max)";

        /// <summary>
        /// The SQL Server unlimited binary field (BLOB) type - varbinary(max).
        /// </summary>
        public const string UnlimitedBinary = "varbinary(max)";
    }
}
