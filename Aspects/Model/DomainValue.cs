using System;
using System.Diagnostics;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Class DomainValue inherits <see cref="BaseDomainValue"/> and implements <see cref="IHasStoreId{TId}"/>.
    /// </summary>
    /// <typeparam name="TId">The type of the t identifier.</typeparam>
    [DebuggerDisplay("{GetType().Name, nq}[{Id,nq}]")]
    public abstract class DomainValue<TId> : BaseDomainValue,
        IHasStoreId<TId>
        where TId : IEquatable<TId>
    {
        #region IHasStoreId<TId> Members
        TId _id;
        bool _idSet;

        /// <summary>
        /// Gets or sets the store identifier.
        /// </summary>
        public virtual TId Id
        {
            get { return _id; }
            set
            {
                if (_idSet)
                    throw new InvalidOperationException("Once the value of the property is set it cannot be changed.");
                _id = value;
                _idSet = true;
            }
        }
        #endregion
    }
}
