using TaksiApp.Shared.Kernel.Common;

namespace TaksiApp.Auth.Domain.Entities;

/// <summary>
/// Represents a permission that can be assigned to roles.
/// </summary>
public sealed class Permission : Entity<Guid>
{
    private readonly List<RolePermission> _rolePermissions = new();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    // Private constructor for EF Core
    private Permission() { }

    private Permission(Guid id, string name, string description, string category)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Permission Create(string name, string description, string category)
    {
        return new Permission(Guid.NewGuid(), name, description, category);
    }

    // Common permissions factory methods
    public static class Categories
    {
        public const string User = "User";
        public const string Role = "Role";
        public const string Permission = "Permission";
        public const string System = "System";
    }

    public static Permission CreateUserRead() => Create("user.read", "Read user information", Categories.User);
    public static Permission CreateUserWrite() => Create("user.write", "Create and update users", Categories.User);
    public static Permission CreateUserDelete() => Create("user.delete", "Delete users", Categories.User);
    public static Permission CreateRoleManage() => Create("role.manage", "Manage roles", Categories.Role);
    public static Permission CreatePermissionManage() => Create("permission.manage", "Manage permissions", Categories.Permission);
}