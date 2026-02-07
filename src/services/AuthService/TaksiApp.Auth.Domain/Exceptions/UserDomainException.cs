// TaksiApp.Auth.Domain/Exceptions/UserDomainException.cs
namespace TaksiApp.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when User aggregate business rules are violated.
/// </summary>
/// <remarks>
/// This exception is specific to User aggregate operations and provides
/// predefined factory methods for common user-related domain errors.
/// </remarks>
public sealed class UserDomainException : DomainException
{
    /// <summary>
    /// The ID of the user involved in the exception, if applicable.
    /// </summary>
    public Guid? UserId { get; }

    private UserDomainException(string errorCode, string message, Guid? userId = null)
        : base(errorCode, message)
    {
        UserId = userId;
    }

    private UserDomainException(string errorCode, string message, Guid? userId, IReadOnlyDictionary<string, object>? metadata)
        : base(errorCode, message, metadata)
    {
        UserId = userId;
    }

    #region Factory Methods - Account Status

    /// <summary>
    /// Creates an exception for when an operation is attempted on an inactive user account.
    /// </summary>
    public static UserDomainException UserNotActive(Guid userId) =>
        new(
            ErrorCodes.UserNotActive,
            $"User account is not active. User ID: {userId}",
            userId);

    /// <summary>
    /// Creates an exception for when an operation is attempted on a locked user account.
    /// </summary>
    public static UserDomainException UserLocked(Guid userId) =>
        new(
            ErrorCodes.UserLocked,
            $"User account is locked. User ID: {userId}",
            userId);

    /// <summary>
    /// Creates an exception for when an operation requires email verification but the user is not verified.
    /// </summary>
    public static UserDomainException UserNotVerified(Guid userId) =>
        new(
            ErrorCodes.UserNotVerified,
            $"User email is not verified. User ID: {userId}",
            userId);

    /// <summary>
    /// Creates an exception for when trying to lock an already locked account.
    /// </summary>
    public static UserDomainException UserAlreadyLocked(Guid userId) =>
        new(
            ErrorCodes.UserAlreadyLocked,
            $"User account is already locked. User ID: {userId}",
            userId);

    /// <summary>
    /// Creates an exception for when trying to unlock an account that is not locked.
    /// </summary>
    public static UserDomainException UserNotLocked(Guid userId) =>
        new(
            ErrorCodes.UserNotLocked,
            $"User account is not locked. User ID: {userId}",
            userId);

    /// <summary>
    /// Creates an exception for when trying to verify an already verified email.
    /// </summary>
    public static UserDomainException EmailAlreadyVerified(Guid userId) =>
        new(
            ErrorCodes.EmailAlreadyVerified,
            $"User email is already verified. User ID: {userId}",
            userId);

    /// <summary>
    /// Creates an exception for when trying to lock an unverified account.
    /// </summary>
    public static UserDomainException CannotLockUnverifiedAccount(Guid userId) =>
        new(
            ErrorCodes.CannotLockUnverifiedAccount,
            $"Cannot lock an unverified user account. Verify email first. User ID: {userId}",
            userId);

    #endregion

    #region Factory Methods - Role Management

    /// <summary>
    /// Creates an exception for when trying to add a duplicate role to a user.
    /// </summary>
    public static UserDomainException DuplicateRole(Guid userId, string roleName) =>
        new(
            ErrorCodes.DuplicateRole,
            $"User already has role '{roleName}'. User ID: {userId}",
            userId,
            new Dictionary<string, object> { ["RoleName"] = roleName });

    /// <summary>
    /// Creates an exception for when trying to remove a role that the user doesn't have.
    /// </summary>
    public static UserDomainException RoleNotAssigned(Guid userId, Guid roleId) =>
        new(
            ErrorCodes.RoleNotAssigned,
            $"User does not have the specified role. User ID: {userId}, Role ID: {roleId}",
            userId,
            new Dictionary<string, object> { ["RoleId"] = roleId });

    #endregion

    #region Factory Methods - Password

    /// <summary>
    /// Creates an exception for when password hash is empty or invalid.
    /// </summary>
    public static UserDomainException InvalidPasswordHash() =>
        new(
            ErrorCodes.InvalidPasswordHash,
            "Password hash cannot be null or empty");

    /// <summary>
    /// Creates an exception for when password change is attempted on a locked account.
    /// </summary>
    public static UserDomainException CannotChangePasswordWhileLocked(Guid userId) =>
        new(
            ErrorCodes.CannotChangePasswordWhileLocked,
            $"Cannot change password while account is locked. User ID: {userId}",
            userId);

    #endregion

    #region Factory Methods - Profile

    /// <summary>
    /// Creates an exception for when profile update is attempted on an inactive account.
    /// </summary>
    public static UserDomainException CannotUpdateInactiveProfile(Guid userId) =>
        new(
            ErrorCodes.CannotUpdateInactiveProfile,
            $"Cannot update profile of an inactive user. User ID: {userId}",
            userId);

    /// <summary>
    /// Creates an exception for when name validation fails.
    /// </summary>
    public static UserDomainException InvalidName(string fieldName) =>
        new(
            ErrorCodes.InvalidName,
            $"{fieldName} cannot be null or empty",
            null,
            new Dictionary<string, object> { ["FieldName"] = fieldName });

    #endregion

    #region Factory Methods - General

    /// <summary>
    /// Creates an exception for when a user is not found.
    /// </summary>
    public static UserDomainException UserNotFound(Guid userId) =>
        new(
            ErrorCodes.UserNotFound,
            $"User not found. User ID: {userId}",
            userId);

    /// <summary>
    /// Creates an exception for when a user with the specified email already exists.
    /// </summary>
    public static UserDomainException EmailAlreadyExists(string email) =>
        new(
            ErrorCodes.EmailAlreadyExists,
            $"A user with email '{email}' already exists",
            null,
            new Dictionary<string, object> { ["Email"] = email });

    #endregion

    /// <summary>
    /// Error codes for User domain exceptions.
    /// </summary>
    public static class ErrorCodes
    {
        // Account Status
        public const string UserNotActive = "User.NotActive";
        public const string UserLocked = "User.Locked";
        public const string UserNotVerified = "User.NotVerified";
        public const string UserAlreadyLocked = "User.AlreadyLocked";
        public const string UserNotLocked = "User.NotLocked";
        public const string EmailAlreadyVerified = "User.EmailAlreadyVerified";
        public const string CannotLockUnverifiedAccount = "User.CannotLockUnverifiedAccount";

        // Role Management
        public const string DuplicateRole = "User.DuplicateRole";
        public const string RoleNotAssigned = "User.RoleNotAssigned";

        // Password
        public const string InvalidPasswordHash = "User.InvalidPasswordHash";
        public const string CannotChangePasswordWhileLocked = "User.CannotChangePasswordWhileLocked";

        // Profile
        public const string CannotUpdateInactiveProfile = "User.CannotUpdateInactiveProfile";
        public const string InvalidName = "User.InvalidName";

        // General
        public const string UserNotFound = "User.NotFound";
        public const string EmailAlreadyExists = "User.EmailAlreadyExists";
    }
}
