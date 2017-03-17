using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [MetadataType(typeof(EntityMetadata))]
    public partial class Entity : DomainEntity<long, Guid>
    {
        public override Guid Key => UniqueId;

        public virtual Guid UniqueId { get; set; }

        public virtual string RepositoryId { get; set; }

        #region Values
        /// <summary>
        /// Gets or sets the collection of <see cref="Value"/> (for EF and internal use only).
        /// </summary>
        public virtual ICollection<Value> ValuesList { get; set; }
        #endregion

        public virtual DateTime CreatedOn { get; set; }

        public virtual DateTime UpdatedOn { get; set; }

        public virtual byte[] ConcurrencyStamp { get; set; }

    }
}
