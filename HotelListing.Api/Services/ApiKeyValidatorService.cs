using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class ApiKeyValidatorService(HotelListingDbContext db) : IApiKeyValidatorService
{
    public async Task<bool> IsValidAsync(string apiKey, CancellationToken ct = default)
    {
        var apiKeyEntity = await db.ApiKey
             .AsNoTracking()
             .FirstOrDefaultAsync(k => k.Key == apiKey, ct);
        if (apiKeyEntity == null) return false;
        return apiKeyEntity.IsActive;
    }
}
