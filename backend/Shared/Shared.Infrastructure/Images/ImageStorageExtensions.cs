using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Images;

public static class ImageStorageExtensions
{
    public static IServiceCollection AddImageStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var folder = configuration["ImageStorage:Folder"];
        if (string.IsNullOrWhiteSpace(folder))
        {
            throw new InvalidOperationException("Configuration value 'ImageStorage:Folder' is missing.");
        }

        return services.AddSingleton(new ImageStore(folder));
    }
}
