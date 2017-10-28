namespace vm.Aspects.Diagnostics.ExternalMetadata
{
#pragma warning disable 1591
    public abstract class ClaimsIdentityMetadata
    {
        [Dump(0, DumpNullValues = ShouldDump.Skip)]
        public object AuthenticationType;

        [Dump(1, DumpNullValues = ShouldDump.Skip)]
        public object Name;

        [Dump(2)]
        public object IsAuthenticated;

        [Dump(false)]
        public object NameClaimType;

        [Dump(false)]
        public object RoleClaimType;

        [Dump(false)]
        public object Label;

        [Dump(false)]
        public object Actor;

        [Dump(false)]
        public object BootstrapContext;

        [Dump(false)]
        public object CustomSerializationData;

        [Dump(8)]
        public object Claims;
    }
}
