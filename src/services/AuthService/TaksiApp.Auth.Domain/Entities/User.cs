// TaksiApp.Auth.Domain/Entities/User.cs
using TaksiApp.Auth.Domain.Events;
using TaksiApp.Auth.Domain.Exceptions;
using TaksiApp.Shared.Kernel.Abstractions;
using TaksiApp.Shared.Kernel.Common;
using TaksiApp.Shared.Kernel.ValueObjects;

namespace TaksiApp.Auth.Domain.Entities;

/// <summary>
/// Represents a user in the authentication system.
/// </summary>
/// <remarks>
/// <para>
/// User is the aggregate root for the authentication domain. It encapsulates
/// all user-related business rules and operations.
/// </para>
/// <para>
/// Aggregate Invariants:
/// - Email must be valid (enforced by Email Value Object)
/// - Password hash must never be empty or null
/// - Roles collection cannot contain duplicates
/// - Locking requires the account to be verified first
/// </para>
/// <para>
/// Account Status Flags:
/// - IsActive: Controls whether the account can be used at all
/// - IsVerified: Controls whether email verification is complete
/// - IsLocked: Controls whether the account is temporarily disabled
/// </para>
/// </remarks>
public sealed class User : AggregateRoot<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = new();
    private readonly List<UserRole> _roles = new();

    /// <summary>
    /// The user's email address as a validated value object.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// The hashed password. Never store plain text passwords.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// The user's full name (FirstName + LastName).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Optional phone number.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Indicates whether the user account is active.
    /// Inactive accounts cannot log in or perform most operations.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Indicates whether the user's email has been verified.
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Indicates whether the user account is locked.
    /// Locked accounts cannot log in until unlocked.
    /// </summary>
    public bool IsLocked { get; private set; }

    /// <summary>
    /// Reason for locking the account, if locked.
    /// </summary>
    public string? LockReason { get; private set; }

    /// <summary>
    /// When the user account was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// When the user last logged in.
    /// </summary>
    public DateTime? LastLoginAtUtc { get; private set; }

    /// <summary>
    /// Optional tenant identifier for multi-tenant scenarios.
    /// </summary>
    public string? TenantId { get; private set; }

    /// <summary>
    /// Collection of refresh tokens associated with this user.
    /// </summary>
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>
    /// Collection of roles assigned to this user.
    /// </summary>
    public IReadOnlyList<UserRole> Roles => _roles.AsReadOnly();

    /// <summary>
    /// Parameterless constructor for ORM frameworks (EF Core).
    /// </summary>
    private User() { }

    /// <summary>
    /// Creates a new user with the specified details.
    /// </summary>
    /// <param name="id">The unique identifier for the user</param>
    /// <param name="email">The user's email address</param>
    /// <param name="passwordHash">The hashed password (must not be empty)</param>
    /// <param name="firstName">The user's first name</param>
    /// <param name="lastName">The user's last name</param>
    /// <param name="phoneNumber">Optional phone number</param>
    /// <param name="tenantId">Optional tenant identifier</param>
    private User(
        Guid id,
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phoneNumber = null,
        string? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw UserDomainException.InvalidPasswordHash();

        if (string.IsNullOrWhiteSpace(firstName))
            throw UserDomainException.InvalidName(nameof(FirstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw UserDomainException.InvalidName(nameof(LastName));

        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        TenantId = tenantId;
        IsActive = true;
        IsVerified = false;
        IsLocked = false;
        CreatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new UserCreatedEvent(Id, Email.Value, FullName));
    }

    /// <summary>
    /// Factory method to create a new user.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="passwordHash">The hashed password</param>
    /// <param name="firstName">The user's first name</param>
    /// <param name="lastName">The user's last name</param>
    /// <param name="phoneNumber">Optional phone number</param>
    /// <param name="tenantId">Optional tenant identifier</param>
    /// <returns>A new User instance</returns>
    public static User Create(
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phoneNumber = null,
        string? tenantId = null)
    {
        return new User(
            Guid.NewGuid(),
            email,
            passwordHash,
            firstName,
            lastName,
            phoneNumber,
            tenantId);
    }

    #region Profile Management

    /// <summary>
    /// Updates the user's profile (name and/or phone number).
    /// </summary>
    /// <param name="firstName">New first name (optional)</param>
    /// <param name="lastName">New last name (optional)</param>
    /// <param name="phoneNumber">New phone number (optional)</param>
    /// <exception cref="UserDomainException">
    /// Thrown when the user account is not active
    /// </exception>
    public void UpdateProfile(
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null)
    {
        if (!IsActive)
            throw UserDomainException.CannotUpdateInactiveProfile(Id);

        var oldName = FullName;
        var changed = false;

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            FirstName = firstName;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            LastName = lastName;
            changed = true;
        }

        if (phoneNumber != null)
        {
            PhoneNumber = phoneNumber;
            changed = true;
        }

        if (changed && !oldName.Equals(FullName, StringComparison.Ordinal))
        {
            RaiseDomainEvent(new UserProfileUpdatedEvent(Id, Email.Value, FullName));
        }
    }

    #endregion

    #region Password Management

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password</param>
    /// <exception cref="UserDomainException">
    /// Thrown when:
    /// - The new password hash is empty
    /// - The account is locked
    /// </exception>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw UserDomainException.InvalidPasswordHash();

        if (IsLocked)
            throw UserDomainException.CannotChangePasswordWhileLocked(Id);

        PasswordHash = newPasswordHash;
        RaiseDomainEvent(new PasswordChangedEvent(Id, Email.Value));
    }

    #endregion

    #region Email Verification

    /// <summary>
    /// Verifies the user's email address.
    /// </summary>
    /// <exception cref="UserDomainException">
    /// Thrown when the email is already verified
    /// </exception>
    public void VerifyEmail()
    {
        if (IsVerified)
            throw UserDomainException.EmailAlreadyVerified(Id);

        IsVerified = true;
        RaiseDomainEvent(new UserEmailVerifiedEvent(Id, Email.Value));
    }

    #endregion

    #region Account Locking

    /// <summary>
    /// Locks the user account.
    /// </summary>
    /// <param name="reason">The reason for locking the account</param>
    /// <exception cref="UserDomainException">
    /// Thrown when:
    /// - The account is already locked
    /// - The account is not verified (business rule: only verified accounts can be locked)
    /// </exception>
    public void Lock(string? reason = null)
    {
        if (IsLocked)
            throw UserDomainException.UserAlreadyLocked(Id);

        if (!IsVerified)
            throw UserDomainException.CannotLockUnverifiedAccount(Id);

        IsLocked = true;
        LockReason = reason;
        RaiseDomainEvent(new UserLockedEvent(Id, Email.Value, reason));
    }

    /// <summary>
    /// Unlocks the user account.
    /// </summary>
    /// <exception cref="UserDomainException">
    /// Thrown when the account is not locked
    /// </exception>
    public void Unlock()
    {
        if (!IsLocked)
            throw UserDomainException.UserNotLocked(Id);

        IsLocked = false;
        LockReason = null;
        RaiseDomainEvent(new UserUnlockedEvent(Id, Email.Value));
    }

    #endregion

    #region Account Status

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            RevokeAllRefreshTokens();
            RaiseDomainEvent(new UserDeactivatedEvent(Id, Email.Value));
        }
    }

    /// <summary>
    /// Activates the user account.
    /// </summary>
    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            RaiseDomainEvent(new UserActivatedEvent(Id, Email.Value));
        }
    }

    /// <summary>
    /// Records a successful login for the user.
    /// </summary>
    /// <param name="ipAddress">The IP address from which the login occurred</param>
    public void RecordLogin(string? ipAddress = null)
    {
        LastLoginAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new UserLoggedInEvent(Id, Email.Value, ipAddress));
    }

    #endregion

    #region Role Management

    /// <summary>
    /// Adds a role to this user.
    /// </summary>
    /// <param name="role">The role to add</param>
    /// <exception cref="UserDomainException">
    /// Thrown when the user already has this role
    /// </exception>
    public void AddRole(Role role)
    {
        if (_roles.Any(r => r.RoleId == role.Id))
            throw UserDomainException.DuplicateRole(Id, role.Name);

        var userRole = new UserRole(Id, role.Id);
        _roles.Add(userRole);
        RaiseDomainEvent(new UserRoleAssignedEvent(Id, role.Id, role.Name));
    }

    /// <summary>
    /// Removes a role from this user.
    /// </summary>
    /// <param name="roleId">The ID of the role to remove</param>
    /// <exception cref="UserDomainException">
    /// Thrown when the user doesn't have this role
    /// </summary>
    public void RemoveRole(Guid roleId)
    {
        var userRole = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (userRole == null)
            throw UserDomainException.RoleNotAssigned(Id, roleId);

        _roles.Remove(userRole);
        RaiseDomainEvent(new UserRoleRemovedEvent(Id, roleId, string.Empty));
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="roleName">The name of the role to check</param>
    /// <returns>True if the user has the role</returns>
    public bool HasRole(string roleName)
    {
        return _roles.Any(r =>
            r.Role?.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Checks if the user has any of the specified roles.
    /// </summary>
    /// <param name="roleNames">The names of the roles to check</param>
    /// <returns>True if the user has any of the roles</returns>
    public bool HasAnyRole(params string[] roleNames)
    {
        return _roles.Any(r =>
            roleNames.Any(name =>
                r.Role?.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true));
    }

    #endregion

    #region Refresh Token Management

    /// <summary>
    /// Adds a new refresh token for the user.
    /// </summary>
    /// <param name="token">The refresh token value</param>
    /// <param name="expiresAtUtc">When the token expires</param>
    /// <param name="ipAddress">The IP address that requested the token</param>
    /// <returns>The created refresh token</returns>
    public RefreshToken AddRefreshToken(string token, DateTime expiresAtUtc, string? ipAddress = null)
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), Id, token, expiresAtUtc, ipAddress);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    /// <param name="tokenId">The ID of the token to revoke</param>
    /// <param name="revokedByIp">The IP address that initiated the revocation</param>
    public void RevokeRefreshToken(Guid tokenId, string? revokedByIp = null)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.Id == tokenId);
        if (token != null && !token.IsRevoked)
        {
            token.Revoke(revokedByIp);
            RaiseDomainEvent(new RefreshTokenRevokedEvent(tokenId, Id, revokedByIp));
        }
    }

    /// <summary>
    /// Revokes all refresh tokens for the user.
    /// </summary>
    /// <param name="revokedByIp">The IP address that initiated the revocation</param>
    public void RevokeAllRefreshTokens(string? revokedByIp = null)
    {
        foreach (var token in _refreshTokens.Where(t => !t.IsRevoked))
        {
            token.Revoke(revokedByIp);
            RaiseDomainEvent(new RefreshTokenRevokedEvent(token.Id, Id, revokedByIp));
        }
    }

    /// <summary>
    /// Removes all expired refresh tokens.
    /// </summary>
    public void RemoveExpiredRefreshTokens()
    {
        _refreshTokens.RemoveAll(t => t.IsExpired);
    }

    #endregion

    #region Business Rules Validation

    /// <summary>
    /// Checks if the user can log in based on account status.
    /// </summary>
    /// <returns>True if the user can log in</returns>
    public bool CanLogin()
    {
        return IsActive && !IsLocked;
    }

    /// <summary>
    /// Validates that the user can perform the specified action.
    /// </summary>
    /// <param name="action">The action to validate</param>
    /// <returns>True if the action is allowed</returns>
    public bool CanPerformAction(string action)
    {
        // Basic business rules for action validation
        return action switch
        {
            "login" => CanLogin(),
            "change_password" => !IsLocked,
            "verify_email" => !IsVerified,
            "update_profile" => IsActive,
            _ => IsActive && !IsLocked
        };
    }

    #endregion
}
