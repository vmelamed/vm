using System;
using System.ComponentModel.DataAnnotations;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [MetadataType(typeof(ValueMetadata))]
    public partial class Value : DomainValue<long>
    {
        public virtual string Name { get; set; }

        #region Entity
        /// <summary>
        /// Gets or sets the reference to <see cref="Entity"/>.
        /// </summary>
        public virtual Entity Entity { get; set; }

        /// <summary>
        /// Gets or sets the foreign key helper property. Used by EF and some queries.
        /// </summary>
        public virtual long EntityId { get; set; }
        #endregion

        public virtual DateTime CreatedOn { get; set; }

        public virtual DateTime UpdatedOn { get; set; }
    }
}
