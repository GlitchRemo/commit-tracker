namespace NGrid.Customer.GraphQl.Gateway;

public static class Extensions
{
    public static void LogError<T>(this IError error, ILogger<T> logger)
    {
        logger.LogInformation("Error: ({Code}) '{Path}' {Error}", error.Code, error.Path, error.Message);
    }
}