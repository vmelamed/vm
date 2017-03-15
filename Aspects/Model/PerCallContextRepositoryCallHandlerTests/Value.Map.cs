using System.Data.Entity.ModelConfiguration;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    /// <summary>
    /// Maps the <see cref="Value"/> to database elements (tables, columns, etc.).
    /// Entity Framework specific configuration.
    /// </summary>
    class ValueMap : EntityTypeConfiguration<Value>
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ValueMap"/> class, where almost all the mapping happens.
        /// </summary>
        public ValueMap()
        {
            var i = 1;

            Map(e =>
                {
                    e.MapInheritedProperties();
                    e.ToTable(nameof(Value), "Test");
                });

            Property(v => v.Name)
                .HasColumnOrder(i++)
                .HasMaxLength(50)
                ;

            Property(v => v.EntityId)
                .HasColumnOrder(i++)
                ;

            Property(v => v.CreatedOn)
                .HasColumnOrder(i++)
                ;

            Property(v => v.UpdatedOn)
                .HasColumnOrder(i++)
                .IsConcurrencyToken()
                ;

            HasRequired(v => v.Entity)
                .WithMany()
                .HasForeignKey(v => v.EntityId)
                ;
        }
    }
}
