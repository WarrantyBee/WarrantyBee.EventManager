using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WarrantyBee.EventManager.Application.Abstractions.Services;

namespace WarrantyBee.EventManager.Api.Filters;

/// <summary>
/// Filter that validates the X-APP-ID and X-APP-SECRET headers using the <see cref="IApiKeyService"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string AppIdHeaderName = "X-APP-ID";
    private const string AppSecretHeaderName = "X-APP-SECRET";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue(AppIdHeaderName, out var appId) || 
            !request.Headers.TryGetValue(AppSecretHeaderName, out var appSecret))
        {
            context.Result = new UnauthorizedObjectResult("API credentials (X-APP-ID/X-APP-SECRET) are missing.");
            return;
        }

        var apiKeyService = context.HttpContext.RequestServices.GetRequiredService<IApiKeyService>();
        var isValid = await apiKeyService.ValidateAsync(appId!, appSecret!);

        if (!isValid)
        {
            context.Result = new UnauthorizedObjectResult("Invalid API credentials.");
            return;
        }

        await next();
    }
}
