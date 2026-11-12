namespace Shared.Infrastructure.Images;

// Declared here and implemented in a module's Infrastructure, so the cleanup job can ask a module which images it uses
// without the shared code depending on any module.
public interface IImageUsage
{
    // The same module name the module passes to ImageStore.SaveAsync, which is also the name of its subfolder.
    string Module { get; }

    Task<IReadOnlySet<string>> GetUsedAddressesAsync();
}
