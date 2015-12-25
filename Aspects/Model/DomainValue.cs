using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Class DomainValue inherits <see cref="BaseDomainValue"/> and implements <see cref="IHasStoreId{TId}"/>.
    /// </summary>
    /// <typeparam name="TId">The type of the t identifier.</typeparam>
    [CLSCompliant(false)]
    [DebuggerDisplay("{GetType().Name, nq}[{Id,nq}]")]
    public abstract class DomainValue<TId> : BaseDomainValue,
        IHasStoreId<TId>
        where TId : IEquatable<TId>
    {
        #region IHasStoreId<TId> Members
        /// <summary>
        /// Provides the inheritors with an access to the backing field of the property <see cref="Id"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "The inheritors may want to override the default behavior without duplicating the field.")]
        protected TId _id;

        /// <summary>
        /// Gets or sets the store identifier.
        /// </summary>
        [Key]
        public virtual TId Id
        {
            get { return _id; }
            set
            {
                if (!_id.Equals(default(TId)))
                    throw new InvalidOperationException("Once the value of the property is set it cannot be changed.");
                _id = value;
            }
        }
        #endregion
    }
}
