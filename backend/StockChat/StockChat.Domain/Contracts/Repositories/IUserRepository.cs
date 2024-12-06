using Ardalis.Result;
using StockChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Domain.Contracts.Repositories;
public interface IUserRepository
{
    Task<Result<User?>> GetByIdAsync(Guid id);
    Task<Result<User?>> GetByEmailAsync(string username);
    Task<Result> CreateUserAsync(User user, string password);
    Task<Result> UpdateUserAsync(User user);
}
