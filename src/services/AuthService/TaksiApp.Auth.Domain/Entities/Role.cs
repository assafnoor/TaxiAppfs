// TaksiApp.Auth.Domain/Entities/Role.cs
using TaksiApp.Auth.Domain.Exceptions;
using TaksiApp.Shared.Kernel.Common;

namespace TaksiApp.Auth.Domain.Entities;

/// <summary>
/// Represents a role that can be assigned to users.
/// </summary>
/// <remarks>
/// <para>
/// Role is an entity within the User aggregate boundary. It encapsulates
/// role-specific business rules and operations.
/// </para>
/// <para>
/// Invariants:
/// - Role name cannot be empty
/// - System roles cannot be modified or deleted
/// </para>
/// </remarks>
public sealed class Role : Entity<Guid>
{
    private readonly List<UserRole> _userRoles = new();
    private readonly List<RolePermission> _permissions = new();

    /// <summary>
    /// The name of the role. Must be unique and non-empty.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Optional description of what this role represents.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Indicates whether this is a system-defined role.
    /// System roles cannot be modified or deleted.
    /// </summary>
    public bool IsSystemRole { get; private set; }

    /// <summary>
    /// When the role was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// When the role was last updated, if ever.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Collection of users assigned to this role.
    /// </summary>
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    /// <summary>
    /// Collection of permissions assigned to this role.
    /// </summary>
    public IReadOnlyList<RolePermission> Permissions => _permissions.AsReadOnly();

    /// <summary>
    /// Parameterless constructor for ORM frameworks (EF Core).
    /// </summary>
    private Role() { }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="id">The unique identifier for the role</param>
    /// <param name="name">The name of the role (must be non-empty)</param>
    /// <param name="description">Optional description of the role</param>
    /// <param name="isSystemRole">Whether this is a system-defined role</param>
    private Role(Guid id, string name, string description, bool isSystemRole)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw RoleDomainException.InvalidRoleName();

        Id = id;
        Name = name;
        Description = description ?? string.Empty;
        IsSystemRole = isSystemRole;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new user-defined role.
    /// </summary>
    /// <param name="name">The name of the role (must be non-empty)</param>
    /// <param name="description">Optional description of the role</param>
    /// <returns>A new Role instance</returns>
    public static Role Create(string name, string? description = null)
    {
        return new Role(Guid.NewGuid(), name, description ?? string.Empty, isSystemRole: false);
    }

    /// <summary>
    /// Updates the name of the role.
    /// </summary>
    /// <param name="newName">The new name for the role (must be non-empty)</param>
    /// <exception cref="RoleDomainException">
    /// Thrown when:
    /// - The role is a system role
    /// - The new name is empty
    /// </exception>
    public void UpdateName(string newName)
    {
        if (IsSystemRole)
            throw RoleDomainException.CannotModifySystemRole(Id, Name);

        if (string.IsNullOrWhiteSpace(newName))
            throw RoleDomainException.InvalidRoleName();

        Name = newName;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the role details (name and description).
    /// </summary>
    /// <param name="name">The new name for the role</param>
    /// <param name="description">The new description for the role</param>
    /// <exception cref="RoleDomainException">
    /// Thrown when the role is a system role
    /// </exception>
    public void UpdateDetails(string name, string? description)
    {
        if (IsSystemRole)
            throw RoleDomainException.CannotModifySystemRole(Id, Name);

        if (string.IsNullOrWhiteSpace(name))
            throw RoleDomainException.InvalidRoleName();

        Name = name;
        Description = description ?? string.Empty;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a permission to this role.
    /// </summary>
    /// <param name="permission">The permission to add</param>
    public void AddPermission(Permission permission)
    {
        if (!_permissions.Any(p => p.PermissionId == permission.Id))
        {
            _permissions.Add(new RolePermission(Id, permission.Id));
        }
    }

    /// <summary>
    /// Removes a permission from this role.
    /// </summary>
    /// <param name="permissionId">The ID of the permission to remove</param>
    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _permissions.Remove(rolePermission);
        }
    }

    /// <summary>
    /// Checks if this role has a specific permission.
    /// </summary>
    /// <param name="permissionName">The name of the permission to check</param>
    /// <returns>True if the role has the permission</returns>
    public bool HasPermission(string permissionName)
    {
        return _permissions.Any(p =>
            p.Permission?.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase) == true);
    }

    #region System Role Factory Methods

    /// <summary>
    /// Creates the predefined Administrator system role.
    /// </summary>
    public static Role CreateAdminRole() =>
        new Role(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Admin",
            "Full system administrator with all permissions",
            isSystemRole: true);

    /// <summary>
    /// Creates the predefined User system role.
    /// </summary>
    public static Role CreateUserRole() =>
        new Role(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "User",
            "Standard user with basic permissions",
            isSystemRole: true);

    /// <summary>
    /// Creates the predefined Moderator system role.
    /// </summary>
    public static Role CreateModeratorRole() =>
        new Role(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Moderator",
            "Content moderator with limited administrative permissions",
            isSystemRole: true);

    /// <summary>
    /// Creates the predefined Guest system role.
    /// </summary>
    public static Role CreateGuestRole() =>
        new Role(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Guest",
            "Guest user with minimal permissions",
            isSystemRole: true);

    #endregion
}
