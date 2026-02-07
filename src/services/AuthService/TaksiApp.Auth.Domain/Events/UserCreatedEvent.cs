// TaksiApp.Auth.Domain/Events/UserCreatedEvent.cs
using TaksiApp.Shared.Kernel.Events;

namespace TaksiApp.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a new user is created in the system.
/// </summary>
/// <param name="UserId">The unique identifier of the created user</param>
/// <param name="Email">The email address of the created user</param>
/// <param name="Name">The full name of the created user</param>
public sealed record UserCreatedEvent(
    Guid UserId,
    string Email,
    string Name) : DomainEventBase;

/// <summary>
/// Domain event raised when a user's email is verified.
/// </summary>
/// <param name="UserId">The unique identifier of the user</param>
/// <param name="Email">The verified email address</param>
public sealed record UserEmailVerifiedEvent(
    Guid UserId,
    string Email) : DomainEventBase;

/// <summary>
/// Domain event raised when a user account is locked.
/// </summary>
/// <param name="UserId">The unique identifier of the locked user</param>
/// <param name="Email">The email address of the locked user</param>
/// <param name="Reason">The reason for locking the account</param>
public sealed record UserLockedEvent(
    Guid UserId,
    string Email,
    string? Reason) : DomainEventBase;

/// <summary>
/// Domain event raised when a user account is unlocked.
/// </summary>
/// <param name="UserId">The unique identifier of the unlocked user</param>
/// <param name="Email">The email address of the unlocked user</param>
public sealed record UserUnlockedEvent(
    Guid UserId,
    string Email) : DomainEventBase;

/// <summary>
/// Domain event raised when a user's password is changed.
/// </summary>
/// <param name="UserId">The unique identifier of the user</param>
/// <param name="Email">The email address of the user</param>
public sealed record PasswordChangedEvent(
    Guid UserId,
    string Email) : DomainEventBase;

/// <summary>
/// Domain event raised when a user's profile is updated.
/// </summary>
/// <param name="UserId">The unique identifier of the user</param>
/// <param name="Email">The email address of the user</param>
/// <param name="Name">The updated full name of the user</param>
public sealed record UserProfileUpdatedEvent(
    Guid UserId,
    string Email,
    string Name) : DomainEventBase;

/// <summary>
/// Domain event raised when a user account is deactivated.
/// </summary>
/// <param name="UserId">The unique identifier of the deactivated user</param>
/// <param name="Email">The email address of the deactivated user</param>
public sealed record UserDeactivatedEvent(
    Guid UserId,
    string Email) : DomainEventBase;

/// <summary>
/// Domain event raised when a user account is activated.
/// </summary>
/// <param name="UserId">The unique identifier of the activated user</param>
/// <param name="Email">The email address of the activated user</param>
public sealed record UserActivatedEvent(
    Guid UserId,
    string Email) : DomainEventBase;

/// <summary>
/// Domain event raised when a role is assigned to a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user</param>
/// <param name="RoleId">The unique identifier of the assigned role</param>
/// <param name="RoleName">The name of the assigned role</param>
public sealed record UserRoleAssignedEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName) : DomainEventBase;

/// <summary>
/// Domain event raised when a role is removed from a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user</param>
/// <param name="RoleId">The unique identifier of the removed role</param>
/// <param name="RoleName">The name of the removed role</param>
public sealed record UserRoleRemovedEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName) : DomainEventBase;

/// <summary>
/// Domain event raised when a user logs in.
/// </summary>
/// <param name="UserId">The unique identifier of the user</param>
/// <param name="Email">The email address of the user</param>
/// <param name="IpAddress">The IP address from which the login occurred</param>
public sealed record UserLoggedInEvent(
    Guid UserId,
    string Email,
    string? IpAddress) : DomainEventBase;

/// <summary>
/// Domain event raised when a refresh token is revoked.
/// </summary>
/// <param name="TokenId">The unique identifier of the revoked token</param>
/// <param name="UserId">The unique identifier of the user</param>
/// <param name="RevokedByIp">The IP address that initiated the revocation</param>
public sealed record RefreshTokenRevokedEvent(
    Guid TokenId,
    Guid UserId,
    string? RevokedByIp) : DomainEventBase;
