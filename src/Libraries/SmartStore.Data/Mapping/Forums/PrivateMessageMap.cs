﻿using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Forums;

namespace SmartStore.Data.Mapping.Forums
{
    public partial class PrivateMessageMap : EntityTypeConfiguration<PrivateMessage>
    {
        public PrivateMessageMap()
        {
            this.ToTable("Forums_PrivateMessage");
            this.HasKey(pm => pm.Id);
            this.Property(pm => pm.Subject).IsRequired().HasMaxLength(450);
            this.Property(pm => pm.Text).IsRequired().IsMaxLength();

            this.HasRequired(pm => pm.FromCustomer)
               .WithMany()
               .HasForeignKey(pm => pm.FromCustomerId)
               .WillCascadeOnDelete(false);

            this.HasRequired(pm => pm.ToCustomer)
               .WithMany()
               .HasForeignKey(pm => pm.ToCustomerId)
               .WillCascadeOnDelete(false);
        }
    }
}
