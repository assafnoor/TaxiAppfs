# TaksiApp.Shared - Foundation Library for ASP.NET Core Microservices

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.0.0-orange)](CHANGELOG.md)

A production-ready, domain-agnostic foundation library for building ASP.NET Core microservices with Clean Architecture, DDD patterns, and comprehensive observability.

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Principles](#-key-principles)
- [Features](#-features)
- [Getting Started](#-getting-started)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Usage Guide](#-usage-guide)
- [Configuration](#-configuration)
- [Best Practices](#-best-practices)
- [FAQ](#-faq)
- [Contributing](#-contributing)

---

## 🎯 Overview

**TaksiApp.Shared** is a comprehensive foundation library designed to accelerate microservice development while enforcing best practices. It provides battle-tested abstractions for domain modeling, cross-cutting concerns, and observability—all without introducing domain-specific logic.

### What This Library Does

- **Standardizes** domain modeling with DDD building blocks (Entities, Value Objects, Aggregate Roots)
- **Simplifies** error handling with the Result pattern (no exception-driven control flow)
- **Ensures** request traceability with correlation IDs and distributed tracing
- **Provides** comprehensive observability (OpenTelemetry metrics, logging, health checks)
- **Enforces** Clean Architecture principles across services

### Why Use This Library?

✅ **Accelerate Development** - Skip boilerplate and focus on business logic  
✅ **Production-Ready** - Built with real-world microservice patterns  
✅ **Domain-Agnostic** - Reusable across any business domain  
✅ **Testable** - All components injectable via DI, no static dependencies  
✅ **Observable** - Full OpenTelemetry integration out of the box  
✅ **Type-Safe** - Leverage C#'s type system for safer code  

---

## 🔑 Key Principles

This library is built on four core principles:

1. **Reusability** - Works across multiple services and domains without modification
2. **Stability** - Low churn, semantic versioning, no breaking changes without migration guides
3. **Domain-Agnostic** - Contains zero business logic or domain-specific code
4. **Production-Ready** - Tested patterns suitable for enterprise microservices

---

## ✨ Features

### 🧱 Kernel Layer

**Domain Building Blocks (DDD)**

- **Entity<TId>** - Base class for entities with identity-based equality
- **AggregateRoot<TId>** - Base class for aggregate roots with domain event support
- **ValueObject** - Base class for value objects with value-based equality
- **IDomainEvent** - Marker interface for domain events
- **DomainEventBase** - Base implementation for domain events

**Error Handling**

- **Result / Result<T>** - Railway-oriented programming for explicit error handling
- **Error** - Strongly-typed error representation with metadata
- **ErrorType** - Standard error categories (Validation, NotFound, Conflict, etc.)

**Validation**

- **Guard** - Fluent guard clauses for parameter validation
- **Email, Money, PhoneNumber** - Built-in value objects with validation

### 🔌 Application Layer

**Execution Context**

- **IExecutionContext** - Provides correlation ID, user ID, tenant ID per request
- **ExecutionContextMetadata** - Immutable metadata container for technical context
- **IDateTimeProvider** - Time abstraction for testability

### 🌐 API Infrastructure

**Controllers & Error Handling**

- **ApiControllerBase** - Base controller with Result → ProblemDetails conversion
- **SharedProblemDetailsFactory** - Consistent error responses with trace context

**Middleware & Handlers**

- **CorrelationIdMiddleware** - Extracts/generates correlation IDs per request
- **CorrelationIdDelegatingHandler** - Propagates correlation IDs to HttpClient calls
- **HttpContextConstants** - Centralized constants for headers and keys

### 📊 Observability

**Tracing (OpenTelemetry)**

- **ActivitySourceFactory** - Creates activity sources for distributed tracing
- **TracingConstants** - Standard trace context keys

**Metrics (OpenTelemetry)**

- **ApplicationMetrics** - Generic error and active request tracking
- **MeterProvider** - Factory for creating custom meters
- **MetricsConstants** - Standard metric names following OpenTelemetry conventions

**Logging (Serilog)**

- **CorrelationIdEnricher** - Adds correlation ID to all log events
- **TraceContextEnricher** - Adds trace/span IDs to log events

**Health Checks**

- **ApplicationHealthCheck** - Generic health check for core services

### 🏗️ Infrastructure

- **UtcDateTimeProvider** - Production implementation of IDateTimeProvider
- **InfrastructureExtensions** - DI registration for infrastructure services

### ⚙️ Extensions

- **ApplicationExtensions** - Registers application layer contracts
- **MiddlewareExtensions** - Registers middleware components
- **ObservabilityExtensions** - Configures OpenTelemetry stack
- **ExecutionContextExtensions** - HttpContext helpers for execution context

---

## 🚀 Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- ASP.NET Core application project

### Installation

#### Via NuGet (Recommended)

```bash
# Install all packages
dotnet add package TaksiApp.Shared.Kernel
dotnet add package TaksiApp.Shared.Application
dotnet add package TaksiApp.Shared.Api
dotnet add package TaksiApp.Shared.Infrastructure
dotnet add package TaksiApp.Shared.Observability
dotnet add package TaksiApp.Shared.Extensions
```

#### Via Project Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\shared\TaksiApp.Shared.Kernel\TaksiApp.Shared.Kernel.csproj" />
  <ProjectReference Include="..\shared\TaksiApp.Shared.Application\TaksiApp.Shared.Application.csproj" />
  <ProjectReference Include="..\shared\TaksiApp.Shared.Api\TaksiApp.Shared.Api.csproj" />
  <ProjectReference Include="..\shared\TaksiApp.Shared.Infrastructure\TaksiApp.Shared.Infrastructure.csproj" />
  <ProjectReference Include="..\shared\TaksiApp.Shared.Observability\TaksiApp.Shared.Observability.csproj" />
  <ProjectReference Include="..\shared\TaksiApp.Shared.Extensions\TaksiApp.Shared.Extensions.csproj" />
</ItemGroup>
```

---

## ⚡ Quick Start

### Minimal Setup

**Program.cs**

```csharp
using TaksiApp.Shared.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register shared services
builder.Services
    .AddSharedApplication(builder.Configuration)      // Application contracts
    .AddSharedInfrastructure()                        // Infrastructure implementations
    .AddSharedObservability(                          // OpenTelemetry stack
        builder.Configuration,
        serviceName: "MyService",
        serviceVersion: "1.0.0");

builder.Services.AddControllers();

var app = builder.Build();

// Add correlation ID middleware (must be early in pipeline)
app.UseCorrelationId();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

**appsettings.json**

```json
{
  "Observability": {
    "OtlpEndpoint": "http://localhost:4317",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableLogging": true,
    "ServiceVersion": "1.0.0"
  }
}
```

That's it! Your service now has:
- ✅ Correlation ID tracking
- ✅ Distributed tracing
- ✅ Metrics collection
- ✅ Structured logging enrichment
- ✅ Health checks

---

## 📖 Usage Guide

### Working with Value Objects

```csharp
using TaksiApp.Shared.Kernel.ValueObjects;
using TaksiApp.Shared.Kernel.Results;

// Create and validate email
var emailResult = Email.Create("user@example.com");
if (emailResult.IsFailure)
{
    // Handle validation error
    var error = emailResult.Error;
    Console.WriteLine($"{error.Code}: {error.Message}");
    return;
}

var email = emailResult.Value;
Console.WriteLine(email.Value); // "user@example.com"

// Create money with currency
var moneyResult = Money.Create(99.99m, "USD");
var money = moneyResult.Value;

// Arithmetic operations (same currency only)
var total = money + Money.Create(10.00m, "USD").Value;
Console.WriteLine(total); // "109.99 USD"

// Phone number validation
var phoneResult = PhoneNumber.Create("1", "5551234567");
var phone = phoneResult.Value;
Console.WriteLine(phone.FullNumber); // "+15551234567"
```

### Using the Result Pattern

```csharp
using TaksiApp.Shared.Kernel.Results;

public class OrderService
{
    public Result<Order> CreateOrder(CreateOrderRequest request)
    {
        // Validate input
        if (request.Quantity <= 0)
        {
            return Result.Failure<Order>(
                Error.Validation(
                    "Order.InvalidQuantity",
                    "Quantity must be greater than zero"));
        }

        // Business logic
        var order = new Order(request);
        
        return Result.Success(order);
    }
}

// Usage
var result = orderService.CreateOrder(request);

result.Match(
    onSuccess: order => Console.WriteLine($"Created order {order.Id}"),
    onFailure: error => Console.WriteLine($"Failed: {error.Message}")
);

// Or use pattern matching
if (result.IsSuccess)
{
    var order = result.Value;
    // Use order
}
else
{
    var error = result.Error;
    // Handle error
}
```

### API Controllers with Result Pattern

```csharp
using TaksiApp.Shared.Api.Controllers;
using TaksiApp.Shared.Kernel.Results;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ApiControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public IActionResult CreateOrder(CreateOrderRequest request)
    {
        var result = _orderService.CreateOrder(request);

        // Automatically converts Result to ProblemDetails on failure
        return result.Match(
            onSuccess: order => Ok(order),
            onFailure: error => Problem(error)
        );
    }
}
```

**Error response example:**

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Error",
  "status": 400,
  "detail": "Quantity must be greater than zero",
  "instance": "/api/orders",
  "errorCode": "Order.InvalidQuantity",
  "timestamp": "2026-02-02T20:30:00Z",
  "traceId": "00-abc123...",
  "spanId": "def456..."
}
```

### Working with Execution Context

```csharp
using TaksiApp.Shared.Application.Abstractions;

public class OrderService
{
    private readonly IExecutionContext _executionContext;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IExecutionContext executionContext, ILogger<OrderService> logger)
    {
        _executionContext = executionContext;
        _logger = logger;
    }

    public void ProcessOrder(Order order)
    {
        // Access correlation ID (always available)
        _logger.LogInformation(
            "Processing order {OrderId} with correlation {CorrelationId}",
            order.Id,
            _executionContext.CorrelationId);

        // Access user context (if authenticated)
        if (_executionContext.UserId != null)
        {
            _logger.LogInformation("User {UserId} placed order", _executionContext.UserId);
        }

        // Multi-tenancy support
        if (_executionContext.TenantId != null)
        {
            // Ensure order belongs to user's tenant
            _executionContext.EnsureTenantAccess(order.TenantId);
        }
    }
}
```

### Customizing Middleware User/Tenant Extraction

```csharp
// Program.cs
app.UseCorrelationId(
    userIdExtractor: ctx => ctx.User.FindFirst("sub")?.Value,
    tenantIdExtractor: ctx => ctx.User.FindFirst("organization_id")?.Value
);
```

### Recording Custom Metrics

```csharp
using TaksiApp.Shared.Observability.Metrics;

public class PaymentService
{
    private readonly ApplicationMetrics _metrics;

    public PaymentService(ApplicationMetrics metrics)
    {
        _metrics = metrics;
    }

    public async Task ProcessPaymentAsync(Payment payment)
    {
        using var _ = _metrics.TrackActiveRequest();
        
        try
        {
            await ProcessAsync(payment);
        }
        catch (PaymentGatewayException ex)
        {
            _metrics.RecordError("PaymentGatewayFailure", "ProcessPayment");
            throw;
        }
    }
}
```

### Creating Custom Metrics

```csharp
using TaksiApp.Shared.Observability.Metrics;

public class CustomMetrics
{
    private readonly Counter<long> _orderCount;
    private readonly Histogram<double> _orderValue;

    public CustomMetrics(MeterProvider meterProvider)
    {
        _orderCount = meterProvider.CreateCounter<long>(
            "orders.created.count",
            unit: "orders",
            description: "Total orders created");

        _orderValue = meterProvider.CreateHistogram<double>(
            "orders.value",
            unit: "USD",
            description: "Order value distribution");
    }

    public void RecordOrder(decimal value)
    {
        _orderCount.Add(1);
        _orderValue.Record((double)value);
    }
}
```

### Domain Events

```csharp
using TaksiApp.Shared.Kernel.Abstractions;
using TaksiApp.Shared.Kernel.Common;
using TaksiApp.Shared.Kernel.Events;

// Define domain event
public sealed record OrderPlacedEvent(Guid OrderId, decimal TotalAmount) : DomainEventBase;

// Aggregate root raises events
public class Order : AggregateRoot<Guid>
{
    public Order(Guid id, decimal totalAmount)
    {
        Id = id;
        TotalAmount = totalAmount;

        // Raise domain event
        RaiseDomainEvent(new OrderPlacedEvent(id, totalAmount));
    }

    public decimal TotalAmount { get; private set; }
}

// Collect and publish events (typically in infrastructure layer)
public class OrderRepository
{
    public async Task SaveAsync(Order order)
    {
        await _dbContext.SaveChangesAsync();

        // Publish domain events
        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent);
        }

        order.ClearDomainEvents();
    }
}
```

### Using Guard Clauses

```csharp
using TaksiApp.Shared.Kernel.Guards;

public class Order
{
    public Order(string customerName, decimal totalAmount)
    {
        Guard.NotNullOrEmpty(customerName); // Automatically uses parameter name
        Guard.GreaterThan(totalAmount, 0m);

        CustomerName = customerName;
        TotalAmount = totalAmount;
    }

    public string CustomerName { get; }
    public decimal TotalAmount { get; }
}

// Advanced guards
public void ProcessOrder(Order order, int quantity)
{
    Guard.NotNull(order);
    Guard.InRange(quantity, 1, 1000);
    Guard.Require<InvalidOperationException>(
        order.Status == OrderStatus.Pending,
        "Order must be pending to process"
    );
}
```

### HttpClient with Correlation ID Propagation

```csharp
// Program.cs - Register HttpClient with correlation ID handler
builder.Services
    .AddHttpClient<IPaymentGateway, PaymentGatewayClient>()
    .AddCorrelationIdHandler(); // Automatically propagates X-Correlation-Id

// Usage (correlation ID automatically added to requests)
public class PaymentGatewayClient : IPaymentGateway
{
    private readonly HttpClient _httpClient;

    public PaymentGatewayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentResult> ProcessAsync(Payment payment)
    {
        // X-Correlation-Id header automatically added
        var response = await _httpClient.PostAsJsonAsync("/payments", payment);
        return await response.Content.ReadFromJsonAsync<PaymentResult>();
    }
}
```

---

## ⚙️ Configuration

### Observability Settings

Configure OpenTelemetry exporters in **appsettings.json**:

```json
{
  "Observability": {
    "OtlpEndpoint": "http://jaeger:4317",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableLogging": true,
    "ServiceVersion": "1.2.0"
  }
}
```

### Custom OpenTelemetry Configuration

```csharp
builder.Services.AddSharedObservability(
    builder.Configuration,
    serviceName: "OrderService",
    serviceVersion: "2.0.0",
    configureOtel: otel =>
    {
        // Add custom instrumentation
        otel.WithTracing(tracing => tracing
            .AddRedisInstrumentation()
            .AddMongoDBInstrumentation());
    });
```

### Custom ProblemDetails Type URLs

```csharp
using TaksiApp.Shared.Api.Factories;

builder.Services.AddSingleton<ProblemDetailsFactory>(
    new SharedProblemDetailsFactory(
        typeUrlFactory: statusCode => $"https://docs.myapi.com/errors/{statusCode}"
    ));
```

### Disabling Features

```csharp
// Disable observability entirely (not recommended for production)
// Simply don't call AddSharedObservability()

// Or disable specific features in appsettings.json
{
  "Observability": {
    "EnableTracing": false,  // Disable tracing only
    "EnableMetrics": true,
    "EnableLogging": true
  }
}
```

---

## 🎯 Best Practices

### Microservice Integration

1. **Register services in correct order:**
   ```csharp
   builder.Services
       .AddSharedApplication(configuration)     // 1. Contracts first
       .AddSharedInfrastructure()               // 2. Infrastructure second
       .AddSharedObservability(...)             // 3. Observability third
       .AddMediatR(...)                         // 4. Your application services last
   ```

2. **Add middleware early in pipeline:**
   ```csharp
   app.UseCorrelationId();    // Must be early
   app.UseAuthentication();
   app.UseAuthorization();
   app.MapControllers();
   ```

3. **Always use Result pattern for business operations:**
   ```csharp
   // ❌ BAD - Exceptions for control flow
   public Order CreateOrder(CreateOrderRequest request)
   {
       if (request.Quantity <= 0)
           throw new ValidationException("Invalid quantity");
       // ...
   }

   // ✅ GOOD - Result pattern
   public Result<Order> CreateOrder(CreateOrderRequest request)
   {
       if (request.Quantity <= 0)
           return Result.Failure<Order>(
               Error.Validation("Order.InvalidQuantity", "Quantity must be positive"));
       // ...
   }
   ```

### Observability Best Practices

1. **Use correlation IDs consistently:**
   - Middleware automatically generates/extracts them
   - Propagates via HttpClient handlers
   - Enriches all logs automatically

2. **Create semantic error codes:**
   ```csharp
   // ✅ GOOD - Clear and hierarchical
   Error.Validation("Order.Quantity.TooLow", "Quantity must be at least 1")
   Error.NotFound("Customer.NotFound", "Customer with ID {id} not found")

   // ❌ BAD - Generic and unhelpful
   Error.Validation("Invalid", "Error")
   ```

3. **Record errors with context:**
   ```csharp
   catch (PaymentException ex)
   {
       _metrics.RecordError("PaymentFailure", "ProcessOrder");
       _logger.LogError(ex, "Payment failed for order {OrderId}", orderId);
       throw;
   }
   ```

### Multi-Tenancy

```csharp
public class OrderService
{
    private readonly IExecutionContext _context;
    private readonly OrderRepository _repository;

    public async Task<Result<Order>> GetOrderAsync(Guid orderId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure<Order>(Error.NotFound("Order.NotFound", "Order not found"));

        // Automatically throws if cross-tenant access attempted
        _context.EnsureTenantAccess(order.TenantId);

        return Result.Success(order);
    }
}
```

### Testing with Time Abstraction

```csharp
// Production code
public class OrderService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public Order CreateOrder()
    {
        var order = new Order
        {
            CreatedAt = _dateTimeProvider.UtcNow
        };
        return order;
    }
}

// Test code
public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new DateTime(2026, 1, 1);
}

[Fact]
public void CreateOrder_SetsCorrectTimestamp()
{
    var fakeTime = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 2, 2) };
    var service = new OrderService(fakeTime);

    var order = service.CreateOrder();

    Assert.Equal(new DateTime(2026, 2, 2), order.CreatedAt);
}
```

---

## ❓ FAQ

### Is this library production-ready?

Yes. This library is built with real-world microservice patterns and is designed for production use. It follows Clean Architecture principles and includes comprehensive observability.

### Does this library contain any domain-specific logic?

No. This library is **domain-agnostic**. It contains zero business logic and can be reused across any domain (e-commerce, healthcare, finance, etc.).

### Can I use only parts of the library?

Yes. The library is divided into separate NuGet packages. Install only what you need:
- **Kernel** - DDD building blocks (always recommended)
- **Application** - Execution context abstractions
- **Infrastructure** - DateTime provider and other utilities
- **Api** - Controllers, middleware, error handling
- **Observability** - OpenTelemetry integration
- **Extensions** - DI registration helpers

### How do I add custom value objects?

Value objects in this library must meet strict criteria (see [VALUE_OBJECTS_POLICY.md](VALUE_OBJECTS_POLICY.md)). For domain-specific value objects (like ProductSKU, OrderNumber), create them in your service's domain layer, not in this shared library.

### Can I customize correlation ID header name?

Yes, but not recommended. The library uses the standard `X-Correlation-Id` header. If you need a custom header, modify `HttpContextConstants.CorrelationIdHeaderName`.

### What if I don't use OpenTelemetry?

Simply don't call `AddSharedObservability()`. The rest of the library works independently. However, we strongly recommend using observability in production.

### Can I use this with MediatR?

Yes. Register shared services **before** MediatR:
```csharp
builder.Services
    .AddSharedApplication(configuration)
    .AddSharedInfrastructure()
    .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

