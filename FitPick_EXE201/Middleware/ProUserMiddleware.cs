using FitPick_EXE201.Helpers;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FitPick_EXE201.Middleware
{
    public class ProUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ProUserMiddleware> _logger;

        public ProUserMiddleware(RequestDelegate next, ILogger<ProUserMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ProUserService proUserService)
        {
            var endpoint = context.GetEndpoint();
            var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (controllerActionDescriptor != null)
            {
                var requiresProUserAttribute = controllerActionDescriptor.MethodInfo
                    .GetCustomAttributes(typeof(RequiresProUserAttribute), true)
                    .FirstOrDefault() as RequiresProUserAttribute;

                if (requiresProUserAttribute != null)
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    {
                        _logger.LogWarning("ProUserMiddleware: User ID not found in token for endpoint {Endpoint}", 
                            context.Request.Path);
                        
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(ApiResponse<object>.ErrorResponse(
                            new List<string> { "Unauthorized: User ID not found in token." }, "Unauthorized"));
                        return;
                    }

                    var isPro = await proUserService.IsProUserAsync(userId);
                    if (!isPro)
                    {
                        _logger.LogWarning("ProUserMiddleware: User {UserId} attempted to access Pro feature {Feature} at {Endpoint}", 
                            userId, requiresProUserAttribute.Feature, context.Request.Path);
                        
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(ApiResponse<object>.ErrorResponse(
                            new List<string> { $"Forbidden: This feature '{requiresProUserAttribute.Feature}' requires a Pro user account." }, "Forbidden"));
                        return;
                    }

                    _logger.LogInformation("ProUserMiddleware: User {UserId} successfully accessed Pro feature {Feature}", 
                        userId, requiresProUserAttribute.Feature);
                }
            }

            await _next(context);
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequiresProUserAttribute : Attribute
    {
        public string? Feature { get; set; }
        
        // Constructor with feature parameter
        public RequiresProUserAttribute(string feature)
        {
            Feature = feature;
        }
        
        // Parameterless constructor for property initializer syntax
        public RequiresProUserAttribute()
        {
        }
    }
}
