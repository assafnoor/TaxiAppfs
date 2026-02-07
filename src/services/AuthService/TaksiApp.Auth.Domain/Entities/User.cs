using System.Data;
using TaksiApp.Auth.Domain.Events;
using TaksiApp.Shared.Kernel.Common;
using TaksiApp.Shared.Kernel.ValueObjects;

namespace TaksiApp.Auth.Domain.Entities;

/// <summary>
/// Represents a user in the authentication system.
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = new();
    private readonly List<UserRole> _roles = new();

    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName => $"{FirstName} {LastName}";
    public bool IsActive { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public string? TenantId { get; private set; }

    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public IReadOnlyList<UserRole> Roles => _roles.AsReadOnly();

    // Private constructor for EF Core
    private User() { }

    private User(
        Guid id,
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        string? tenantId = null)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        TenantId = tenantId;
        IsActive = true;
        IsEmailVerified = false;
        CreatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new UserCreatedEvent(id, email.Value));
    }

    public static User Create(
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        string? tenantId = null)
    {
        return new User(Guid.NewGuid(), email, passwordHash, firstName, lastName, tenantId);
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        RaiseDomainEvent(new UserPasswordChangedEvent(Id, Email.Value));
    }

    public void VerifyEmail()
    {
        if (!IsEmailVerified)
        {
            IsEmailVerified = true;
            RaiseDomainEvent(new UserEmailVerifiedEvent(Id, Email.Value));
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            RevokeAllRefreshTokens();
            RaiseDomainEvent(new UserDeactivatedEvent(Id, Email.Value));
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            RaiseDomainEvent(new UserActivatedEvent(Id, Email.Value));
        }
    }

    public void RecordLogin()
    {
        LastLoginAtUtc = DateTime.UtcNow;
    }

    public void AddRole(Role role)
    {
        if (!_roles.Any(r => r.RoleId == role.Id))
        {
            var userRole = new UserRole(Id, role.Id);
            _roles.Add(userRole);
            RaiseDomainEvent(new UserRoleAssignedEvent(Id, role.Id, role.Name));
        }
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (userRole != null)
        {
            _roles.Remove(userRole);
            RaiseDomainEvent(new UserRoleRemovedEvent(Id, roleId));
        }
    }

    public bool HasRole(string roleName)
    {
        return _roles.Any(r => r.Role?.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase) == true);
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAtUtc, string? ipAddress = null)
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), Id, token, expiresAtUtc, ipAddress);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(Guid tokenId, string? revokedByIp = null)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.Id == tokenId);
        if (token != null && !token.IsRevoked)
        {
            token.Revoke(revokedByIp);
        }
    }

    public void RevokeAllRefreshTokens(string? revokedByIp = null)
    {
        foreach (var token in _refreshTokens.Where(t => !t.IsRevoked))
        {
            token.Revoke(revokedByIp);
        }
    }

    public void RemoveExpiredRefreshTokens()
    {
        _refreshTokens.RemoveAll(t => t.IsExpired);
    }
}