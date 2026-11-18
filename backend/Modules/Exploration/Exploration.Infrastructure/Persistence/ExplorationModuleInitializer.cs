using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public ExplorationModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExplorationDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Tours.AnyAsync())
        {
            dbContext.Tours.AddRange(CreateDemoTours());
            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<Tour> CreateDemoTours()
    {
        var fortress = new Tour(FirstExplorer, "Šetnja Petrovaradinskom tvrđavom",
            "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav i stari deo Novog Sada.",
            TourDifficulty.Easy, ["istorija", "priroda"], new RecommendedGroupSize(2, 15));
        fortress.AddTransportTime(TransportMode.Walking, 120);
        fortress.AddKeyPoint("Gornji plato", "Polazna tačka šetnje, sa pogledom na Dunav i Novi Sad.", "/images/exploration/demo-gornji-plato.jpg", new Location(45.2524, 19.8622), null, true);
        fortress.AddKeyPoint("Sahat kula", "Kula sa satom čije su kazaljke obrnute, da bi se sa reke lakše čitali sati.", "/images/exploration/demo-sahat-kula.jpg", new Location(45.2532, 19.8614), null, false);
        fortress.Publish(new TourPrice(true, null));

        var fruskaGora = new Tour(FirstExplorer, "Biciklom preko Fruške gore",
            "Ruta vodi od Sremske Kamenice preko Popovice do Iriškog venca, pored manastira Beočin i vidikovaca sa kojih se vidi ceo Srem, uz nekoliko dužih uspona.",
            TourDifficulty.Hard, ["biciklizam", "planina"], new RecommendedGroupSize(2, 6));
        fruskaGora.AddTransportTime(TransportMode.Bicycle, 240);
        fruskaGora.AddKeyPoint("Sremska Kamenica", "Polazna tačka rute, u podnožju Fruške gore.", "/images/exploration/demo-sremska-kamenica.jpg", new Location(45.2195, 19.8406), null, true);
        fruskaGora.AddKeyPoint("Iriški venac", "Najviša tačka rute, sa spomenikom i vidikovcem.", "/images/exploration/demo-iriski-venac.jpg", new Location(45.1513, 19.8517), null, false);
        fruskaGora.Publish(new TourPrice(true, null));

        var cityCenter = new Tour(FirstExplorer, "Obilazak centra grada",
            "Kratak obilazak Trga slobode, Dunavske ulice i Dunavskog parka.",
            TourDifficulty.Moderate, ["grad"], new RecommendedGroupSize(1, 10));

        return [fortress, fruskaGora, cityCenter];
    }
}
