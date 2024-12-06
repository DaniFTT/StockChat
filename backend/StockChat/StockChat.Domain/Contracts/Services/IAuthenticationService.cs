using Ardalis.Result;
using StockChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Domain.Contracts.Services;

public interface IAuthenticationService
{ 
    Task<Result<bool>> LoginAsync(string userEmail, string password);
    Task<Result<bool>> RegisterAsync(User user, string password);
    Task LogoutAsync();
}

