namespace Identity.Core.Connections;

public interface IConnectionRepository
{
    Task<Connection?> GetByUsersAsync(Guid userId, Guid otherUserId);
    void Add(Connection connection);
}
