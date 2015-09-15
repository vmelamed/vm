using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vm.Aspects.Threading;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    static class TypeNameResolver
    {
        #region Maps of types and type names
        static ReaderWriterLockSlim _typesToNamesLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        /// <summary>
        /// The map of base type to type names
        /// </summary>
        static IDictionary<Type, string> _typesToNames = new Dictionary<Type, string>
        {
            { typeof(void),         "void"          },
            { typeof(char),         "char"          },
            { typeof(bool),         "boolean"       },
            { typeof(byte),         "unsignedByte"  },
            { typeof(sbyte),        "byte"          },
            { typeof(short),        "short"         },
            { typeof(ushort),       "unsignedShort" },
            { typeof(int),          "int"           },
            { typeof(uint),         "unsignedInt"   },
            { typeof(long),         "long"          },
            { typeof(ulong),        "unsignedLong"  },
            { typeof(float),        "float"         },
            { typeof(double),       "double"        },
            { typeof(decimal),      "decimal"       },
            { typeof(Guid),         "guid"          },
            { typeof(Uri),          "anyURI"        },
            { typeof(string),       "string"        },
            { typeof(TimeSpan),     "duration"      },
            { typeof(DateTime),     "dateTime"      },
            { typeof(DBNull),       "dbNull"        },
            { typeof(Nullable<>),   "nullable"      },
            { typeof(object),       "custom"        },
            { typeof(Enum),         "enum"          },
        };

        /// <summary>
        /// The map of type names to base type
        /// </summary>
        static IDictionary<string, Type> _namesToTypes = new Dictionary<string, Type>
        {
            { "void",           typeof(void)        },
            { "char",           typeof(char)        },
            { "boolean",        typeof(bool)        },
            { "unsignedByte",   typeof(byte)        },
            { "byte",           typeof(sbyte)       },
            { "short",          typeof(short)       },
            { "unsignedShort",  typeof(ushort)      },
            { "int",            typeof(int)         },
            { "unsignedInt",    typeof(uint)        },
            { "long",           typeof(long)        },
            { "unsignedLong",   typeof(ulong)       },
            { "float",          typeof(float)       },
            { "double",         typeof(double)      },
            { "decimal",        typeof(decimal)     },
            { "guid",           typeof(Guid)        },
            { "anyURI",         typeof(Uri)         },
            { "string",         typeof(string)      },
            { "duration",       typeof(TimeSpan)    },
            { "dateTime",       typeof(DateTime)    },
            { "dbNull",         typeof(DBNull)      },
            { "nullable",       typeof(Nullable<>)  },
            { "custom",         typeof(object)      },
            { "enum",           typeof(Enum)        },
        };
        #endregion

        /// <summary>
        /// Gets the type corresponding to a type name written in an xml string.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>The specified type.</returns>
        internal static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            Type type;

            using (_typesToNamesLock.UpgradableReaderLock())
            {
                if (_namesToTypes.TryGetValue(typeName, out type))
                    return type;

                type = Type.GetType(typeName);

                if (type != null)
                    using (_typesToNamesLock.WriterLock())
                    {
                        Debug.Assert(!_typesToNames.ContainsKey(type));
                        _namesToTypes[typeName] = type;
                        _typesToNames[type] = typeName;
                    }
            }

            return type;
        }

        /// <summary>
        /// Gets the name of the type appropriate for writing to an XML element.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The name of the type.</returns>
        internal static string GetTypeName(Type type)
        {
            if (type == null)
                return null;

            string typeName;

            using (_typesToNamesLock.UpgradableReaderLock())
            {
                if (_typesToNames.TryGetValue(type, out typeName))
                    return typeName;

                typeName = type.AssemblyQualifiedName;

                if (!string.IsNullOrWhiteSpace(typeName))
                    using (_typesToNamesLock.WriterLock())
                    {
                        Debug.Assert(!_namesToTypes.ContainsKey(typeName));
                        _typesToNames[type] = typeName;
                        _namesToTypes[typeName] = type;
                    }
            }

            return typeName;
        }
    }
}
