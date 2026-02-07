// TaksiApp.Auth.Domain/Repositories/IUserRepository.cs
using TaksiApp.Auth.Domain.Entities;
using TaksiApp.Shared.Kernel.Abstractions;
using TaksiApp.Shared.Kernel.ValueObjects;

namespace TaksiApp.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate operations.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for user persistence operations.
/// Implementations should handle the actual database interactions.
/// </para>
/// <para>
/// The repository is part of the Domain Layer and depends only on
/// domain entities and value objects. Infrastructure concerns like
/// EF Core, SQL queries, or caching should be in the Infrastructure Layer.
/// </para>
/// <para>
/// All methods return domain entities or value objects, never DTOs or projections.
/// This ensures the domain layer remains pure and testable.
/// </para>
/// </remarks>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address as a string.
    /// </summary>
    /// <param name="email">The email address string to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the specified email.
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a user with this email exists</returns>
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the specified email string.
    /// </summary>
    /// <param name="email">The email address string to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a user with this email exists</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added user</returns>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    /// <param name="user">The user to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user from the repository.
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user was removed, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active users</returns>
    Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by their tenant identifier (for multi-tenant scenarios).
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users in the tenant</returns>
    Task<IReadOnlyList<User>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users with a specific role.
    /// </summary>
    /// <param name="roleName">The role name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users with the specified role</returns>
    Task<IReadOnlyList<User>> GetByRoleNameAsync(string roleName, CancellationToken cancellationToken = default);
}
