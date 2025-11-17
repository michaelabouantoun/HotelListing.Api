using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace HotelListing.Api.CachePolicies;

// Your custom override of the DefaultPolicy
internal sealed class AuthenticatedUserCachingPolicy : IOutputCachePolicy
{
    public static readonly AuthenticatedUserCachingPolicy Instance = new();

    public AuthenticatedUserCachingPolicy()
    {
    }

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        var attemptOutputCaching = AttemptOutputCaching(context);

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = attemptOutputCaching;
        context.AllowCacheStorage = attemptOutputCaching;

        // Allow locking so multiple requests don't all regenerate the response
        context.AllowLocking = true;

        // Vary by all query parameters by default
        context.CacheVaryByRules.QueryKeys = "*";
        var user=context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated == true) { 
        var userId=user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                context.CacheVaryByRules.VaryByValues.Add("UserId",userId);
            }
        
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    private static bool AttemptOutputCaching(OutputCacheContext context)
    {
        var request = context.HttpContext.Request;

        // Only GET or HEAD are cacheable
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
        {
            return false;
        }

      
        return true;
    }
}
