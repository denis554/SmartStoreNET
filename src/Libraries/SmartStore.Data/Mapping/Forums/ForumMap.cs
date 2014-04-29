﻿using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Forums;

namespace SmartStore.Data.Mapping.Forums
{
    public partial class ForumMap : EntityTypeConfiguration<Forum>
    {
        public ForumMap()
        {
            this.ToTable("Forums_Forum");
            this.HasKey(f => f.Id);
            this.Property(f => f.Name).IsRequired().HasMaxLength(200);
			this.Property(f => f.Description).IsMaxLength();
            
            this.HasRequired(f => f.ForumGroup)
                .WithMany(fg => fg.Forums)
                .HasForeignKey(f => f.ForumGroupId);
        }
    }
}
