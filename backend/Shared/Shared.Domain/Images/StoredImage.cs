namespace Shared.Domain.Images;

// One file on disk. Address has exactly the shape ImageStore returns when saving, so it can be matched against the addresses a module uses.
public sealed record StoredImage
{
    public string Module { get; }
    public string Address { get; }
    public DateTime WrittenAt { get; }

    public StoredImage(string module, string address, DateTime writtenAt)
    {
        Module = module;
        Address = address;
        WrittenAt = writtenAt;
    }
}
