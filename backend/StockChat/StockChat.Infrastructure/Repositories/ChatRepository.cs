using StockChat.Domain.Contracts.Repositories;
using StockChat.Domain.Entities;
using StockChat.Infrastructure.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Infrastructure.Repositories;
public class ChatRepository : BaseRepository<Chat>, IChatRepository
{
    public ChatRepository(ApplicationDbContext context) : base(context)
    {
    }
}