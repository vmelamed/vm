using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using vm.Aspects.Model.Tests;

namespace vm.Aspects.Model.EFRepository.Tests
{
    class TestXEntityConfiguration : EntityTypeConfiguration<TestXEntity>
    {
        public TestXEntityConfiguration()
        {
            // TPC: The following call implements Table-per-Class inheritance strategy
            Map(e =>
                {
                    e.MapInheritedProperties();
                    e.ToTable("TestXEntity");
                });

            HasKey(e => e.Id);
            Property(e => e.Id)
                //.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(0)
                ;

            Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnOrder(1)
                ;

            Property(e => e.StringProperty)
                .HasMaxLength(100)
                .IsOptional()
                .HasColumnOrder(2)
                ;

            Property(e => e.Created)
                ;

            Property(e => e.Updated)
                .IsConcurrencyToken()
                ;
        }
    }

    class TestEntityConfiguration : EntityTypeConfiguration<TestEntity>
    {
        public TestEntityConfiguration()
        {
            // TPC: The following call implements Table-per-Class inheritance strategy
            Map(e =>
            {
                e.MapInheritedProperties();
                e.ToTable("TestEntity");
            });

            HasKey(e => e.Id);
            Property(e => e.Id)
                //.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(0)
                ;

            Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnOrder(1)
                ;

            Property(e => e.StringProperty)
                .HasMaxLength(100)
                .IsOptional()
                .HasColumnOrder(2)
                ;

            Property(e => e.Created)
                ;

            Property(e => e.Updated)
                .IsConcurrencyToken()
                ;

            HasOptional(e => e.XEntity)
                .WithMany()
                .HasForeignKey(e => e.XEntityId)
                ;

            HasOptional(e => e.Value)
                .WithOptionalPrincipal(v => v.Entity)
                ;
        }
    }

    class TestEntity1Configuration : EntityTypeConfiguration<TestEntity1>
    {
        public TestEntity1Configuration()
        {
            // TPH: The following call implements Table-per-Hierarchy inheritance strategy
            Map<TestEntity1>(e => e.Requires("Type")
                                     .HasValue(TestEntity1.Discriminator)
                                     .HasMaxLength(30)
                                     .IsFixedLength()
                                     .IsUnicode(false)
                                     .HasColumnOrder(4)
                                     );
        }
    }

    class TestEntity2Configuration : EntityTypeConfiguration<TestEntity2>
    {
        public TestEntity2Configuration()
        {
            // TPH: The following call implements Table-per-Hierarchy inheritance strategy
            Map<TestEntity2>(e => e.Requires("Type")
                                     .HasValue(TestEntity2.Discriminator)
                                     .HasMaxLength(30)
                                     .IsFixedLength()
                                     .IsUnicode(false)
                                     .HasColumnOrder(4)
                                     );

            HasMany(e => e.InternalValues)
                .WithOptional(v => v.Entity2)
                .HasForeignKey(v => v.Entity2Id)
                ;
        }
    }

    class TestValueConfiguration : EntityTypeConfiguration<TestValue>
    {
        public TestValueConfiguration()
        {
            // TPC: The following call implements Table-per-Class inheritance strategy
            Map(e =>
            {
                e.MapInheritedProperties();
                e.ToTable("TestValue");
            });

            HasKey(e => e.Id);
            Property(e => e.Id)
                //.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(0)
                ;

            Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnOrder(1)
                ;

            Property(v => v.Entity2Id)
                ;

            HasOptional(v => v.Entity)
                .WithOptionalDependent(e => e.Value)
                ;

            HasOptional(v => v.Entity2)
                .WithMany(e => e.InternalValues)
                .HasForeignKey(v => v.Entity2Id)
                ;
        }
    }
}
