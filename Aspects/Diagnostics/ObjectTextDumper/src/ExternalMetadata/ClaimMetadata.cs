#pragma warning disable CS1591  // Missing XML comment for publicly visible type or member

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ClaimMetadata
    {
        [Dump(0)]
        public object? Type { get; set; }

        [Dump(1)]
        public object? Value { get; set; }

        [Dump(2)]
        public object? ValueType { get; set; }

        [Dump(false)]
        public object? Issuer { get; set; }

        [Dump(false)]
        public object? OriginalIssuer { get; set; }

        [Dump(false)]
        public object? Properties { get; set; }

        [Dump(false)]
        public object? Subject { get; set; }

        [Dump(false)]
        public object? CustomSerializationData { get; set; }
    }
}
