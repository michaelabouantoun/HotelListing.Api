using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

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
