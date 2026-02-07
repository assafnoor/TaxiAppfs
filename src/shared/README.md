# TaksiApp.Shared Library

A foundational shared library for building Clean Architecture / DDD / Microservices systems in .NET. This library provides cross-cutting concerns, domain primitives, CQRS infrastructure, and observability tooling that can be reused across all services in a distributed system.

---

## Project Overview

The TaksiApp.Shared library is organized into six NuGet packages, each with a specific responsibility:

| Package | Purpose |
|---------|---------|
| `TaksiApp.Shared.Kernel` | Domain primitives: Entity, AggregateRoot, ValueObject, Result pattern, Guards |
| `TaksiApp.Shared.Application` | CQRS abstractions, ExecutionContext, MediatR pipeline behaviors |
| `TaksiApp.Shared.Api` | ASP.NET Core middleware, base controllers, ProblemDetails factory |
| `TaksiApp.Shared.Infrastructure` | Infrastructure implementations (e.g., `IDateTimeProvider`) |
| `TaksiApp.Shared.Observability` | OpenTelemetry metrics, tracing, Serilog enrichers, health checks |
| `TaksiApp.Shared.Extensions` | DI registration extensions for all shared components |

---

## Installation

### Package References

Add the required packages to your service's `.csproj` file:

```xml
<ItemGroup>
  <!-- Core packages (always required) -->
  <ProjectReference Include="..\..\..\shared\TaksiApp.Shared.Kernel\TaksiApp.Shared.Kernel.csproj" />
  <ProjectReference Include="..\..\..\shared\TaksiApp.Shared.Application\TaksiApp.Shared.Application.csproj" />
  
  <!-- API layer (for ASP.NET Core services) -->
  <ProjectReference Include="..\..\..\shared\TaksiApp.Shared.Api\TaksiApp.Shared.Api.csproj" />
  
  <!-- Infrastructure implementations -->
  <ProjectReference Include="..\..\..\shared\TaksiApp.Shared.Infrastructure\TaksiApp.Shared.Infrastructure.csproj" />
  
  <!-- Observability (metrics, tracing, logging) -->
  <ProjectReference Include="..\..\..\shared\TaksiApp.Shared.Observability\TaksiApp.Shared.Observability.csproj" />
  
  <!-- DI extensions (recommended - includes all registration helpers) -->
  <ProjectReference Include="..\..\..\shared\TaksiApp.Shared.Extensions\TaksiApp.Shared.Extensions.csproj" />
</ItemGroup>
```

For NuGet packages (when published):

```xml
<ItemGroup>
  <PackageReference Include="TaksiApp.Shared.Extensions" Version="1.0.0" />
</ItemGroup>
```

> **Note**: `TaksiApp.Shared.Extensions` transitively references all other shared packages.

### Required External Dependencies

Ensure these packages are installed in your service:

```xml
<ItemGroup>
  <!-- MediatR for CQRS -->
  <PackageReference Include="MediatR" Version="12.x" />
  
  <!-- FluentValidation for ValidationBehavior -->
  <PackageReference Include="FluentValidation" Version="11.x" />
  <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.x" />
  
  <!-- Serilog for structured logging -->
  <PackageReference Include="Serilog.AspNetCore" Version="8.x" />
</ItemGroup>
```

---

## Configuration

### Minimal Setup (Program.cs)

```csharp
using TaksiApp.Shared.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. Register Shared Services
// ============================================

// Application layer (IExecutionContext, IHttpContextAccessor)
builder.Services.AddSharedApplication(builder.Configuration);

// Infrastructure layer (IDateTimeProvider)
builder.Services.AddSharedInfrastructure();

// Observability (OpenTelemetry, metrics, tracing, health checks)
builder.Services.AddSharedObservability(
    builder.Configuration,
    serviceName: "OrderService",
    serviceVersion: "1.0.0");

// MediatR pipeline behaviors (Logging → Tracing → Validation)
builder.Services.AddSharedMediatRBehaviors();

// ============================================
// 2. Register Service-Specific Components
// ============================================

// MediatR handlers from your service
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// FluentValidation validators
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// ============================================
// 3. Configure Middleware Pipeline
// ============================================

// Correlation ID middleware (must be early in pipeline)
app.UseCorrelationId();

// Standard middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### Full Configuration with Custom Extractors

```csharp
using TaksiApp.Shared.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Service Registration
// ============================================

