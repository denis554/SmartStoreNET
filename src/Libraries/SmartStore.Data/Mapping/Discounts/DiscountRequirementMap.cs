using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Discounts;

namespace SmartStore.Data.Mapping.Discounts
{
    public partial class DiscountRequirementMap : EntityTypeConfiguration<DiscountRequirement>
    {
        public DiscountRequirementMap()
        {
            this.ToTable("DiscountRequirement");
            this.HasKey(dr => dr.Id);

            this.Property(dr => dr.SpentAmount).HasPrecision(18, 4);
        }
    }
}