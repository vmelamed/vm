using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace vm.Aspects.Diagnostics.Implementation
{
    static class DumpScriptCache
    {
        struct ScriptLookup : IEquatable<ScriptLookup>
        {
            public ScriptLookup(
                Type objectType,
                ClassDumpMetadata classDumpMetadata,
                ObjectTextDumper objectTextDumper)
            {
                ObjectType             = objectType;
                ClassDumpMetadata      = classDumpMetadata;
                PropertiesBindingFlags = objectTextDumper.Settings.PropertyBindingFlags;
                FieldsBindingFlags     = objectTextDumper.Settings.FieldBindingFlags;
            }

            public Type ObjectType { get; }

            public ClassDumpMetadata ClassDumpMetadata { get; }

            public BindingFlags PropertiesBindingFlags { get; }

            public BindingFlags FieldsBindingFlags { get; }

            #region Identity rules implementation.
            #region IEquatable<ScriptLookup> Members
            public bool Equals(ScriptLookup other) =>
                ObjectType             == other.ObjectType              &&
                ClassDumpMetadata      == other.ClassDumpMetadata       &&
                PropertiesBindingFlags == other.PropertiesBindingFlags  &&
                FieldsBindingFlags     == other.FieldsBindingFlags;
            #endregion

            public override bool Equals(object? obj) => obj is ScriptLookup sl && Equals(sl);

            public static bool operator ==(ScriptLookup left, ScriptLookup right) => left.Equals(right);

            public static bool operator !=(ScriptLookup left, ScriptLookup right) => !(left == right);

            public override int GetHashCode() =>
                HashCode.Combine(ObjectType, ClassDumpMetadata, PropertiesBindingFlags, FieldsBindingFlags);
            #endregion
        };

        readonly static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();
        // cache of the assembled an possibly compiled already scripts
        readonly static IDictionary<ScriptLookup, Script> _cache = new Dictionary<ScriptLookup, Script>();
        // scripts that are in the process of building
        readonly static ISet<ScriptLookup> _buildingNow = new HashSet<ScriptLookup>();

        internal static bool TryFind(
            ObjectTextDumper objectTextDumper,
            object obj,
            ClassDumpMetadata classDumpMetadata,
            out Script? script)
        {
            var lookup = new ScriptLookup(obj.GetType(), classDumpMetadata, objectTextDumper);

            using var _ = new ReaderSlimSync(_sync);

            var found = _cache.TryGetValue(lookup, out script);

            // ok, it is not compiled yet but maybe it is still building
            if (!found)
                found = _buildingNow.Contains(lookup);

            return found;
        }

        internal static Script Add(
            ObjectTextDumper objectTextDumper,
            Type objectType,
            ClassDumpMetadata classDumpMetadata,
            DumpScript _dumpScript)
        {
            var lookup = new ScriptLookup(objectType, classDumpMetadata, objectTextDumper);
            var script = _dumpScript.Compile();

            using var _ = new WriterSlimSync(_sync);

            _cache[lookup] = script;
            _buildingNow.Remove(lookup);

            return script;
        }

        internal static void Reset()
        {
            using var _ = new WriterSlimSync(_sync);

            _cache.Clear();
            _buildingNow.Clear();
        }

        internal static void BuildingScriptFor(
            ObjectTextDumper objectTextDumper,
            Type objectType,
            ClassDumpMetadata classDumpMetadata)
        {
            using var _ = new WriterSlimSync(_sync);

            _buildingNow.Add(new ScriptLookup(objectType, classDumpMetadata, objectTextDumper));
        }
    }
}
