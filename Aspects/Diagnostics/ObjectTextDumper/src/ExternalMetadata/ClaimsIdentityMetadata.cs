#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class ClaimsIdentityMetadata
    {
        [Dump(0, DumpNullValues = ShouldDump.Skip)]
        public object AuthenticationType { get; set; }

        [Dump(1, DumpNullValues = ShouldDump.Skip)]
        public object Name { get; set; }

        [Dump(2)]
        public object IsAuthenticated { get; set; }

        [Dump(false)]
        public object NameClaimType { get; set; }

        [Dump(false)]
        public object RoleClaimType { get; set; }

        [Dump(false)]
        public object Label { get; set; }

        [Dump(false)]
        public object Actor { get; set; }

        [Dump(false)]
        public object BootstrapContext { get; set; }

        [Dump(false)]
        public object CustomSerializationData { get; set; }

        [Dump(8)]
        public object Claims { get; set; }
    }
}
