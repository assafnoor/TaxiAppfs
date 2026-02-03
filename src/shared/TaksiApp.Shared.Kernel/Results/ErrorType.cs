// TaksiApp.Shared.Kernel/Results/ErrorType.cs
namespace TaksiApp.Shared.Kernel.Results;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure,
    Unauthorized,
    Forbidden
}
