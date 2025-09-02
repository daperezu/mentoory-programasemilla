using LinaSys.BusinessIncubator.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LinaSys.BusinessIncubator.Infrastructure.Persistence;

public static class Converters
{
    public static ValueConverter<FodaType, char> FodaTypeConverter =>
        new(
            v => v.ToString()[0],
            v => (FodaType)v);

    public static ValueConverter<OdsrType, char> OdsrTypeConverter =>
        new(
            v => v.ToString()[0],
            v => (OdsrType)v);
}
