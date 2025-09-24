using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.DTOs.Booking;

public record CreateBookingDto(
    DateOnly CheckIn,
    DateOnly CheckOut,
    [Required]
    [Range(minimum:1,maximum:10)]
    int Guests
    ) : IValidatableObject //you add custom validation logic beyond what data annotations can express
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckOut <= CheckIn)
        {
            yield return new ValidationResult("Check-out must be after checki-in.", [nameof(CheckOut), nameof(CheckIn)]); //returning custom error messages tied to specefic fields
        }
    }
}
/*ASP.NET validation calls your Validate() method.

It starts executing:

If CheckOut <= CheckIn, the first yield return triggers → sends back a ValidationResult.

The method pauses here, but isn’t finished.

ASP.NET says: “Do you have more results?”

The method resumes after the last yield.

It checks the Guests <= 0 condition.

If true, it yields another error.

Only when the method has no more yield return, the iteration ends.*/
/*Role of yield in C#

Produce values one at a time

Instead of building and returning a whole collection, you can emit(yield return) each value as you find it.

Keep the method “alive” until finished

Unlike return, which ends the method immediately, yield return pauses execution, gives one result, and then continues from the next line when asked for more.

Support multiple results in a simple way

In validation, you may have many possible errors.

yield return lets you list them naturally:*/