builder.Services
    .AddSharedApplication(builder.Configuration)
    .AddSharedInfrastructure()
    .AddSharedObservability(
        builder.Configuration,
        serviceName: "OrderService",
        serviceVersion: "1.0.0",
        configureOtel: otel =>
        {
            // Add custom OpenTelemetry configuration
            otel.WithTracing(tracing =>
            {
                tracing.AddSource("MyCustomActivitySource");
            });
        })
    .AddSharedMediatRBehaviors();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// HTTP clients with correlation ID propagation
builder.Services
    .AddHttpClient("PaymentService", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:Payment:BaseUrl"]!);
    })
    .AddCorrelationIdHandler();

builder.Services
    .AddHttpClient("NotificationService", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:Notification:BaseUrl"]!);
    })
    .AddCorrelationIdHandler();

// Controllers with ProblemDetails factory
builder.Services.AddControllers();
builder.Services.AddSingleton<ProblemDetailsFactory, SharedProblemDetailsFactory>();

var app = builder.Build();

// ============================================
// Middleware Pipeline
// ============================================

// Correlation ID with custom extractors for JWT claims
app.UseCorrelationId(
    userIdExtractor: ctx => ctx.User.FindFirst("sub")?.Value,
    tenantIdExtractor: ctx => ctx.User.FindFirst("tenant_id")?.Value
);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint(); // Prometheus metrics endpoint

app.Run();
```

### Configuration File (appsettings.json)

```json
{
  "Observability": {
    "OtlpEndpoint": "http://localhost:4317",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableLogging": true,
    "ServiceVersion": "1.0.0"
  },
  "Services": {
    "Payment": {
      "BaseUrl": "https://payment.internal"
    },
    "Notification": {
      "BaseUrl": "https://notification.internal"
    }
  }
}
```

### Serilog Configuration (appsettings.json)

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Serilog Setup with Enrichers (Program.cs)

```csharp
using Serilog;
using TaksiApp.Shared.Observability.Logging.Enrichers;

// Configure Serilog before building the host
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.With<TraceContextEnricher>()
        .Enrich.With(services.GetRequiredService<CorrelationIdEnricher>());
});
```

---

## Layer-by-Layer Registration

### Option 1: All-in-One (Recommended)

```csharp
builder.Services
    .AddSharedApplication(builder.Configuration)
    .AddSharedInfrastructure()
    .AddSharedObservability(builder.Configuration, "MyService")
    .AddSharedMediatRBehaviors();
```

### Option 2: Selective Registration

```csharp
// Only application contracts (no observability)
builder.Services.AddSharedApplication(builder.Configuration);
builder.Services.AddSharedInfrastructure();

// Only MediatR behaviors (if you configure observability separately)
builder.Services.AddSharedMediatRBehaviors();
```

### Option 3: Manual Registration (Advanced)

```csharp
// Manual IExecutionContext registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IExecutionContext>(sp =>
{
    var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
    if (httpContextAccessor?.HttpContext != null)
    {
        return httpContextAccessor.HttpContext.GetExecutionContext();
    }
    return new ExecutionContext(Guid.NewGuid().ToString());
});

// Manual IDateTimeProvider registration
builder.Services.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();

// Manual behavior registration (order matters!)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

---

## Middleware Order

The middleware order is critical for proper correlation ID propagation:

```csharp
var app = builder.Build();

// 1. Exception handling (catches all errors)
app.UseExceptionHandler("/error");

// 2. Correlation ID (must be before authentication to capture user/tenant)
app.UseCorrelationId();

// 3. Authentication (populates HttpContext.User)
app.UseAuthentication();

// 4. Authorization
app.UseAuthorization();

// 5. Routing
app.UseRouting();

// 6. Endpoints
app.MapControllers();
app.MapHealthChecks("/health");
```

