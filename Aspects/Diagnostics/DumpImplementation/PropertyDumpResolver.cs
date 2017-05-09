using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    /// <summary>
    /// Primary responsibility of the class is to retrieve the DumpAttribute instance associated with a property to be dumped from the property's 
    /// class or associated metadata type (buddy). For improved performance the class encapsulates a cache of PropertyInfo describing the property
    /// and the associated <see cref="DumpAttribute"/>.
    /// </summary>
    static class PropertyDumpResolver
    {
        static Dictionary<Tuple<MemberInfo, Type>, DumpAttribute> _propertyDumpMap = new Dictionary<Tuple<MemberInfo, Type>, DumpAttribute>();
        static ReaderWriterLockSlim _lockPropertyDumpMap                           = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Gets the synchronization object of the cache/dictionary of property info-dump attributes.
        /// </summary>
        static ReaderWriterLockSlim SyncPropertiesDumpData => _lockPropertyDumpMap;

        /// <summary>
        /// Gets the cache/dictionary of property info-dump attributes.
        /// </summary>
        static Dictionary<Tuple<MemberInfo, Type>, DumpAttribute> PropertiesDumpData => _propertyDumpMap;

        /// <summary>
        /// Gets the dump attribute applied to a property.
        /// </summary>
        /// <param name="mi">The property info.</param>
        /// <param name="metadata">Type of the metadata.</param>
        /// <returns></returns>
        public static DumpAttribute GetPropertyDumpAttribute(
            MemberInfo mi,
            Type metadata = null)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));
            Contract.Requires((mi is PropertyInfo) || (mi is FieldInfo), "The parameter can be only "+nameof(PropertyInfo)+" or "+nameof(FieldInfo)+" type.");
            Contract.Ensures(Contract.Result<DumpAttribute>() != null);

            var lookup = Tuple.Create(mi, metadata);
            DumpAttribute dumpAttribute;

            // if we have the dump attribute in the cache - return it
            try
            {
                SyncPropertiesDumpData.EnterReadLock();
                if (PropertiesDumpData.TryGetValue(lookup, out dumpAttribute))
                    return dumpAttribute;
            }
            finally
            {
                SyncPropertiesDumpData.ExitReadLock();
            }

            var pi = mi as PropertyInfo;

            // is there an attribute on the corresponding property in the metadata
            if (metadata != null  &&  pi !=null  &&  pi.GetIndexParameters().Length == 0)
            {
                MemberInfo miMeta = null;

                foreach (var f in metadata.GetFields()
                                          .Union<MemberInfo>(
                                                metadata.GetProperties())
                                          .Where(f => f.Name == mi.Name))
                    if (miMeta == null)
                        miMeta = f;
                    else
                        // hidden property(?) - take the property from the most derived class:
                        if (miMeta.DeclaringType.IsAssignableFrom(f.DeclaringType))
                        miMeta = f;

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
            {
                // get the attribute directly from the property's type
                dumpAttribute = mi.GetCustomAttribute<DumpAttribute>();

                // if still no dump attribute - assume the default dump attribute
                if (dumpAttribute == null)
                    dumpAttribute = DumpAttribute.Default;
            }

            // put the property info and the attribute in the cache
            try
            {
                SyncPropertiesDumpData.EnterWriteLock();
                PropertiesDumpData[lookup] = dumpAttribute;
            }
            finally
            {
                SyncPropertiesDumpData.ExitWriteLock();
            }

            // return the dump attribute
            return dumpAttribute;
        }

        public static bool PropertyHasNonDefaultDumpAttribute(
            MemberInfo memberInfo)
        {
            Contract.Requires<ArgumentNullException>(memberInfo != null, nameof(memberInfo));

            try
            {
                SyncPropertiesDumpData.EnterWriteLock();
                return _propertyDumpMap.Any(
                            kv => kv.Key.Item1.Name == memberInfo.Name  &&
                                  kv.Key.Item1.DeclaringType.IsAssignableFrom(memberInfo.DeclaringType)   &&
                                  !kv.Value.IsDefaultAttribute());
            }
            finally
            {
                SyncPropertiesDumpData.ExitWriteLock();
            }
        }
    }
}
