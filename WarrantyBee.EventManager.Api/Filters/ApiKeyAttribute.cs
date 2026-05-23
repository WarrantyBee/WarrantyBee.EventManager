using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WarrantyBee.EventManager.Application.Abstractions.Services;

namespace WarrantyBee.EventManager.Api.Filters;

/// <summary>
/// Filter that validates the X-API-KEY header using the <see cref="IApiKeyService"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("API Key is missing.");
            return;
        }

        var apiKeyService = context.HttpContext.RequestServices.GetRequiredService<IApiKeyService>();
        var clientName = apiKeyService.ValidateKey(extractedApiKey!);

        if (string.IsNullOrEmpty(clientName))
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key.");
            return;
        }

        context.HttpContext.Items["ClientName"] = clientName;

        await next();
    }
}