> **Important**: `UseCorrelationId()` should be placed after `UseAuthentication()` if you need to extract user/tenant from JWT claims. Place it before if you only need correlation IDs.

---

## Design Goals & Philosophy

1. **Explicit over implicit**: All operations return `Result<T>` instead of throwing exceptions for expected failures.
2. **Immutability by default**: `ExecutionContext`, `ExecutionContextMetadata`, `Error`, and all value objects are immutable.
3. **Thread-safety**: All shared components are designed for concurrent access in web server environments.
4. **Layer separation**: Application contracts are separated from infrastructure implementations.
5. **Observability-first**: Correlation IDs, distributed tracing, and structured logging are built-in.
6. **No magic**: Explicit registration, explicit middleware ordering, explicit behavior pipelines.

---

## Problems Solved

| Problem | Solution |
|---------|----------|
| Inconsistent error handling across services | `Result<T>` pattern with typed `Error` |
| Lost request context in async flows | `IExecutionContext` with scoped lifetime |
| Missing correlation IDs in distributed traces | `CorrelationIdMiddleware` + `CorrelationIdDelegatingHandler` |
| Boilerplate CQRS setup | `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler` |
| Repeated validation/logging/tracing code | MediatR pipeline behaviors |
| Inconsistent ProblemDetails responses | `ApiControllerBase` + `SharedProblemDetailsFactory` |
| Cross-tenant data access vulnerabilities | `ExecutionContext.EnsureTenantAccess()` |
| Testability of time-dependent code | `IDateTimeProvider` abstraction |

---

## What This Library Provides

### Domain Layer (`TaksiApp.Shared.Kernel`)
- [`Entity<TId>`](TaksiApp.Shared.Kernel/Common/Entity.cs) — Base class for entities with identity-based equality
- [`AggregateRoot<TId>`](TaksiApp.Shared.Kernel/Common/AggregateRoot.cs) — Entity with domain event support
- [`ValueObject`](TaksiApp.Shared.Kernel/Common/ValueObject.cs) — Base class for value objects with structural equality
- [`Result`](TaksiApp.Shared.Kernel/Results/Result.cs) / [`Result<T>`](TaksiApp.Shared.Kernel/Results/Result{T}.cs) — Railway-oriented result pattern
- [`Error`](TaksiApp.Shared.Kernel/Results/Error.cs) — Immutable, serialization-safe error representation
- [`Guard`](TaksiApp.Shared.Kernel/Guards/Guard.cs) — Defensive programming guard clauses
- Value Objects: [`Email`](TaksiApp.Shared.Kernel/ValueObjects/Email.cs), [`Money`](TaksiApp.Shared.Kernel/ValueObjects/Money.cs), [`PhoneNumber`](TaksiApp.Shared.Kernel/ValueObjects/PhoneNumber.cs)

### Application Layer (`TaksiApp.Shared.Application`)
- [`ICommand`](TaksiApp.Shared.Application/Abstractions/ICommand.cs) / [`IQuery<T>`](TaksiApp.Shared.Application/Abstractions/IQuery.cs) — CQRS marker interfaces
- [`ICommandHandler<T>`](TaksiApp.Shared.Application/Abstractions/ICommandHandler.cs) / [`IQueryHandler<T, R>`](TaksiApp.Shared.Application/Abstractions/IQueryHandler.cs) — Handler contracts
- [`IExecutionContext`](TaksiApp.Shared.Application/Abstractions/IExecutionContext.cs) — Request-scoped context (correlation, user, tenant)
- [`ExecutionContextMetadata`](TaksiApp.Shared.Application/Abstractions/ExecutionContextMetadata.cs) — Immutable metadata container
- [`LoggingBehavior`](TaksiApp.Shared.Application/Behaviors/LoggingBehavior.cs) — Structured logging for all commands/queries
- [`TracingBehavior`](TaksiApp.Shared.Application/Behaviors/TracingBehavior.cs) — OpenTelemetry span creation
- [`ValidationBehavior`](TaksiApp.Shared.Application/Behaviors/ValidationBehavior.cs) — FluentValidation integration

