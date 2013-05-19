using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductTagMap : EntityTypeConfiguration<ProductTag>
    {
        public ProductTagMap()
        {
            this.ToTable("ProductTag");
            this.HasKey(pt => pt.Id);
            this.Property(pt => pt.Name).IsRequired().HasMaxLength(400);

            this.HasMany(pt => pt.Products)
                .WithMany(p => p.ProductTags)
                .Map(m => m.ToTable("Product_ProductTag_Mapping"));
        }
    }
}