using Microsoft.Extensions.Logging;

namespace VipTags.Utilities;

public static class AsyncHelpers
{
    public static void RunWithErrorLogging(ILogger logger, Func<Task> action)
    {
        Task.Run(async () =>
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occurred in a background task");
                throw;
            }
        });
    }
}