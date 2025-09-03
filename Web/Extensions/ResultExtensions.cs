using LinaSys.Shared.Application;

namespace LinaSys.Web.Extensions;

public static class ResultExtensions
{
    public static void DemandSuccessResult(this Result result, string? exceptionMessage = null)
    {
        if (!result.IsSuccess)
        {
            throw new Exception(exceptionMessage);
        }
    }
}
