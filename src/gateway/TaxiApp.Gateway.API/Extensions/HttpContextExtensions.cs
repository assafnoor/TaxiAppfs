namespace TaxiApp.Gateway.Api.Extensions;

public static class HttpContextExtensions
{
    public static string? GetProxiedDestination(this HttpContext context)
    {
        var revProxyFeature = context.GetReverseProxyFeature();
        return revProxyFeature?.ProxiedDestination?.Model.Config.Address;
    }
}