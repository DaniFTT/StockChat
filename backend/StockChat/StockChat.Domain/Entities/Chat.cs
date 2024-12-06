using StockChat.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Domain.Entities;

public class Chat : BaseEntity
{
    public string ChatName { get; set; } = string.Empty;
}

