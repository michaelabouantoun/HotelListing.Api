using HotelListing.Api.Common.Constants;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace HotelListing.Api.AuthorizationFilters;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class HotelOrSystemAdminAttribute : TypeFilterAttribute
{
    public HotelOrSystemAdminAttribute() : base(typeof(HotelOrSystemAdminFilter))
    {
    }
}
public class HotelOrSystemAdminFilter(HotelListingDbContext dbContext) : IAsyncAuthorizationFilter
{
    //if the administrator of the system attempts to cancel or confirm or getBookings of a hotel,
    //the administrator of the system is not a hotel admin unless we're going to add that user to every hotel,
    //which i wouldnt necessarily want to do that means we need a cleaner way to kind of handle that authorization or that policiy 
    //everytime we're trying to do something with the admin or the hotelAdmin,we keep on checking(in the booking service code) if this is a hotel admin  
    //and we dont want that everytime so we actually want to create a filter that we can use to decorate the actual action instead of needing authorize[Roles="HotelAdmin, Administrator"]
    //and then check if its a hotel admin we want to just be able to put something here that says protect this endpoint with logic so thats why we do this attribut filter
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context) //called early in the filter pipeline to confirm request is authorized
    {
        var httpUser = context.HttpContext.User;
        if (httpUser?.Identity?.IsAuthenticated == false)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        //If user is a global Administrator, allow immediately
        if (httpUser!.IsInRole(RoleNames.Administrator))
        {
            return; //it will allow them through
        }
        var userId = httpUser.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? httpUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new ForbidResult();
            return;
        }
        //Try to get hotelId from route values
        context.RouteData.Values.TryGetValue("hotelId", out var hotelIdObj);
        int.TryParse(hotelIdObj?.ToString(), out int hotelId);
        if (hotelId == 0)
        {
            context.Result = new ForbidResult();
            return;
        }
        //Check if user is an admin for this specific hotel
        //so we avoid checking it in the service
        var isHotelAdminUser = await dbContext.HotelAdmins
          .AnyAsync(q => q.UserId == userId && q.HotelId == hotelId);
        if (!isHotelAdminUser)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}