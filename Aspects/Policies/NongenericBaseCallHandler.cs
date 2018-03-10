using System;
using System.Collections.Generic;
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
        /// Struct MethodWithIsOverriddenFlag encapsulates a method info and a flag showing if this method has been overridden in the inheritting classes.
        /// </summary>
        protected struct MethodIsOverridden
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MethodIsOverridden"/> struct.
            /// </summary>
            /// <param name="methodInfo">The method information.</param>
            public MethodIsOverridden(
                MethodInfo methodInfo)
            {
                MethodInfo   = methodInfo;
                IsOverridden = methodInfo!=null ? methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType : false;
            }

            /// <summary>
            /// Gets or sets the method information.
            /// </summary>
            public MethodInfo MethodInfo { get; }

            /// <summary>
            /// Gets or sets a value indicating whether the <see cref="MethodInfo"/> is overridden.
            /// </summary>
            public bool IsOverridden { get; }
        }

        /// <summary>
        /// Struct HandlerForMethod encapsulates a handler type and a particular method being handled in that handler.
        /// The struct is used as a key in the <see cref="MethodToContinueWith"/> dictionary below.
        /// </summary>
        protected struct HandlerForMethod : IEquatable<HandlerForMethod>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="HandlerForMethod"/> struct.
            /// </summary>
            /// <param name="handler">The handler.</param>
            /// <param name="method">The method.</param>
            public HandlerForMethod(
                Type handler,
                MethodBase method)
            {
                Handler = handler ?? throw new ArgumentNullException(nameof(handler));
                Method  = method  ??  throw new ArgumentNullException(nameof(method));
            }

            /// <summary>
            /// Gets the type of the call handler.
            /// </summary>
            public Type Handler { get; }

            /// <summary>
            /// Gets the method being handled.
            /// </summary>
            public MethodBase Method { get; }

            #region Identity rules implementation.
            /// <remarks/>
            public bool Equals(HandlerForMethod other)
            {
                return Handler == other.Handler  &&
                       Method  == other.Method;
            }

            /// <remarks/>
            public override bool Equals(object obj) => obj is HandlerForMethod ? Equals((HandlerForMethod)obj) : false;

            /// <remarks/>
            public override int GetHashCode()
            {
                var hashCode = Constants.HashInitializer;

                unchecked
                {
                    hashCode = Constants.HashMultiplier * hashCode + Handler.GetHashCode();
                    hashCode = Constants.HashMultiplier * hashCode + Method.GetHashCode();
                }

                return hashCode;
            }

            /// <remarks/>
            public static bool operator ==(HandlerForMethod left, HandlerForMethod right) => left.Equals(right);

            /// <remarks/>
            public static bool operator !=(HandlerForMethod left, HandlerForMethod right) => !(left==right);
            #endregion
        }

        /// <summary>
        /// Synchronizes the access to the dictionary <see cref="ContinueWithGenericMethods"/>
        /// </summary>
        protected static ReaderWriterLockSlim SyncContinueWithGenericMethods = new ReaderWriterLockSlim();
        /// <summary>
        /// The continue with generic methods mapped to the actual type of call handlers
        /// </summary>
        protected static IDictionary<Type, MethodIsOverridden> ContinueWithGenericMethods = new Dictionary<Type, MethodIsOverridden>();


        /// <summary>
        /// Synchronizes the access to the dictionary <see cref="MethodToContinueWith"/>
        /// </summary>
        protected static ReaderWriterLockSlim SyncMethodToContinueWith = new ReaderWriterLockSlim();
        /// <summary>
        /// The continue with concrete methods mapped to the actual method calls being handled
        /// </summary>
        protected static IDictionary<HandlerForMethod, MethodInfo> MethodToContinueWith = new Dictionary<HandlerForMethod, MethodInfo>();
    }
}
