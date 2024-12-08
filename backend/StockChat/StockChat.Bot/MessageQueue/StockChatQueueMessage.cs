using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Bot.MessageQueue;

public class StockChatQueueMessage
{
    public string UserId { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
}

