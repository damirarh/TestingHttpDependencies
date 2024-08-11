using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace OrchestrationApi.DataAnnotations;

public class CountryCodeAttribute()
    : ValidationAttribute(() => "The {0} field is not a valid ISO 3166-1 alpha-2 code.")
{
    public override bool IsValid(object? value)
    {
        return CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name))
            .Any(region => region.TwoLetterISORegionName == value?.ToString());
    }
}
