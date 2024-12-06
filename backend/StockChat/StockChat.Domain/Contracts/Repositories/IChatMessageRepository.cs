using StockChat.Domain.Entities;

namespace StockChat.Domain.Contracts.Repositories;

public interface IChatMessageRepository : IBaseRepository<ChatMessage>
{
    Task<IEnumerable<ChatMessage>> GetLastMessagesAsync(Guid chatId, int count);
}

