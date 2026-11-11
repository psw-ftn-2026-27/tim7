
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new RecommendedGroupSize(2, 10), new TourLength(7.5m));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new RecommendedGroupSize(1, 4), null);
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new RecommendedGroupSize(3, 3), null);
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour PublishedBikeRideToPetrovaradin;
    public static readonly Tour PublishedOldTownWalk;
    public static readonly Tour ArchivedLakeside;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new RecommendedGroupSize(1, 8), null);
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new RecommendedGroupSize(2, 6), null);
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.Publish();

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new RecommendedGroupSize(4, 12), null);
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.Publish();

        PublishedBikeRideToPetrovaradin = new(WellKnownUsers.SecondExplorer, "Biciklom do Petrovaradina", "Vožnja kreće sa Keja žrtava racije, prelazi Varadinski most i penje se do Petrovaradinske tvrđave, uz obilazak Sahat kule i povratak kroz Podgrađe.", TourDifficulty.Easy, ["grad", "istorija"], new RecommendedGroupSize(1, 6), null);
        PublishedBikeRideToPetrovaradin.AddTransportTime(TransportMode.Bicycle, 40);
        PublishedBikeRideToPetrovaradin.AddTransportTime(TransportMode.Walking, 120);
        PublishedBikeRideToPetrovaradin.Publish();

        PublishedOldTownWalk = new(WellKnownUsers.SecondExplorer, "Peške kroz staro jezgro", "Šetnja vodi od Trga slobode, kroz Zmaj Jovinu i Dunavsku ulicu, pored Vladičanskog dvora i Saborne crkve, do Dunavskog parka i Muzeja Vojvodine.", TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 15), null);
        PublishedOldTownWalk.AddTransportTime(TransportMode.Walking, 75);
        PublishedOldTownWalk.Publish();

        ArchivedLakeside = new(WellKnownUsers.Explorer, "Obala Palićkog jezera", "Tura prati obalu Palićkog jezera od Velike terase do Ženskog štranda, uz obilazak Vodotornja i zoološkog vrta na severnoj strani jezera.", TourDifficulty.Easy, ["priroda", "jezero"], new RecommendedGroupSize(2, 8), null);
        ArchivedLakeside.AddTransportTime(TransportMode.Walking, 75);
        ArchivedLakeside.Publish();
        ArchivedLakeside.Archive();
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, PublishedBikeRideToPetrovaradin, PublishedOldTownWalk, ArchivedLakeside];
}
