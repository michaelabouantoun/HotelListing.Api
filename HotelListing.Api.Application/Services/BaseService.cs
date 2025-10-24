using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelListing.Api.Application.Services;

public abstract class BaseService
{
    protected bool ValidateDto<T>(T dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
       return Validator.TryValidateObject(dto, context, results, true);
        
    }

}
