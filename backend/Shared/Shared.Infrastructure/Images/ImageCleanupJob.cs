using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.Images;

namespace Shared.Infrastructure.Images;

// Coordinates only: asks the modules, lists the files, lets OrphanImagePolicy decide, deletes and logs a summary.
internal sealed class ImageCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ImageStore _imageStore;
    private readonly ILogger<ImageCleanupJob> _logger;
    private readonly TimeSpan _interval;
    private readonly TimeSpan _minAge;

    public ImageCleanupJob(
        IServiceScopeFactory scopeFactory, ImageStore imageStore, ILogger<ImageCleanupJob> logger, TimeSpan interval, TimeSpan minAge)
    {
        _scopeFactory = scopeFactory;
        _imageStore = imageStore;
        _logger = logger;
        _interval = interval;
        _minAge = minAge;
    }

    // The token is forwarded only to the wait between runs, so the server can stop without waiting out the interval.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync();
            }
            catch (Exception exception)
            {
                // A background job has no middleware above it; an escaped exception would stop the server.
                _logger.LogError(exception, "Image cleanup failed and will run again after the interval.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunOnceAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var usages = scope.ServiceProvider.GetServices<IImageUsage>().ToList();
        if (usages.Count == 0)
        {
            _logger.LogInformation("Image cleanup: no module reports its images, nothing deleted.");
            return;
        }

        var reports = new List<ImageUsageReport>();
        foreach (var usage in usages)
        {
            reports.Add(await AskAsync(usage));
        }

        List<StoredImage> files;
        try
        {
            files = _imageStore.ListStored();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Image cleanup: the image folder {Folder} could not be read, run skipped.", _imageStore.Folder);
            return;
        }

        var selected = OrphanImagePolicy.SelectForDeletion(files, reports, DateTime.UtcNow, _minAge);
        var deleted = _imageStore.Delete(selected);

        foreach (var image in selected.Except(deleted))
        {
            _logger.LogWarning("Image cleanup: {Address} could not be deleted and will be retried on the next run.", image.Address);
        }

        var inUse = files.Count(file => reports.Any(report =>
            report.Module == file.Module && report.Addresses is not null && report.Addresses.Contains(file.Address)));
        _logger.LogInformation(
            "Image cleanup: {Checked} files checked, {InUse} in use, {Deleted} deleted: {DeletedFiles}",
            files.Count, inUse, deleted.Count, string.Join(", ", deleted.Select(image => image.Address)));
    }

    // An exception becomes a report without addresses, which makes the policy delete nothing in this run.
    private async Task<ImageUsageReport> AskAsync(IImageUsage usage)
    {
        try
        {
            return new ImageUsageReport(usage.Module, await usage.GetUsedAddressesAsync());
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Image cleanup: module {Module} did not report its images, nothing will be deleted.", usage.Module);
            return new ImageUsageReport(usage.Module, null);
        }
    }
}
