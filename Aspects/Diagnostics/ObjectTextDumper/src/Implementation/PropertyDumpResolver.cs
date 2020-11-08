using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace vm.Aspects.Diagnostics.Implementation
{
    /// <summary>
    /// Primary responsibility of the class is to retrieve the DumpAttribute instance associated with a property to be dumped from the property's
    /// class or associated metadata type (buddy). For improved performance the class encapsulates a cache of PropertyInfo describing the property
    /// and the associated <see cref="DumpAttribute"/>.
    /// </summary>
    static class PropertyDumpResolver
    {
        /// <summary>
        /// Gets the synchronization object of the cache/dictionary of property info-dump attributes.
        /// </summary>
        static ReaderWriterLockSlim SyncPropertiesDumpData { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Gets the cache/dictionary of property info-dump attributes.
        /// </summary>
        static Dictionary<(MemberInfo mi, Type? metadata), DumpAttribute> PropertiesDumpData { get; } = new Dictionary<(MemberInfo mi, Type? metadata), DumpAttribute>();

        /// <summary>
        /// Gets the dump attribute applied to a property.
        /// </summary>
        /// <param name="mi">The property info.</param>
        /// <param name="metadata">Type of the metadata.</param>
        /// <returns></returns>
        public static DumpAttribute GetPropertyDumpAttribute(
            MemberInfo mi,
            Type? metadata = null)
        {
            if (mi is not PropertyInfo and not FieldInfo)
                throw new ArgumentException("The parameter can be only "+nameof(PropertyInfo)+" or "+nameof(FieldInfo)+" type.", nameof(mi));

            // if we have the dump attribute in the cache - return it
            using (new ReaderSlimSync(SyncPropertiesDumpData))
                if (PropertiesDumpData.TryGetValue((mi, metadata), out var da) && da is not null)
                    return da;

            var pi = mi as PropertyInfo;
            DumpAttribute? dumpAttribute = null;

            // is there an attribute on the corresponding property in the metadata
            if (metadata != null  &&  pi?.GetIndexParameters().Length == 0)
            {
                MemberInfo? miMeta = null;

                foreach (var f in metadata.GetFields()
                                          .Union<MemberInfo>(
                                  metadata.GetProperties())
                                          .Where(f => f.Name == mi.Name))
                    if (miMeta?.DeclaringType?.IsAssignableFrom(f.DeclaringType) is not false)
                        miMeta = f;         // ^if hidden property(?) - take the property from the most derived class:

                //// if not found in the properties - search in the fields
                //if (miMeta == null)
                //    foreach (var p in metadata.GetProperties()
                //                              .Where(p => p.Name == pi.Name))
                //        if (miMeta == null)
                //            miMeta = p;
                //        else
                //            // hidden property(?) - take the property from the most derived class:
                //            if (miMeta.DeclaringType.IsAssignableFrom(p.DeclaringType))
                //                miMeta = p;

                if (miMeta!=null)
                    dumpAttribute = miMeta.GetCustomAttribute<DumpAttribute>();
            }

            if (dumpAttribute == null)
                // get the attribute directly from the property's type, if still no dump attribute - assume the default dump attribute
                dumpAttribute = mi.GetCustomAttribute<DumpAttribute>() ?? DumpAttribute.Default;

            // put the property info and the attribute in the cache
            using (new WriterSlimSync(SyncPropertiesDumpData))
                PropertiesDumpData[(mi, metadata)] = dumpAttribute;

            // return the dump attribute
            return dumpAttribute;
        }
    }
}
