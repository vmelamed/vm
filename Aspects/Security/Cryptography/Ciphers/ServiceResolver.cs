using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    class ServiceResolver : ServiceLocatorImplBase
    {
        class TypeAndName : IEquatable<TypeAndName>
        {
            public TypeAndName(
                Type type,
                string name)
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                if (string.IsNullOrWhiteSpace(name))
                    name = string.Empty;

                Type = type;
                Name = name;
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
            /// The <see cref="M:Equals"/> methods and the overloaded <c>operator==</c>-s test for business identity, 
            /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
            /// </remarks>
            public virtual bool Equals(TypeAndName other)
            {
                if (ReferenceEquals(other, null))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;

                return Type == other.Type && Name == other.Name;
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
            /// The <see cref="M:Equals"/> methods and the overloaded <c>operator==</c>-s test for business identity, 
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

                hashCode = 37 * hashCode + Type.GetHashCode();
                hashCode = 37 * hashCode + Name.GetHashCode();

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
                TypeAndName right) => ReferenceEquals(left, null)
                                        ? ReferenceEquals(right, null)
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

        static Lazy<IServiceLocator> _serviceLocator = new Lazy<IServiceLocator>(() => new ServiceResolver(), true);

        internal static IServiceLocator Current => _serviceLocator.Value;

        readonly IDictionary<TypeAndName, Lazy<object>> _defaultServices = new Dictionary<TypeAndName, Lazy<object>>
        {
            { new TypeAndName(typeof(IKeyLocationStrategy),       string.Empty),                     new Lazy<object>(() => new KeyLocationStrategy())           },
            { new TypeAndName(typeof(IKeyStorage),                string.Empty),                     new Lazy<object>(() => new KeyFile())                       },
            { new TypeAndName(typeof(IKeyStorageAsync),           string.Empty),                     new Lazy<object>(() => new KeyFile())                       },

            { new TypeAndName(typeof(string),                     Algorithms.Symmetric.ResolveName), new Lazy<object>(() => Algorithms.Symmetric.Default)        },
            { new TypeAndName(typeof(ISymmetricAlgorithmFactory), string.Empty),                     new Lazy<object>(() => new SymmetricAlgorithmFactory())     },

            { new TypeAndName(typeof(string),                     Algorithms.Hash.ResolveName),      new Lazy<object>(() => Algorithms.Hash.Default)             },
            { new TypeAndName(typeof(IHashAlgorithmFactory),      string.Empty),                     new Lazy<object>(() => new HashAlgorithmFactory())          },

            { new TypeAndName(typeof(string),                     Algorithms.KeyedHash.ResolveName), new Lazy<object>(() => Algorithms.KeyedHash.Default)        },
            { new TypeAndName(typeof(IHashAlgorithmFactory),      Algorithms.KeyedHash.ResolveName), new Lazy<object>(() => new KeyedHashAlgorithmFactory())     },

            { new TypeAndName(typeof(string),                     Algorithms.Signature.ResolveName), new Lazy<object>(() => Algorithms.Signature.Default)        },
        };

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The DI container should do it.")]
        public ServiceResolver()
        {
            _defaultServices[new TypeAndName(typeof(IServiceLocator), string.Empty)] = new Lazy<object>(() => this);
        }

        /// <summary>
        /// Does the actual work of resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">
        /// Type of service requested.
        /// </param>
        /// <returns>
        /// Sequence of service instance objects.
        /// </returns>
        /// <exception cref="Microsoft.Practices.ServiceLocation.ActivationException">
        /// Thrown if the service type is not supported.
        /// </exception>
        protected override IEnumerable<object> DoGetAllInstances(
            Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return _defaultServices.Where(kv => kv.Key.Type == serviceType)
                                   .Select(kv => kv.Value.Value)
                                   .ToList();
        }

        /// <summary>
        /// Does the actual work of resolving the requested service instance.
        /// </summary>
        /// <param name="serviceType">
        /// Type of instance requested.
        /// </param>
        /// <param name="key">
        /// Name of registered service you want. May be null.
        /// </param>
        /// <returns>
        /// The requested service instance.
        /// </returns>
        /// <exception cref="Microsoft.Practices.ServiceLocation.ActivationException">
        /// Thrown if the service type is not supported.
        /// </exception>
        protected override object DoGetInstance(
            Type serviceType,
            string key)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (string.IsNullOrWhiteSpace(key))
                key = string.Empty;

            var v = _defaultServices.FirstOrDefault(kv => kv.Key == new TypeAndName(serviceType, key));

            if (v.Key == null)
                throw new ActivationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Service type {0} with key {1} is not registered in the internal service locator.",
                                serviceType.FullName,
                                key));

            return v.Value.Value;
        }

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(_defaultServices != null, "The field cannot be null.");
        }
    }
}