### API Layer (`TaksiApp.Shared.Api`)
- [`CorrelationIdMiddleware`](TaksiApp.Shared.Api/Middleware/CorrelationIdMiddleware.cs) — Extracts/generates correlation IDs
- [`CorrelationIdDelegatingHandler`](TaksiApp.Shared.Api/Middleware/CorrelationIdDelegatingHandler.cs) — Propagates correlation IDs to outgoing HTTP calls
- [`ApiControllerBase`](TaksiApp.Shared.Api/Controllers/ApiControllerBase.cs) — Base controller with `Error` → `ProblemDetails` conversion
- [`SharedProblemDetailsFactory`](TaksiApp.Shared.Api/Factories/SharedProblemDetailsFactory.cs) — Trace-aware ProblemDetails factory

### Observability (`TaksiApp.Shared.Observability`)
- [`ApplicationMetrics`](TaksiApp.Shared.Observability/Metrics/Meters/ApplicationMetrics.cs) — Generic error/request metrics
- [`MeterProvider`](TaksiApp.Shared.Observability/Metrics/MeterProvider.cs) — OpenTelemetry meter factory
- [`ActivitySourceFactory`](TaksiApp.Shared.Observability/Tracing/ActivitySourceFactory.cs) — Tracing span factory
- [`CorrelationIdEnricher`](TaksiApp.Shared.Observability/Logging/Enrichers/CorrelationIdEnricher.cs) — Serilog enricher for correlation context
- [`TraceContextEnricher`](TaksiApp.Shared.Observability/Logging/Enrichers/TraceContextEnricher.cs) — Serilog enricher for trace/span IDs
- [`ApplicationHealthCheck`](TaksiApp.Shared.Observability/HealthChecks/ApplicationHealthCheck.cs) — Basic application health check

---

## What This Library Does NOT Do

| Responsibility | Why Not Included |
|----------------|------------------|
| Authentication/Authorization | Service-specific; use ASP.NET Core Identity or external providers |
| Database access (EF Core, Dapper) | Infrastructure concern; each service defines its own DbContext |
| Message bus integration | Service-specific; use MassTransit, NServiceBus, or raw RabbitMQ |
| Domain event publishing | Requires infrastructure (Outbox pattern); implement per-service |
| Caching | Service-specific caching strategies vary |
| API versioning | Service-specific routing decisions |
| Rate limiting | Gateway or service-specific concern |
| Retry policies | Service-specific; use Polly per HTTP client |

---

## Core Concepts

### Result Pattern

The library uses the Result pattern instead of exceptions for expected failures:

```csharp
public Result<Order> CreateOrder(CreateOrderRequest request)
{
    if (request.Quantity <= 0)
        return Result.Failure<Order>(
            Error.Validation("Order.InvalidQuantity", "Quantity must be positive"));

    var order = new Order(request.CustomerId, request.Quantity);
    return Result.Success(order);
}
```

**Key methods on `Result<T>`:**
- `Match(onSuccess, onFailure)` — Pattern matching
- `Bind(func)` — Chain result-producing operations
- `Map(func)` — Transform successful value
- `Tap(action)` — Execute side effect on success
- `Ensure(predicate, error)` — Validate condition

### Execution Context

[`IExecutionContext`](TaksiApp.Shared.Application/Abstractions/IExecutionContext.cs:7) provides request-scoped context:

```csharp
public interface IExecutionContext
{
    string CorrelationId { get; }
    string? UserId { get; }
    string? TenantId { get; }
    IReadOnlyDictionary<string, object> Metadata { get; }
    void EnsureTenantAccess(string? resourceTenantId);
}
```

The context is:
- **Immutable** — Cannot be modified after creation
- **Scoped** — One instance per HTTP request
- **Propagated** — Flows to downstream services via `X-Correlation-Id` header

### CQRS with MediatR

Commands and queries are separated by intent:

