using StockChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Domain.Contracts.Repositories;

public interface IChatRepository : IBaseRepository<Chat>
{
}

