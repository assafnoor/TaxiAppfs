// TaksiApp.Auth.Domain/Exceptions/RoleDomainException.cs
namespace TaksiApp.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when Role entity business rules are violated.
/// </summary>
/// <remarks>
/// This exception is specific to Role entity operations and provides
/// predefined factory methods for common role-related domain errors.
/// </remarks>
public sealed class RoleDomainException : DomainException
{
    /// <summary>
    /// The ID of the role involved in the exception, if applicable.
    /// </summary>
    public Guid? RoleId { get; }

    private RoleDomainException(string errorCode, string message, Guid? roleId = null)
        : base(errorCode, message)
    {
        RoleId = roleId;
    }

    private RoleDomainException(string errorCode, string message, Guid? roleId, IReadOnlyDictionary<string, object>? metadata)
        : base(errorCode, message, metadata)
    {
        RoleId = roleId;
    }

    #region Factory Methods

    /// <summary>
    /// Creates an exception for when trying to modify a system role.
    /// </summary>
    public static RoleDomainException CannotModifySystemRole(Guid roleId, string roleName) =>
        new(
            ErrorCodes.CannotModifySystemRole,
            $"System role '{roleName}' cannot be modified. Role ID: {roleId}",
            roleId,
            new Dictionary<string, object> { ["RoleName"] = roleName });

    /// <summary>
    /// Creates an exception for when trying to delete a system role.
    /// </summary>
    public static RoleDomainException CannotDeleteSystemRole(Guid roleId, string roleName) =>
        new(
            ErrorCodes.CannotDeleteSystemRole,
            $"System role '{roleName}' cannot be deleted. Role ID: {roleId}",
            roleId,
            new Dictionary<string, object> { ["RoleName"] = roleName });

    /// <summary>
    /// Creates an exception for when role name is invalid.
    /// </summary>
    public static RoleDomainException InvalidRoleName() =>
        new(
            ErrorCodes.InvalidRoleName,
            "Role name cannot be null or empty");

    /// <summary>
    /// Creates an exception for when a role with the specified name already exists.
    /// </summary>
    public static RoleDomainException RoleNameAlreadyExists(string roleName) =>
        new(
            ErrorCodes.RoleNameAlreadyExists,
            $"A role with name '{roleName}' already exists",
            null,
            new Dictionary<string, object> { ["RoleName"] = roleName });

    /// <summary>
    /// Creates an exception for when a role is not found.
    /// </summary>
    public static RoleDomainException RoleNotFound(Guid roleId) =>
        new(
            ErrorCodes.RoleNotFound,
            $"Role not found. Role ID: {roleId}",
            roleId);

    /// <summary>
    /// Creates an exception for when a role is not found by name.
    /// </summary>
    public static RoleDomainException RoleNotFoundByName(string roleName) =>
        new(
            ErrorCodes.RoleNotFound,
            $"Role not found. Role Name: {roleName}",
            null,
            new Dictionary<string, object> { ["RoleName"] = roleName });

    #endregion

    /// <summary>
    /// Error codes for Role domain exceptions.
    /// </summary>
    public static class ErrorCodes
    {
        public const string CannotModifySystemRole = "Role.CannotModifySystemRole";
        public const string CannotDeleteSystemRole = "Role.CannotDeleteSystemRole";
        public const string InvalidRoleName = "Role.InvalidRoleName";
        public const string RoleNameAlreadyExists = "Role.NameAlreadyExists";
        public const string RoleNotFound = "Role.NotFound";
    }
}
