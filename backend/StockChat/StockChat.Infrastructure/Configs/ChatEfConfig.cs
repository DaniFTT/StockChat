using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockChat.Domain.Entities;

namespace StockChat.Infrastructure.Configs;

internal class ChatEfConfig : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ChatName).HasMaxLength(150);
        builder.Property(c => c.CreatedBy).HasMaxLength(150);
    }
}

