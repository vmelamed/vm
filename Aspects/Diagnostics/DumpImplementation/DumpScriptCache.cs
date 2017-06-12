using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    static class DumpScriptCache
    {
        struct ScriptLookup : IEquatable<ScriptLookup>
        {
            public ScriptLookup(
                Type objectType,
                ClassDumpData classDumpData,
                ObjectTextDumper objectTextDumper)
            {
                Contract.Requires<ArgumentNullException>(objectTextDumper != null, nameof(objectTextDumper));
                Contract.Requires<ArgumentNullException>(objectType       != null, nameof(objectType));

                ObjectType             = objectType;
                ClassDumpData          = classDumpData;
                PropertiesBindingFlags = objectTextDumper.PropertiesBindingFlags;
                FieldsBindingFlags     = objectTextDumper.FieldsBindingFlags;
            }

            public Type ObjectType { get; }

            public ClassDumpData ClassDumpData { get; }

            public BindingFlags PropertiesBindingFlags { get; }

            public BindingFlags FieldsBindingFlags { get; }

            #region Identity rules implementation.
            #region IEquatable<ScriptLookup> Members
            public bool Equals(ScriptLookup other) =>
                ObjectType             == other.ObjectType              &&
                ClassDumpData          == other.ClassDumpData           &&
                PropertiesBindingFlags == other.PropertiesBindingFlags  &&
                FieldsBindingFlags     == other.FieldsBindingFlags;
            #endregion

            public override bool Equals(object obj) => obj is ScriptLookup ? Equals((ScriptLookup)obj) : false;

            public override int GetHashCode()
            {
                var hashCode = Constants.HashInitializer;

                hashCode = Constants.HashMultiplier * hashCode + ObjectType.GetHashCode();
                hashCode = Constants.HashMultiplier * hashCode + ClassDumpData.GetHashCode();
                hashCode = Constants.HashMultiplier * hashCode + PropertiesBindingFlags.GetHashCode();
                hashCode = Constants.HashMultiplier * hashCode + FieldsBindingFlags.GetHashCode();

                return hashCode;
            }

            public static bool operator ==(ScriptLookup left, ScriptLookup right) => left.Equals(right);

            public static bool operator !=(ScriptLookup left, ScriptLookup right) => !(left==right);
            #endregion
        };

        readonly static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();
        readonly static IDictionary<ScriptLookup, Script> _cache = new Dictionary<ScriptLookup, Script>();
        readonly static ISet<ScriptLookup> _buildingNow = new HashSet<ScriptLookup>();

        internal static bool TryFind(
            ObjectTextDumper objectTextDumper,
            object obj,
            ClassDumpData classDumpData,
            out Script script)
        {
            Contract.Requires<ArgumentNullException>(objectTextDumper != null, nameof(objectTextDumper));
            Contract.Requires<ArgumentNullException>(obj              != null, nameof(obj));

            script = null;

            var lookup = new ScriptLookup(obj.GetType(), classDumpData, objectTextDumper);

            _sync.EnterReadLock();

            var found = _cache.TryGetValue(lookup, out script);

            if (!found)
                found = _buildingNow.Contains(lookup);

            _sync.ExitReadLock();
            return found;
        }

        internal static Script Add(
            ObjectTextDumper objectTextDumper,
            Type objectType,
            ClassDumpData classDumpData,
            DumpScript _dumpScript)
        {
            Contract.Requires<ArgumentNullException>(objectTextDumper != null, nameof(objectTextDumper));
            Contract.Requires<ArgumentNullException>(objectType       != null, nameof(objectType));
            Contract.Requires<ArgumentNullException>(_dumpScript      != null, nameof(_dumpScript));

            var lookup = new ScriptLookup(objectType, classDumpData, objectTextDumper);
            var script = _dumpScript.GetScriptAction();

            _sync.EnterWriteLock();

            _cache[lookup] = script;
            _buildingNow.Remove(lookup);

            _sync.ExitWriteLock();

            return script;
        }

        internal static void Reset()
        {
            _sync.EnterWriteLock();

            _cache.Clear();
            _buildingNow.Clear();

            _sync.ExitWriteLock();
        }

        internal static void BuildingScriptFor(
            ObjectTextDumper objectTextDumper,
            Type objectType,
            ClassDumpData classDumpData)
        {
            _sync.EnterWriteLock();

            _buildingNow.Add(new ScriptLookup(objectType, classDumpData, objectTextDumper));

            _sync.ExitWriteLock();
        }
    }
}
