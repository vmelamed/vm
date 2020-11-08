using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using vm.Aspects.Diagnostics.Implementation;

using MetadataTypeAttribute = System.ComponentModel.DataAnnotations.MetadataTypeAttribute;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Primary responsibility of the class is to retrieve the metadata (the buddy class) and DumpAttribute instance associated with a class to be
    /// dumped. For speed the class encapsulates a cache of type (usually class or struct) and the associated <see cref="ClassDumpData"/> which
    /// contains the buddy and DumpAttribute instance. Allows to define externally metadata and a DumpAttribute on a class/struct level - useful for
    /// FCL and 3rd party classes for which we do not have access to their source code.
    /// </summary>
    static class ClassMetadataResolver
    {
        /// <summary>
        /// Synchronized cache of dump metadata (buddy classes) defined explicitly either in the initializer above or by calling SetMetadataType.
        /// </summary>
        static readonly Dictionary<Type, ClassDumpData> _typesDumpData = new Dictionary<Type, ClassDumpData>();
        static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static Dictionary<Type, ClassDumpData> GetSnapshotTypesDumpData()
        {
            using var _ = new ReaderSlimSync(_lock);

            return _typesDumpData.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Resets the class dump.
        /// </summary>
        internal static void ResetClassDumpData()
        {
            using var _ = new WriterSlimSync(_lock);

            _typesDumpData.Clear();
        }

        /// <summary>
        /// Adds buddy type and dump attribute for classes which we do not have access to, e.g. Exception.
        /// </summary>
        /// <param name="type">The type for which to set buddy type and dump attribute.</param>
        /// <param name="metadata">The metadata type (buddy class).</param>
        /// <param name="dumpAttribute">The dump attribute.</param>
        /// <param name="replace">
        /// If set to <see langword="false" /> and there is already dump metadata associated with the <paramref name="type"/>
        /// the method will throw exception of type <see cref="InvalidOperationException"/>;
        /// otherwise it will silently override the existing metadata with <paramref name="metadata"/> and <paramref name="dumpAttribute"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="replace"/> is <see langword="false"/> and there is already metadata associated with the <paramref name="type"/>.
        /// </exception>
        public static void SetClassDumpData(
            Type type,
            Type? metadata = null,
            DumpAttribute? dumpAttribute = null,
            bool replace = false)
        {
            if (metadata == null)
            {
                var attribute = type.GetCustomAttribute<MetadataTypeAttribute>();

                metadata = attribute is not null
                                ? attribute.MetadataClassType
                                : type;
            }

            AddClassDumpData(type, metadata, dumpAttribute, replace);
        }

        /// <summary>
        /// Gets the dump attribute either from the type itself or if the class is applied <see cref="MetadataTypeAttribute" /> from the specified class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="dumpAttribute">The dump attribute.</param>
        /// <returns>
        /// The <see cref="DumpAttribute" /> optional, manually provided, dump attribute.
        /// </returns>
        public static ClassDumpData GetClassDumpData(
            Type type,
            DumpAttribute? dumpAttribute = null)
        {
            // the dump data in the cache has preference:
            // if the class is already in the cache return that
            var dumpData = TryGetClassDumpData(type);

            if (dumpData.HasValue)
                return dumpData.Value;

            // extract the dump data from the type
            var dmpDta = ExtractClassDumpData(type);

            // add it to the cache
            AddClassDumpData(type, dmpDta, false);

            // and return what we found
            return dumpAttribute != null ? new ClassDumpData(dmpDta.Metadata, dumpAttribute) : dmpDta;
        }

        static ClassDumpData ExtractClassDumpData(Type type)
        {
            // see if the class has a buddy:
            var attribute = type.GetCustomAttribute<MetadataTypeAttribute>();

            // see if the class is generic and the open generic has a buddy:
            if (attribute is null  &&  type.IsGenericType)
                attribute = type.GetGenericTypeDefinition()
                                .GetCustomAttribute<MetadataTypeAttribute>();

            // if there is no buddy, we assume that the class provides the metadata itself
            return new ClassDumpData(attribute?.MetadataClassType ?? type);
        }

        static ClassDumpData? TryGetClassDumpData(Type type)
        {
            using var _ = new ReaderSlimSync(_lock);

            return _typesDumpData.TryGetValue(type, out var dumpData)
                ? dumpData
                : type.IsGenericType && _typesDumpData.TryGetValue(type.GetGenericTypeDefinition(), out dumpData)
                    ? dumpData
                    : null;
        }

        static void AddClassDumpData(
            Type type,
            Type buddy,
            DumpAttribute? dumpAttribute,
            bool replace) => AddClassDumpData(type, new ClassDumpData(buddy, dumpAttribute), replace);

        static void AddClassDumpData(
            Type type,
            ClassDumpData classDumpData,
            bool replace)
        {
            using var _ = new WriterSlimSync(_lock);

            if (replace || !_typesDumpData.TryGetValue(type, out var dumpData))
            {
                _typesDumpData[type] = classDumpData;
                return;
            }

            if (dumpData != classDumpData)
                throw new InvalidOperationException($"The type {type.FullName} is already associated with metadata type {_typesDumpData[type].Metadata.FullName} and a DumpAttribute instance.");
        }
    }
}
