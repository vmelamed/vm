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
                    e.ToTable(nameof(Entity));
                });

            Property(e => e.UniqueId)
                .HasColumnOrder(i++)
                ;

            Property(e => e.Name)
                .HasMaxLength(64)
                .HasColumnOrder(i++)
                ;

            Property(e => e.CreatedOn)
                ;

            Property(e => e.UpdatedOn)
                ;

            HasMany(e => e.ValuesList)
                .WithRequired(v => v.Entity)
                .HasForeignKey(v => v.EntityId)
                .WillCascadeOnDelete(true)
                ;
        }
    }

}
