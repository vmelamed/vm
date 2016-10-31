using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace vm.Aspects.Model.EFRepository.HiLoIdentity
{
    class HiLoIdentityGeneratorConfiguration : EntityTypeConfiguration<HiLoIdentityGenerator>
    {
        public HiLoIdentityGeneratorConfiguration()
        {
            var i = 0;

            ToTable("_HiLoIdentityGenerator", "HiLoIdentity");

            HasKey(g => g.EntitySetName);

            Property(g => g.EntitySetName)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(i++)
                .HasMaxLength(HiLoIdentityGenerator.EntitySetNameMaxLength)
                ;

            Property(g => g.HighValue)
                .HasColumnOrder(i++)
                ;

            Property(g => g.MaxLowValue)
                .HasColumnOrder(i++)
                ;

            Ignore(g => g.LowValue);
        }
    }
}