### Does this work with Entity Framework Core?

Yes. The `Entity<TId>` base class handles EF Core proxies correctly. The library includes EF Core instrumentation in OpenTelemetry tracing.

### How do I upgrade to future versions?

We follow **semantic versioning**:
- **Patch** (1.0.x) - Bug fixes, backward compatible
- **Minor** (1.x.0) - New features, backward compatible
- **Major** (x.0.0) - Breaking changes (includes migration guide)

### What about API versioning?

This library doesn't include API versioning. Use [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) or similar libraries.

---

## 🚨 Important Notes

### Metadata Usage Warning

`ExecutionContextMetadata` is for **technical context only** (correlation, debugging). Never store:
- ❌ Business rules or decisions
- ❌ User preferences or settings
- ❌ Entity state or domain data
- ❌ Authorization decisions

Use proper domain objects for business data.

### Value Object Growth

This library includes only **universal** value objects:
- ✅ Email (RFC 5322 standard)
- ✅ Money (ISO 4217 standard)
- ✅ PhoneNumber (ITU-T E.164 standard)

Domain-specific value objects (Address, CreditCard, SKU) belong in your service's domain layer, not here.

### Static State

This library contains **zero static state**. Everything is injectable via DI for maximum testability.

---

## 📚 Additional Resources

- [CHANGELOG.md](CHANGELOG.md) - Version history and release notes
- [VALUE_OBJECTS_POLICY.md](VALUE_OBJECTS_POLICY.md) - Guidelines for adding value objects
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Robert C. Martin
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/) - Official documentation
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) - Result pattern explained

---

## 🤝 Contributing

Contributions are welcome! Please:

1. Follow existing code style and patterns
2. Add XML documentation to all public APIs
3. Include unit tests for new features
4. Update README.md if adding user-facing features
5. Ensure all tests pass before submitting PR

**Before adding new features**, please open an issue to discuss whether it fits the library's scope (domain-agnostic, reusable, production-ready).

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 💬 Support

For questions or issues:
1. Check the [FAQ](#-faq) section
2. Search existing [GitHub Issues](https://github.com/your-org/taksiapp-shared/issues)
3. Open a new issue with detailed description and code samples

---

**Built with ❤️ for the ASP.NET Core community**