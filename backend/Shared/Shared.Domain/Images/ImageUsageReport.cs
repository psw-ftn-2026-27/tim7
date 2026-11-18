namespace Shared.Domain.Images;

public sealed record ImageUsageReport
{
    public string Module { get; }

    // Null means the module did not answer, so nothing may be deleted in this run.
    public IReadOnlySet<string>? Addresses { get; }

    public ImageUsageReport(string module, IReadOnlySet<string>? addresses)
    {
        Module = module;
        Addresses = addresses;
    }
}
