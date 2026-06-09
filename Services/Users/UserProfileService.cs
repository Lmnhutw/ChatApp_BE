using ChatApp_BE.Data;
using ChatApp_BE.Domain.Entities;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels.ProfileViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApp_BE.Services.Users;

public sealed class UserProfileService : IUserProfileService
{
    private const int MaxSearchPageSize = 50;

    private readonly ChatAppContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserProfileService(ChatAppContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<UserServiceResult> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null
            ? UserServiceResult.NotFound("User not found.")
            : UserServiceResult.Success(ToProfileResponse(user));
    }

    public async Task<UserServiceResult> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return UserServiceResult.NotFound("User not found.");
        }

        user.FullName = request.FullName ?? user.FullName;
        user.DisplayName = request.DisplayName ?? user.DisplayName;
        user.Avatar = request.Avatar ?? user.Avatar;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return UserServiceResult.BadRequest(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        return UserServiceResult.Success(ToProfileResponse(user));
    }

    public async Task<UserServiceResult> SearchUsersAsync(string userId, string query, int take)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return UserServiceResult.BadRequest("Search query is required.");
        }

        var pageSize = Math.Clamp(take, 1, MaxSearchPageSize);
        var searchTerm = query.Trim();

        var blockedUserIds = _context.UserBlocks
            .Where(block => block.BlockerUserId == userId)
            .Select(block => block.BlockedUserId);

        var blockedByUserIds = _context.UserBlocks
            .Where(block => block.BlockedUserId == userId)
            .Select(block => block.BlockerUserId);

        var users = await _context.Users
            .AsNoTracking()
            .Where(user =>
                user.Id != userId &&
                !blockedUserIds.Contains(user.Id) &&
                !blockedByUserIds.Contains(user.Id) &&
                ((user.FullName != null && EF.Functions.Like(user.FullName, $"%{searchTerm}%")) ||
                 (user.DisplayName != null && EF.Functions.Like(user.DisplayName, $"%{searchTerm}%")) ||
                 (user.Email != null && EF.Functions.Like(user.Email, $"%{searchTerm}%"))))
            .OrderBy(user => user.FullName ?? user.DisplayName ?? user.Email)
            .Take(pageSize)
            .Select(user => ToProfileResponse(user))
            .ToListAsync();

        return UserServiceResult.Success(users);
    }

    public async Task<UserServiceResult> GetBlockedUsersAsync(string userId)
    {
        var blockedUsers = await _context.UserBlocks
            .AsNoTracking()
            .Where(block => block.BlockerUserId == userId)
            .Include(block => block.BlockedUser)
            .OrderByDescending(block => block.CreatedAt)
            .Select(block => ToBlockResponse(block))
            .ToListAsync();

        return UserServiceResult.Success(blockedUsers);
    }

    public async Task<UserServiceResult> BlockUserAsync(string userId, BlockUserRequest request)
    {
        if (request.UserId == userId)
        {
            return UserServiceResult.BadRequest("Cannot block yourself.");
        }

        var targetUserExists = await _context.Users.AnyAsync(user => user.Id == request.UserId);
        if (!targetUserExists)
        {
            return UserServiceResult.NotFound("User not found.");
        }

        var existingBlock = await _context.UserBlocks.FindAsync(userId, request.UserId);
        if (existingBlock is not null)
        {
            return UserServiceResult.Success(new { Message = "User blocked." });
        }

        _context.UserBlocks.Add(new UserBlock
        {
            BlockerUserId = userId,
            BlockedUserId = request.UserId
        });

        await _context.SaveChangesAsync();

        return UserServiceResult.Success(new { Message = "User blocked." });
    }

    public async Task<UserServiceResult> UnblockUserAsync(string userId, string blockedUserId)
    {
        var existingBlock = await _context.UserBlocks.FindAsync(userId, blockedUserId);
        if (existingBlock is null)
        {
            return UserServiceResult.NotFound("Blocked user not found.");
        }

        _context.UserBlocks.Remove(existingBlock);
        await _context.SaveChangesAsync();

        return UserServiceResult.Success(new { Message = "User unblocked." });
    }

    private static UserProfileResponse ToProfileResponse(ApplicationUser user)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FullName = user.FullName,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar
        };
    }

    private static UserBlockResponse ToBlockResponse(UserBlock block)
    {
        return new UserBlockResponse
        {
            BlockedUserId = block.BlockedUserId,
            FullName = block.BlockedUser.FullName ?? block.BlockedUser.DisplayName,
            Avatar = block.BlockedUser.Avatar,
            CreatedAt = block.CreatedAt
        };
    }
}
