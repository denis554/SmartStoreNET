﻿using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Forums;

namespace SmartStore.Data.Mapping.Forums
{
    public partial class ForumTopicMap : EntityTypeConfiguration<ForumTopic>
    {
        public ForumTopicMap()
        {
            this.ToTable("Forums_Topic");
            this.HasKey(ft => ft.Id);
            this.Property(ft => ft.Subject).IsRequired().HasMaxLength(450);
            this.Ignore(ft => ft.ForumTopicType);

            this.HasRequired(ft => ft.Forum)
                .WithMany()
                .HasForeignKey(ft => ft.ForumId);

            this.HasRequired(ft => ft.Customer)
               .WithMany(c => c.ForumTopics)
               .HasForeignKey(ft => ft.CustomerId)
               .WillCascadeOnDelete(false);
        }
    }
}
