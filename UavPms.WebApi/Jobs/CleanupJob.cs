using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UavPms.WebApi.Jobs;

public class CleanupJob
{
    private readonly ILogger<CleanupJob> _logger;

    public CleanupJob(ILogger<CleanupJob> logger)
    {
        _logger = logger;
    }

    public Task Execute()
    {
        _logger.LogInformation("Auto-Cleanup job started: Purging temporary files and logs older than 30 days...");

        // Simulate file/log purging in the storage folder
        var imagePath = "/home/an/uav_storage/images";
        try
        {
            if (Directory.Exists(imagePath))
            {
                var directoryInfo = new DirectoryInfo(imagePath);
                var thresholdDate = DateTime.UtcNow.AddDays(-30);
                int deletedCount = 0;

                foreach (var file in directoryInfo.GetFiles())
                {
                    if (file.CreationTimeUtc < thresholdDate)
                    {
                        file.Delete();
                        deletedCount++;
                    }
                }

                _logger.LogInformation("Auto-Cleanup complete. Purged {Count} files older than 30 days.", deletedCount);
            }
            else
            {
                _logger.LogWarning("Auto-Cleanup: Storage directory '{Path}' does not exist. Skipping file purge.", imagePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during Auto-Cleanup job execution.");
        }

        return Task.CompletedTask;
    }
}
