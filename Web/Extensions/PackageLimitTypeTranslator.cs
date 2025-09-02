using LinaSys.Subscription.Application.BusinessIncubatorPackage.Queries;

namespace LinaSys.Web.Extensions;

public static class PackageLimitTypeTranslator
{
    public static string ToView(this PackageLimit packageLimit) => packageLimit.Type switch
    {
        1 => "Proyectos",
        2 => "Usuarios",
        _ => "- desconocido -",
    };
}
