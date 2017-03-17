using System.Data.Entity.ModelConfiguration;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    /// <summary>
    /// Maps the <see cref="Entity"/> to database elements (tables, columns, etc.).
    /// Entity Framework specific configuration.
    /// </summary>
    class EntityMap : EntityTypeConfiguration<Entity>
    {
        /// <summary>
        /// Initializes an instance of the <see cref="EntityMap"/> class, where almost all the mapping happens.
        /// </summary>
        public EntityMap()
        {
            var i = 1;

            Map(e =>
                {
                    e.MapInheritedProperties();
                    e.ToTable(nameof(Entity), "Test");
                });

            Property(e => e.RepositoryId)
                .HasColumnOrder(i++)
                .HasMaxLength(64)
                ;

            Property(e => e.UniqueId)
                .HasColumnOrder(i++)
                ;

            Property(e => e.CreatedOn)
                .HasColumnOrder(i++)
                ;

            Property(e => e.UpdatedOn)
                .HasColumnOrder(i++)
                ;

            Property(e => e.ConcurrencyStamp)
                .IsRowVersion()
                ;

            HasMany(e => e.ValuesList)
                .WithRequired(v => v.Entity)
                .HasForeignKey(v => v.EntityId)
                .WillCascadeOnDelete(true)
                ;
        }
    }

}
