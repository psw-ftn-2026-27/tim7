namespace Shared.Domain.Images;

// Decides which stored images are orphans that may be deleted. Reads no disk, calls no module and logs nothing.
public static class OrphanImagePolicy
{
    public static List<StoredImage> SelectForDeletion(
        IReadOnlyList<StoredImage> files, IReadOnlyList<ImageUsageReport> reports, DateTime now, TimeSpan minAge)
    {
        if (reports.Any(report => report.Addresses is null))
        {
            return [];
        }

        var usedByModule = reports.ToDictionary(report => report.Module, report => report.Addresses!);

        return files
            .Where(file => usedByModule.TryGetValue(file.Module, out var used) && !used.Contains(file.Address))
            .Where(file => now - file.WrittenAt >= minAge)
            .ToList();
    }
}
