using Identity.Core.Connections;
using Microsoft.AspNetCore.Identity;
using Shared.Domain.Exceptions;

namespace Identity.Core.Following;

public sealed class FollowingService
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public FollowingService(IConnectionRepository connectionRepository, IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task SendRequestAsync(Guid senderId, Guid recipientId)
    {
        await EnsureExplorerExistsAsync(recipientId);

        var connection = await _connectionRepository.GetByUsersAsync(senderId, recipientId);
        if (connection is null)
        {
            connection = new Connection(senderId, recipientId);
            _connectionRepository.Add(connection);
        }

        connection.SendRequest(senderId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptRequestAsync(Guid recipientId, Guid senderId)
    {
        var connection = await GetConnectionAsync(recipientId, senderId);

        connection.Accept(recipientId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectRequestAsync(Guid recipientId, Guid senderId)
    {
        var connection = await GetConnectionAsync(recipientId, senderId);

        connection.Reject(recipientId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawRequestAsync(Guid senderId, Guid recipientId)
    {
        var connection = await GetConnectionAsync(senderId, recipientId);

        connection.Withdraw(senderId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFollowerAsync(Guid userId, Guid followerId)
    {
        var connection = await GetConnectionAsync(userId, followerId);

        connection.RemoveFollower(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnfollowAsync(Guid userId, Guid followedId)
    {
        var connection = await GetConnectionAsync(userId, followedId);

        connection.Unfollow(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    // Only explorers take part in following, so any other ID names no one who can be followed.
    private async Task EnsureExplorerExistsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !await _userManager.IsInRoleAsync(user, "explorer"))
        {
            throw new NotFoundException($"There is no explorer with ID {userId}.");
        }
    }

    private async Task<Connection> GetConnectionAsync(Guid userId, Guid otherUserId)
    {
        var connection = await _connectionRepository.GetByUsersAsync(userId, otherUserId);
        if (connection is null)
        {
            throw new NotFoundException($"There is no follow request between users {userId} and {otherUserId}.");
        }
        return connection;
    }
}
