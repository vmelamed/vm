using System;
using System.Collections.Generic;

namespace vm.Aspects.Security.Cryptography.Ciphers.DefaultServices
{
    static class Resolver
    {
        class TypeAndName : IEquatable<TypeAndName>
        {
            public TypeAndName(
                Type type,
                string name = null)
            {
                Type = type  ??  throw new ArgumentNullException(nameof(type));
                Name = name.IsNullOrWhiteSpace() ? null : name;
            }

            public Type Type { get; set; }
            public string Name { get; set; }

            #region Identity rules implementation.
            #region IEquatable<TypeAndName> Members
            /// <summary>
            /// Indicates whether the current object is equal to a reference to another object of the same type.
            /// </summary>
            /// <param name="other">A reference to another object of type <see cref="TypeAndName"/> to compare with this object.</param>
            /// <returns>
            /// <c>false</c> if <paramref name="other"/> is equal to <c>null</c>, otherwise
            /// <c>true</c> if <paramref name="other"/> refers to <c>this</c> object, otherwise
            /// <c>true</c> if <i>the business identities</i> of the current object and the <paramref name="other"/> are equal by value,
            /// e.g. <c>BusinessKeyProperty == other.BusinessKeyProperty &amp;&amp; (some other properties equality...);</c>; otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>
            /// The <see cref="Equals(object)"/> methods and the overloaded <c>operator==</c>-s test for business identity, 
            /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
            /// </remarks>
            public virtual bool Equals(TypeAndName other)
            {
                if (other is null)
                    return false;
                if (ReferenceEquals(this, other))
                    return true;

                return Type == other.Type  &&
                       Name == other.Name;
            }
            #endregion

            /// <summary>
            /// Determines whether this <see cref="TypeAndName"/> instance is equal to the specified <see cref="System.Object"/> reference.
            /// </summary>
            /// <param name="obj">The <see cref="Object"/> reference to compare with this <see cref="TypeAndName"/> object.</param>
            /// <returns>
            /// <c>false</c> if <paramref name="obj"/> is equal to <c>null</c>, otherwise
            /// <c>true</c> if <paramref name="obj"/> refers to <c>this</c> object, otherwise
            /// <c>true</c> if <paramref name="obj"/> <i>is an instance of</i> <see cref="TypeAndName"/> and 
            /// <i>the business identities</i> of the current object and the <paramref name="obj"/> are equal by value; otherwise, 
            /// <c>false</c>.
            /// </returns>
            /// <remarks>
            /// The <see cref="Equals(TypeAndName)"/> methods and the overloaded <c>operator==</c>-s test for business identity, 
            /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
            /// </remarks>
            public override bool Equals(object obj) => Equals(obj as TypeAndName);

            /// <summary>
            /// Serves as a hash function for the objects of <see cref="TypeAndName"/> and its derived types.
            /// </summary>
            /// <returns>A hash code for the current <see cref="TypeAndName"/> instance.</returns>
            public override int GetHashCode()
            {
                var hashCode = 17;

                unchecked
                {
                    hashCode = 37 * hashCode + Type.GetHashCode();
                    hashCode = 37 * hashCode + (Name?.GetHashCode() ?? 0);
                }

                return hashCode;
            }

            /// <summary>
            /// Compares two <see cref="TypeAndName"/> objects.
            /// </summary>
            /// <param name="left">The left operand.</param>
            /// <param name="right">The right operand.</param>
            /// <returns>
            /// <c>true</c> if the objects are considered to be equal (<see cref="M:Equals{TypeAndName}"/>);
            /// otherwise <c>false</c>.
            /// </returns>
            public static bool operator ==(
                TypeAndName left,
                TypeAndName right) => left is null
                                        ? right is null
                                        : left.Equals(right);

            /// <summary>
            /// Compares two <see cref="TypeAndName"/> objects.
            /// </summary>
            /// <param name="left">The left operand.</param>
            /// <param name="right">The right operand.</param>
            /// <returns>
            /// <c>true</c> if the objects are not considered to be equal (<see cref="M:Equals{TypeAndName}"/>);
            /// otherwise <c>false</c>.
            /// </returns>
            public static bool operator !=(
                TypeAndName left,
                TypeAndName right) => !(left==right);
            #endregion
        }

        public const string Keyed = "keyed";

        static readonly IDictionary<TypeAndName, Func<object>> _defaultServices = new Dictionary<TypeAndName, Func<object>>
        {
            { new TypeAndName(typeof(IKeyLocationStrategy)          ), () => new KeyFileLocationStrategy()   },
            { new TypeAndName(typeof(IKeyStorage)                   ), () => new KeyFileStorage()            },
            { new TypeAndName(typeof(IKeyStorageTasks)              ), () => new KeyFileStorage()            },
            { new TypeAndName(typeof(IRandom)                       ), () => new RandomGenerator()           },
            { new TypeAndName(typeof(ISymmetricAlgorithmFactory)    ), () => new SymmetricAlgorithmFactory() },
            { new TypeAndName(typeof(IHashAlgorithmFactory)         ), () => new HashAlgorithmFactory()      },
            { new TypeAndName(typeof(IHashAlgorithmFactory), Keyed  ), () => new KeyedHashAlgorithmFactory() },
        };

        public static T TryGetInstance<T>(string key = null) where T : class
            => _defaultServices.TryGetValue(new TypeAndName(typeof(T), key), out var f) ? (T)f() : default;

        public static T GetInstance<T>(string key = null) where T : class
            => TryGetInstance<T>(key)
                    ?? throw new InvalidOperationException($"Could not get an instance of {typeof(T).Name} with a key \"{key ?? "<null>"}\"");

        public static T GetInstanceOrDefault<T>(T service, string key = null) where T : class
            => service  ??  GetInstance<T>(key);
    }
}
