using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace vm.Aspects.Model.EFRepository.HiLoIdentity
{
    class HiLoIdentityGeneratorConfiguration : EntityTypeConfiguration<HiLoIdentityGenerator>
    {
        public HiLoIdentityGeneratorConfiguration()
        {
            ToTable("_HiLoIdentityGenerator", "HiLoIdentity");

            HasKey(g => g.EntitySetName);

            Property(g => g.EntitySetName)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(0)
                .HasMaxLength(HiLoIdentityGenerator.EntitySetNameMaxLength)
                ;

            Property(g => g.HighValue)
                .IsRequired()
                .HasColumnOrder(1)
                ;

            Property(g => g.MaxLowValue)
                .IsRequired()
                .HasColumnOrder(2)
                ;

            Ignore(g => g.LowValue);
        }
    }
}
