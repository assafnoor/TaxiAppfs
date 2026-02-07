using TaksiApp.Shared.Kernel.Common;

namespace TaksiApp.Auth.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between Users and Roles.
/// </summary>
public sealed class UserRole : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }

    public User User { get; private set; }
    public Role Role { get; private set; }

    // Private constructor for EF Core
    private UserRole() { }

    public UserRole(Guid userId, Guid roleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RoleId = roleId;
        AssignedAtUtc = DateTime.UtcNow;
    }
}

/// <summary>
/// Represents the many-to-many relationship between Roles and Permissions.
/// </summary>
public sealed class RolePermission : Entity<Guid>
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }

    public Role Role { get; private set; }
    public Permission Permission { get; private set; }

    // Private constructor for EF Core
    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        Id = Guid.NewGuid();
        RoleId = roleId;
        PermissionId = permissionId;
        AssignedAtUtc = DateTime.UtcNow;
    }
}