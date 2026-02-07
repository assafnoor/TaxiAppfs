// TaksiApp.Auth.Domain/Repositories/IRoleRepository.cs
using TaksiApp.Auth.Domain.Entities;

namespace TaksiApp.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for Role entity operations.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for role persistence operations.
/// Implementations should handle the actual database interactions.
/// </para>
/// <para>
/// The repository is part of the Domain Layer and depends only on
/// domain entities. Infrastructure concerns should be in the Infrastructure Layer.
/// </para>
/// </remarks>
public interface IRoleRepository
{
    /// <summary>
    /// Gets a role by its unique identifier.
    /// </summary>
    /// <param name="id">The role's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The role if found, null otherwise</returns>
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="name">The role name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The role if found, null otherwise</returns>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role exists with the specified name.
    /// </summary>
    /// <param name="name">The role name to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a role with this name exists</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new role to the repository.
    /// </summary>
    /// <param name="role">The role to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added role</returns>
    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role in the repository.
    /// </summary>
    /// <param name="role">The role to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated role</returns>
    Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from the repository.
    /// </summary>
    /// <param name="id">The role's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the role was removed, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all system roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of system roles</returns>
    Task<IReadOnlyList<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all user-defined (non-system) roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user-defined roles</returns>
    Task<IReadOnlyList<Role>> GetUserRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all roles</returns>
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
}
