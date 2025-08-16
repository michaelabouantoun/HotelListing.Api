using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Controllers; //This means all code in the file is automatically in that namespace without wrapping it in { }.

[Route("api/[controller]")]
[ApiController]
public class CountriesController(HotelListingDbContext context) : ControllerBase //i can write the constructor directly and context is accessed by all the class member //and i can do the injection in this way
{

    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
    {
        var countries = await context.Countries.ToListAsync();
        //process countries// its better than return directly
        return countries;
    }

    // GET: api/Countries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Country>> GetCountry(int id)
    {
        var country = await context.Countries
            .Include(c=>c.Hotels) //Eager loading the Hotels navigation property
            .FirstOrDefaultAsync(q=>q.CountryId==id);

        if (country == null)
        {
            return NotFound();
        }

        return country;
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCountry(int id, Country country)
    {
        if (id != country.CountryId)
        {
            return BadRequest();
        }

        context.Entry(country).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CountryExistsAsync(id)) //its better to be async because acces to the db
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry(Country country)
    {
        context.Countries.Add(country);
        await context.SaveChangesAsync(); //entebeh may throw exception if id already exist

        return CreatedAtAction("GetCountry", new { id = country.CountryId }, country);
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var country = await context.Countries.FindAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        context.Countries.Remove(country);
        await context.SaveChangesAsync(); //delete didnt throw exception

        return NoContent();
    }

    private async Task<bool> CountryExistsAsync(int id) //its better to be async because access to the db
    {
        return await context.Countries.AnyAsync(e => e.CountryId == id);
    }
}
