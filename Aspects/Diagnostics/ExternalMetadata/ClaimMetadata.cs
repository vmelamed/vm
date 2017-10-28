namespace vm.Aspects.Diagnostics.ExternalMetadata
{
#pragma warning disable 1591
    public abstract class ClaimMetadata
    {
        [Dump(0)]
        public object Type;

        [Dump(1)]
        public object Value;

        [Dump(2)]
        public object ValueType;

        [Dump(false)]
        public object Issuer;

        [Dump(false)]
        public object OriginalIssuer;

        [Dump(false)]
        public object Properties;

        [Dump(false)]
        public object Subject;

        [Dump(false)]
        public object CustomSerializationData;
    }
}