```csharp
// Command (write operation)
public record CreateOrderCommand(Guid CustomerId, int Quantity) : ICommand<Guid>;

// Query (read operation)
public record GetOrderQuery(Guid OrderId) : IQuery<OrderDto>;
```

All handlers return `Result` or `Result<T>`:

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        // Implementation
        return Result.Success(order.Id);
    }
}
```

---

## Main Components

### Entity & AggregateRoot

```csharp
public class Order : AggregateRoot<Guid>
{
    public CustomerId CustomerId { get; private set; }
    public Money TotalAmount { get; private set; }
    
    public Order(Guid id, CustomerId customerId) : base(id)
    {
        CustomerId = customerId;
        RaiseDomainEvent(new OrderCreatedEvent(id));
    }
}
```

**Key characteristics:**
- Identity-based equality (two entities with same ID are equal)
- Handles EF Core proxy types correctly
- Domain events are collected and cleared after publishing

### ValueObject

```csharp
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}
```

**Key characteristics:**
- Structural equality (compared by value, not reference)
- Immutable by design
- Self-validating (use factory methods with `Result<T>`)

### Error

[`Error`](TaksiApp.Shared.Kernel/Results/Error.cs:24) is an immutable struct with factory methods:

```csharp
Error.Validation("Order.InvalidQuantity", "Quantity must be positive");
Error.NotFound("Order.NotFound", $"Order {orderId} not found");
Error.Conflict("Order.AlreadyShipped", "Cannot modify shipped order");
Error.Unauthorized("Auth.InvalidToken", "Token has expired");
Error.Forbidden("Auth.InsufficientPermissions", "Admin role required");
Error.Failure("Database.Timeout", "Database connection timed out");
```

**Error types map to HTTP status codes:**
| ErrorType | HTTP Status |
|-----------|-------------|
| Validation | 400 Bad Request |
| NotFound | 404 Not Found |
| Conflict | 409 Conflict |
| Unauthorized | 401 Unauthorized |
| Forbidden | 403 Forbidden |
| Failure | 500 Internal Server Error |

### Guard Clauses

```csharp
public void ProcessOrder(Order order, decimal amount)
{
    Guard.NotNull(order);
    Guard.GreaterThan(amount, 0);
    Guard.InRange(order.Quantity, 1, 1000);
    Guard.NotNullOrEmpty(order.CustomerName);
}
```

---

## Execution Flow

### HTTP Request Flow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           HTTP Request                                   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                     CorrelationIdMiddleware                              │
│  • Extract/generate correlation ID                                       │
│  • Extract user ID and tenant ID                                         │
│  • Create ExecutionContext                                               │
│  • Store in HttpContext.Items                                            │
│  • Enrich Activity with tags                                             │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Controller Action                                │
│  • Inject IExecutionContext                                              │
│  • Send command/query via MediatR                                        │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      MediatR Pipeline                                    │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ LoggingBehavior                                                  │    │
│  │  • Log request start with correlation ID                         │    │
│  │  • Measure execution time                                        │    │
│  │  • Log success/failure                                           │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                              │                                           │
│                              ▼                                           │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ TracingBehavior                                                  │    │
│  │  • Create Activity span                                          │    │
│  │  • Set request type/name tags                                    │    │
│  │  • Record success/error status                                   │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                              │                                           │
│                              ▼                                           │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ ValidationBehavior                                               │    │
│  │  • Run FluentValidation validators                               │    │
│  │  • Return Result.Failure if validation fails                     │    │
│  │  • Short-circuit pipeline on failure                             │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                              │                                           │
│                              ▼                                           │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ Command/Query Handler                                            │    │
│  │  • Execute business logic                                        │    │
│  │  • Return Result<T>                                              │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Controller Action                                │
│  • Match on Result                                                       │
│  • Return Ok(value) or Problem(error)                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Correlation ID Propagation

```
┌──────────────┐     X-Correlation-Id: abc-123     ┌──────────────┐
│   Service A  │ ─────────────────────────────────▶│   Service B  │
│              │                                    │              │
│ Middleware   │                                    │ Middleware   │
│ extracts ID  │                                    │ extracts ID  │
└──────────────┘                                    └──────────────┘
       │                                                   │
       │ CorrelationIdDelegatingHandler                    │
       │ adds header to outgoing requests                  │
       ▼                                                   ▼
