using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using StockChat.Domain.Contracts.Repositories;
using StockChat.Domain.Entities;

namespace StockChat.Infrastructure.Repositories;
public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<User?>> GetByIdAsync(Guid id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<Result<User?>> GetByEmailAsync(string username)
    {
        var result = await _userManager.FindByEmailAsync(username);
        if (result is null)
            return Result.NotFound();

        return Result.Success(result)!;
    }

    public async Task<Result> CreateUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return Result.Error(new ErrorList(result.Errors.Select(e => e.Description)));     

        return Result.Success();
    }

    public async Task<Result> UpdateUserAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result.Error("Falha ao atualizar usuário: " +
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return Result.Success();
    }
}