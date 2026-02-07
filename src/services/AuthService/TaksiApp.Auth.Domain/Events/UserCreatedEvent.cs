using TaksiApp.Shared.Kernel.Events;

namespace TaksiApp.Auth.Domain.Events;

public sealed record UserCreatedEvent(Guid UserId, string Email) : DomainEventBase;

public sealed record UserPasswordChangedEvent(Guid UserId, string Email) : DomainEventBase;

public sealed record UserEmailVerifiedEvent(Guid UserId, string Email) : DomainEventBase;

public sealed record UserDeactivatedEvent(Guid UserId, string Email) : DomainEventBase;

public sealed record UserActivatedEvent(Guid UserId, string Email) : DomainEventBase;

public sealed record UserRoleAssignedEvent(Guid UserId, Guid RoleId, string RoleName) : DomainEventBase;

public sealed record UserRoleRemovedEvent(Guid UserId, Guid RoleId) : DomainEventBase;

public sealed record UserLoggedInEvent(Guid UserId, string Email, string? IpAddress) : DomainEventBase;

public sealed record RefreshTokenRevokedEvent(Guid TokenId, Guid UserId, string? RevokedByIp) : DomainEventBase;