┌──────────────┐                                    ┌──────────────┐
│    Logs      │                                    │    Logs      │
│ correlation_ │                                    │ correlation_ │
│ id: abc-123  │                                    │ id: abc-123  │
└──────────────┘                                    └──────────────┘
```

---

## Usage Examples

### HTTP Request (Controller)

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly IExecutionContext _executionContext;

    public OrdersController(ISender sender, IExecutionContext executionContext)
    {
        _sender = sender;
        _executionContext = executionContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Quantity);

        var result = await _sender.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            onSuccess: orderId => CreatedAtAction(
                nameof(GetOrder),
                new { id = orderId },
                new { id = orderId }),
            onFailure: error => Problem(error));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            onSuccess: order => Ok(order),
            onFailure: error => Problem(error));
    }
}
```

### Application Service (Command Handler)

```csharp
public sealed class CreateOrderCommandHandler 
    : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IExecutionContext _executionContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IExecutionContext executionContext,
        IDateTimeProvider dateTimeProvider)
    {
        _orderRepository = orderRepository;
        _executionContext = executionContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        // Validate business rules
        var customerResult = await _orderRepository
            .GetCustomerAsync(command.CustomerId, cancellationToken);

        if (customerResult.IsFailure)
            return Result.Failure<Guid>(customerResult.Error);

        var customer = customerResult.Value;

        // Ensure tenant isolation
        _executionContext.EnsureTenantAccess(customer.TenantId);

        // Create aggregate
        var order = Order.Create(
            customer,
            command.Quantity,
            _dateTimeProvider.UtcNow);

        if (order.IsFailure)
            return Result.Failure<Guid>(order.Error);

        // Persist
        await _orderRepository.AddAsync(order.Value, cancellationToken);

        return Result.Success(order.Value.Id);
    }
}
```

### Background Job

```csharp
public class OrderExpirationJob : IJob
{
    private readonly ISender _sender;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderExpirationJob(
        ISender sender,
        IServiceScopeFactory scopeFactory)
    {
        _sender = sender;
        _scopeFactory = scopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // Create a scope for the job
        using var scope = _scopeFactory.CreateScope();
        
        // ExecutionContext is automatically created with a new correlation ID
        // when no HTTP context is available
        var executionContext = scope.ServiceProvider
            .GetRequiredService<IExecutionContext>();

        // Log with correlation ID
        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<OrderExpirationJob>>();
        
        logger.LogInformation(
            "Starting order expiration job. CorrelationId: {CorrelationId}",
            executionContext.CorrelationId);

        var command = new ExpireStaleOrdersCommand();
        var result = await _sender.Send(command);

        result.Match(
            onSuccess: () => logger.LogInformation("Order expiration completed"),
            onFailure: error => logger.LogError(
                "Order expiration failed: {ErrorCode} - {ErrorMessage}",
                error.Code,
                error.Message));
    }
}
```

### Service Registration (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register shared services
builder.Services
    .AddSharedApplication(builder.Configuration)
    .AddSharedInfrastructure()
    .AddSharedObservability(
        builder.Configuration,
        serviceName: "OrderService",
        serviceVersion: "1.0.0")
    .AddSharedMediatRBehaviors();

// Register MediatR with service-specific handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
});

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderCommand).Assembly);

var app = builder.Build();

// Use correlation ID middleware
app.UseCorrelationId(
    userIdExtractor: ctx => ctx.User.FindFirst("sub")?.Value,
    tenantIdExtractor: ctx => ctx.User.FindFirst("tenant_id")?.Value);

app.MapControllers();
app.Run();
```

### HTTP Client with Correlation ID Propagation

```csharp
// In Program.cs
builder.Services
    .AddHttpClient("PaymentService", client =>
    {
        client.BaseAddress = new Uri("https://payment.internal");
    })
    .AddCorrelationIdHandler();

