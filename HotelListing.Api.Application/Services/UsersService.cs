using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Auth;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Config;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace HotelListing.Api.Application.Services;

public class UsersService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtOptions, IHttpContextAccessor httpContextAccessor, HotelListingDbContext hotelListingDbContext) : IUsersService
{
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {

        var user = new ApplicationUser
        {
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            UserName = registerUserDto.Email
        };
        var result = await userManager.CreateAsync(user, registerUserDto.Password); //pass is hashed then stored in the db //this method has a layer of protection at the db level where user manager and identity enfore, like whatever pass length restrictions i put on, if the same username/email is coming in as one that already exists,all of those will in errors and result will not be successful
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Error(ErrorCodes.BadRequest, e.Description)).ToArray();
            return Result<RegisteredUserDto>.BadRequest(errors);
        }
        await userManager.AddToRoleAsync(user, registerUserDto.Role);
        //if Hotel Admin, add to hotelAdmins table
        if (registerUserDto.Role == RoleNames.HotelAdmin)
        {
            var hotelExists = await hotelListingDbContext.Hotels
       .AnyAsync(h => h.Id == registerUserDto.AssociatedHotelId);

            if (!hotelExists)
            {
                return Result<RegisteredUserDto>.Failure(new Error(ErrorCodes.NotFound, $"AssociatedHotel to this hotel admin is not found")); // Or throw / return custom error
            }
            var hotelAdmin = hotelListingDbContext.HotelAdmins.Add(
                new HotelAdmin
                {
                    UserId = user.Id,
                    HotelId = registerUserDto.AssociatedHotelId.GetValueOrDefault()
                });
            await hotelListingDbContext.SaveChangesAsync();
        }
        var registeredUser = new RegisteredUserDto
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Id = user.Id,
            Role = registerUserDto.Role
        };
        //after that means result succeed we can like send confirmation email
        return Result<RegisteredUserDto>.Success(registeredUser);
    }
    public async Task<Result<string>> LoginAsync(LoginUserDto loginUserDto)
    {
        var user = await userManager.FindByEmailAsync(loginUserDto.Email);
        if (user == null)
        {
            return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials"));
        }
        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginUserDto.Password); //note: because the object is tracked this method it doesnt refetch from the memory (why async? only for API consistency because other UserManager methods usually hits the store
        //the method take the user.PasswordHash it applies the same hashing alg to the input pass it compares result ...
        if (!isPasswordValid)
        {
            return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials"));
        }
        //Issue a token
        var token = await GenerateToken(user);

        return Result<string>.Success(token);
    }
    // we create that for a dry principle
    public string UserId => httpContextAccessor? //every Http request in asp.net core has a HttpContext, inside it,HttpContext.User represents the current authenticated user if the request went through authentication middleware(Jwtbearer),the middleware populates User With a ClaimsPrincipal that has claims 
        .HttpContext?//but if the request is anonymous or authentication failed or you didnt configure authentication at all, then HttpContext.User will be empty
        .User?
        .FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? httpContextAccessor?
        .HttpContext?
        .User?
        .FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private async Task<string> GenerateToken(ApplicationUser user)
    {
        //set basic user claims
        var claims = new List<Claim>
        {
         new(JwtRegisteredClaimNames.Sub,user.Id),
         new(JwtRegisteredClaimNames.Email,user.Email),
         new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
         new(JwtRegisteredClaimNames.Name,user.FullName),

        };

        //set user role claims
        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
        claims = claims.Union(roleClaims).ToList();

        //set jwt key credentials
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //create an encoded token
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtOptions.Value.DurationInMinutes)),
            signingCredentials: credentials
            );


        //return token value
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
