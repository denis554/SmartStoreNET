using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Data.Mapping.Messages
{
    public partial class MessageTemplateMap : EntityTypeConfiguration<MessageTemplate>
    {
        public MessageTemplateMap()
        {
            this.ToTable("MessageTemplate");
            this.HasKey(mt => mt.Id);

            this.Property(mt => mt.Name).IsRequired().HasMaxLength(200);
			this.Property(mt => mt.Body).IsMaxLength();
            this.Property(mt => mt.BccEmailAddresses).HasMaxLength(200);
            this.Property(mt => mt.Subject).HasMaxLength(1000);
            this.Property(mt => mt.EmailAccountId).IsRequired();
        }
    }
}