// In a handler
public class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public async Task<Result> Handle(
        ProcessPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("PaymentService");
        
        // X-Correlation-Id header is automatically added
        var response = await client.PostAsJsonAsync(
            "/api/payments",
            command,
            cancellationToken);

        // ...
    }
}
```

---

## Multi-Tenancy & Security Considerations

### Tenant Isolation

The [`ExecutionContext.EnsureTenantAccess()`](TaksiApp.Shared.Application/Context/ExecutionContext.cs:49) method provides runtime tenant isolation:

```csharp
public void EnsureTenantAccess(string? resourceTenantId)
{
    // System context (no tenant) can access anything
    if (TenantId == null) return;

    // Null resource tenant = shared resource
    if (resourceTenantId == null) return;

    // Cross-tenant access denied
    if (resourceTenantId != TenantId)
    {
        throw new UnauthorizedAccessException(
            $"Cross-tenant access violation. User tenant: {TenantId}, Resource tenant: {resourceTenantId}");
    }
}
```

**Usage in handlers:**

```csharp
public async Task<Result<OrderDto>> Handle(
    GetOrderQuery query,
    CancellationToken cancellationToken)
{
    var order = await _repository.GetByIdAsync(query.OrderId, cancellationToken);
    
    if (order == null)
        return Result.Failure<OrderDto>(Error.NotFound("Order.NotFound", "Order not found"));

    // Throws UnauthorizedAccessException if tenant mismatch
    _executionContext.EnsureTenantAccess(order.TenantId);

    return Result.Success(order.ToDto());
}
```

### Security Best Practices

1. **Never trust client-provided tenant IDs** — Extract from authenticated claims only
2. **Always call `EnsureTenantAccess()`** before returning tenant-specific data
3. **Use scoped `IExecutionContext`** — Never cache or store across requests
4. **Validate at the boundary** — Use `ValidationBehavior` for input validation
5. **Log with correlation IDs** — Enable forensic analysis of security incidents

---

## Best Practices

### Result Pattern

```csharp
// ✅ Good: Chain operations with Bind
var result = await GetCustomerAsync(customerId)
    .Bind(customer => ValidateCredit(customer))
    .Bind(customer => CreateOrder(customer, items));

// ✅ Good: Transform with Map
var orderDto = orderResult.Map(order => order.ToDto());

// ✅ Good: Use Match for final handling
return result.Match(
    onSuccess: dto => Ok(dto),
    onFailure: error => Problem(error));

// ❌ Bad: Accessing Value without checking IsSuccess
var order = result.Value; // Throws if failure

// ❌ Bad: Using exceptions for expected failures
if (customer == null)
    throw new CustomerNotFoundException(); // Use Result.Failure instead
```

### Execution Context

```csharp
// ✅ Good: Inject IExecutionContext
public class MyHandler
{
    private readonly IExecutionContext _context;
    
    public MyHandler(IExecutionContext context)
    {
        _context = context;
    }
}

// ✅ Good: Use for logging correlation
_logger.LogInformation(
    "Processing order. CorrelationId: {CorrelationId}",
    _context.CorrelationId);

// ❌ Bad: Storing context in static field
private static IExecutionContext _context; // Never do this

// ❌ Bad: Passing context through method parameters
public void Process(IExecutionContext context) // Inject via DI instead
```

### Value Objects

```csharp
// ✅ Good: Factory method with validation
public static Result<Email> Create(string? email)
{
    if (string.IsNullOrWhiteSpace(email))
        return Result.Failure<Email>(Error.Validation("Email.Empty", "Email required"));
    
    return Result.Success(new Email(email.ToLowerInvariant()));
}

// ✅ Good: Immutable properties
public string Value { get; } // No setter

// ❌ Bad: Public constructor without validation
public Email(string value) { Value = value; } // Use factory method

