using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using StockChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Infrastructure.Configs;


internal class ChatMessageEfConfig : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Text).HasMaxLength(1024);

        builder.HasOne(cm => cm.Chat)
               .WithMany()
               .HasForeignKey(cm => cm.ChatId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
               .WithMany()
               .HasForeignKey(cm => cm.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
