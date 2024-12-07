using Ardalis.Result;
using StockChat.Domain.Dtos;
using StockChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Domain.Contracts.Services;

public interface IAuthService
{ 
    Task<Result<UserDto>> LoginAsync(string userEmail, string password);
    Task<Result<UserDto>> RegisterAsync(User user, string password);
    Task<Result<UserDto>> GetCurrentUser();
    Task LogoutAsync();
}

