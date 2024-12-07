using Microsoft.EntityFrameworkCore;
using StockChat.Domain.Contracts.Repositories;
using StockChat.Domain.Entities;
using StockChat.Infrastructure.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Infrastructure.Repositories;
public class ChatMessageRepository : BaseRepository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChatMessage>> GetLastMessagesAsync(Guid chatId, int count)
    {
        return await _dbSet
            .Include(u => u.User)
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}