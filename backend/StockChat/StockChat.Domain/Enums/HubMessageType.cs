﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Domain.Enums;

public enum HubMessageType
{
    NewMessage,
    NewChat,
    GetLastMessages,
    Notification,
}

