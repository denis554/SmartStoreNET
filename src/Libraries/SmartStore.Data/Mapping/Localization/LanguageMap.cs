using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Data.Mapping.Localization
{
    public partial class LanguageMap : EntityTypeConfiguration<Language>
    {
        public LanguageMap()
        {
            this.ToTable("Language");
            this.HasKey(l => l.Id);
            this.Property(l => l.Name).IsRequired().HasMaxLength(100);
            this.Property(l => l.LanguageCulture).IsRequired().HasMaxLength(20);
            this.Property(l => l.UniqueSeoCode).HasMaxLength(2);
            this.Property(l => l.FlagImageFileName).HasMaxLength(50);
        
        }
    }
}