using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Class NongenericBaseCallHandler encapsulates static dictionaries/caches that are used in the inheriting <see cref="BaseCallHandler{T}"/>
    /// </summary>
    public abstract class NongenericBaseCallHandler
    {
        /// <summary>
        /// Synchronizes the access to the dictionary <see cref="HandlerToGenericContinueWith"/>
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        protected readonly static ReaderWriterLockSlim SyncHandlerToGenericContinueWith = new ReaderWriterLockSlim();
        /// <summary>
        /// The continue-with generic methods mapped to the type of the defining call handlers.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        protected readonly static IDictionary<Type, MethodInfo> HandlerToGenericContinueWith = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// Synchronizes the access to the dictionary <see cref="HandlerTypeReturnTypeToContinueWith"/>
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        protected readonly static ReaderWriterLockSlim SyncMethodToContinueWith = new ReaderWriterLockSlim();
        /// <summary>
        /// The continue with concrete/closed methods mapped to the handler type and the return type of the method being handled.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        protected readonly static IDictionary<HandlerTypeReturnType, MethodInfo> HandlerTypeReturnTypeToContinueWith = new Dictionary<HandlerTypeReturnType, MethodInfo>();

        /// <summary>
        /// Struct HandlerTypeReturnType encapsulates a handler type and the return type of particular method being handled in that handler.
        /// The struct is used as a key in the <see cref="HandlerTypeReturnTypeToContinueWith"/> dictionary above.
        /// </summary>
        protected struct HandlerTypeReturnType : IEquatable<HandlerTypeReturnType>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="HandlerTypeReturnType"/> struct.
            /// </summary>
            /// <param name="handlerType">The handler type.</param>
            /// <param name="returnType">The method's return type.</param>
            public HandlerTypeReturnType(
                Type handlerType,
                Type returnType)
            {
                HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
                ReturnType  = returnType  ?? throw new ArgumentNullException(nameof(returnType));
            }

            /// <summary>
            /// Gets the type of the call handler.
            /// </summary>
            public Type HandlerType { get; }

            /// <summary>
            /// Gets the method's return type.
            /// </summary>
            public Type ReturnType { get; }

            #region Identity rules implementation.
            /// <remarks/>
            public bool Equals(HandlerTypeReturnType other)
            {
                return HandlerType == other.HandlerType  &&
                       ReturnType  == other.ReturnType;
            }

            /// <remarks/>
            public override bool Equals(object obj) => obj is HandlerTypeReturnType ? Equals((HandlerTypeReturnType)obj) : false;

            /// <remarks/>
            public override int GetHashCode()
            {
                var hashCode = Constants.HashInitializer;

                unchecked
                {
                    hashCode = Constants.HashMultiplier * hashCode + HandlerType.GetHashCode();
                    hashCode = Constants.HashMultiplier * hashCode + ReturnType.GetHashCode();
                }

                return hashCode;
            }

            /// <remarks/>
            public static bool operator ==(HandlerTypeReturnType left, HandlerTypeReturnType right) => left.Equals(right);

            /// <remarks/>
            public static bool operator !=(HandlerTypeReturnType left, HandlerTypeReturnType right) => !(left==right);
            #endregion
        }
    }
}