// ❌ Bad: Mutable properties
public string Value { get; set; } // Value objects must be immutable
```

### Error Codes

```csharp
// ✅ Good: Structured, specific error codes
Error.Validation("Order.Quantity.BelowMinimum", "Quantity must be at least 1");
Error.NotFound("Customer.NotFound", $"Customer {id} not found");
Error.Conflict("Order.AlreadyShipped", "Cannot modify shipped order");

// ❌ Bad: Generic error codes
Error.Validation("Error", "Something went wrong");
Error.Failure("Failed", "Operation failed");
```

---

## Common Mistakes to Avoid

### 1. Throwing Exceptions for Business Rules

```csharp
// ❌ Wrong
if (quantity <= 0)
    throw new ArgumentException("Quantity must be positive");

// ✅ Correct
if (quantity <= 0)
    return Result.Failure<Order>(
        Error.Validation("Order.InvalidQuantity", "Quantity must be positive"));
```

### 2. Ignoring Result Failures

```csharp
// ❌ Wrong: Ignoring the result
var result = await _sender.Send(command);
return Ok(); // What if result.IsFailure?

// ✅ Correct: Handle both cases
var result = await _sender.Send(command);
return result.Match(
    onSuccess: () => Ok(),
    onFailure: error => Problem(error));
```

### 3. Storing Business Data in Metadata

```csharp
// ❌ Wrong: Business data in metadata
var context = new ExecutionContext(
    correlationId,
    userId,
    tenantId,
    metadata: new Dictionary<string, object>
    {
        ["orderTotal"] = 150.00m,  // Business data
        ["customerTier"] = "Gold"   // Business data
    });

// ✅ Correct: Only technical/diagnostic data
var context = new ExecutionContext(
    correlationId,
    userId,
    tenantId,
    metadata: new Dictionary<string, object>
    {
        ["requestId"] = requestId,      // Diagnostic
        ["apiVersion"] = "v2",           // Technical
        ["clientIp"] = clientIp          // Diagnostic
    });
```

### 4. Modifying Execution Context

```csharp
// ❌ Wrong: Trying to modify context
_executionContext.UserId = newUserId; // Won't compile - no setter

// ✅ Correct: Create new context if needed (rare)
var newContext = new ExecutionContext(
    _executionContext.CorrelationId,
    newUserId,
    _executionContext.TenantId);
```

### 5. Wrong Behavior Registration Order

```csharp
// ❌ Wrong: Validation before logging
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// ✅ Correct: Use AddSharedMediatRBehaviors() which ensures correct order
services.AddSharedMediatRBehaviors();
```

### 6. Not Propagating Correlation IDs

```csharp
// ❌ Wrong: Manual HTTP client without correlation
var client = new HttpClient();
await client.GetAsync("https://other-service/api/data");

// ✅ Correct: Use IHttpClientFactory with handler
builder.Services
    .AddHttpClient("OtherService")
    .AddCorrelationIdHandler();
```

---

## When to Use This Library

✅ **Use when:**
- Building microservices that need consistent error handling
- Implementing CQRS with MediatR
- Requiring distributed tracing across services
- Building multi-tenant applications
- Needing request-scoped context (correlation, user, tenant)
- Wanting to avoid exception-driven control flow

---

## When NOT to Use This Library

❌ **Don't use when:**
- Building a simple CRUD application without CQRS
- Working with legacy code that uses exceptions for control flow
- The overhead of Result pattern is not justified
- You need a different error handling strategy
- Your team is not familiar with functional programming concepts

---

## Package Dependencies

| Package | Key Dependencies |
|---------|------------------|
| `TaksiApp.Shared.Kernel` | None (pure .NET) |
| `TaksiApp.Shared.Application` | MediatR, FluentValidation |
| `TaksiApp.Shared.Api` | Microsoft.AspNetCore.* |
| `TaksiApp.Shared.Infrastructure` | TaksiApp.Shared.Application |
| `TaksiApp.Shared.Observability` | OpenTelemetry.*, Serilog |
| `TaksiApp.Shared.Extensions` | All above packages |

---

## License

This library is part of the TaksiApp system and is intended for internal use.
