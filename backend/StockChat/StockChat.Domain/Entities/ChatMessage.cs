﻿using StockChat.Domain.Abstractions;
using StockChat.Domain.Enums;

namespace StockChat.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatId { get; set; }
    public Chat? Chat { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Text { get; set; } = string.Empty;
    public UserType UserType { get; set; }
}

