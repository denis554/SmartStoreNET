using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Data.Mapping.Directory
{
    public partial class CountryMap : EntityTypeConfiguration<Country>
    {
        public CountryMap()
        {
            this.ToTable("Country");
            this.HasKey(c =>c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(100);
            this.Property(c =>c.TwoLetterIsoCode).HasMaxLength(2);
            this.Property(c =>c.ThreeLetterIsoCode).HasMaxLength(3);
        }
    }
}