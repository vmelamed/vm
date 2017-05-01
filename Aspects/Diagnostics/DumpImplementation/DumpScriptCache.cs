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
        readonly static IDictionary<ScriptLookup, Action<object, ClassDumpData, ObjectTextDumper>> _cache = new Dictionary<ScriptLookup, Action<object, ClassDumpData, ObjectTextDumper>>();

        internal static bool FoundAndExecuted(
            ObjectTextDumper objectTextDumper,
            object obj,
            ClassDumpData classDumpData)
        {
            Contract.Requires<ArgumentNullException>(objectTextDumper != null, nameof(objectTextDumper));
            Contract.Requires<ArgumentNullException>(obj              != null, nameof(obj));

            Action<object, ClassDumpData, ObjectTextDumper> dumpScript;

            _sync.EnterReadLock();
            var found = _cache.TryGetValue(new ScriptLookup(obj.GetType(), classDumpData, objectTextDumper), out dumpScript);
            _sync.ExitReadLock();

            if (!found)
                return false;

            dumpScript(obj, classDumpData, objectTextDumper);
            return true;
        }

        internal static void Add(
            ObjectTextDumper objectTextDumper,
            Type objectType,
            ClassDumpData classDumpData,
            DumpScript _dumpScript)
        {
            Contract.Requires<ArgumentNullException>(objectTextDumper != null, nameof(objectTextDumper));
            Contract.Requires<ArgumentNullException>(objectType       != null, nameof(objectType));
            Contract.Requires<ArgumentNullException>(_dumpScript      != null, nameof(_dumpScript));

            Action<object, ClassDumpData, ObjectTextDumper> script = _dumpScript.GetScript();

            _sync.EnterWriteLock();
            _cache[new ScriptLookup(objectType, classDumpData, objectTextDumper)] = script;
            _sync.ExitWriteLock();
        }

        internal static void Reset()
        {
            _sync.EnterWriteLock();
            _cache.Clear();
            _sync.ExitWriteLock();
        }
    }
}
