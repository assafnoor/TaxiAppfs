using TaksiApp.Shared.Kernel.Common;

namespace TaksiApp.Auth.Domain.Entities;

/// <summary>
/// Represents a role that can be assigned to users.
/// </summary>
public sealed class Role : Entity<Guid>
{
    private readonly List<UserRole> _userRoles = new();
    private readonly List<RolePermission> _permissions = new();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyList<RolePermission> Permissions => _permissions.AsReadOnly();

    // Private constructor for EF Core
    private Role() { }

    private Role(Guid id, string name, string description, bool isSystemRole = false)
    {
        Id = id;
        Name = name;
        Description = description;
        IsSystemRole = isSystemRole;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Role Create(string name, string description, bool isSystemRole = false)
    {
        return new Role(Guid.NewGuid(), name, description, isSystemRole);
    }

    public void UpdateDetails(string name, string description)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("System roles cannot be modified");

        Name = name;
        Description = description;
    }

    public void AddPermission(Permission permission)
    {
        if (!_permissions.Any(p => p.PermissionId == permission.Id))
        {
            _permissions.Add(new RolePermission(Id, permission.Id));
        }
    }

    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _permissions.Remove(rolePermission);
        }
    }

    public bool HasPermission(string permissionName)
    {
        return _permissions.Any(p => p.Permission?.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase) == true);
    }

    // System roles factory methods
    public static Role CreateAdminRole() => Create("Admin", "System Administrator", true);
    public static Role CreateUserRole() => Create("User", "Standard User", true);
    public static Role CreateModeratorRole() => Create("Moderator", "Content Moderator", true);
}