
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new RecommendedGroupSize(2, 10));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new RecommendedGroupSize(1, 4));
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new RecommendedGroupSize(3, 3));
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour PublishedBikeRideToPetrovaradin;
    public static readonly Tour PublishedOldTownWalk;
    public static readonly Tour ArchivedLakeside;
    public static readonly Tour PublishedFreeTourWithHiddenFirstKeyPoint;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new RecommendedGroupSize(1, 8));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);
        PublishableRiverside.AddKeyPoint("Ribarsko ostrvo", "Polazna tačka staze, na ušću rukavca u Dunav.", "/images/exploration/seed-ribarsko-ostrvo.jpg", new Location(45.2386, 19.8972), "Kod ribarske kućice se dobija mapa staze.", true);
        PublishableRiverside.AddKeyPoint("Most slobode", "Kraj staze, ispod najdužeg mosta u gradu.", "/images/exploration/seed-most-slobode.jpg", new Location(45.2441, 19.8600), null, false);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new RecommendedGroupSize(2, 6));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.AddKeyPoint("Trg Branka Radičevića", "Polazna tačka ture, u centru Sremskih Karlovaca.", "/images/exploration/seed-trg-branka-radicevica.jpg", new Location(45.2025, 19.9339), "Iza fontane je skriven ulaz u stari podrum.", true);
        PublishedVineyards.AddKeyPoint("Porodični podrum", "Degustacija bermeta u prvom podrumu.", "/images/exploration/seed-porodicni-podrum.jpg", new Location(45.1960, 19.9200), "Lozinka za popust je bermet.", false);
        PublishedVineyards.Publish(new TourPrice(false, 50m));

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new RecommendedGroupSize(4, 12));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.AddKeyPoint("Krušedol", "Prvi manastir na turi.", "/images/exploration/seed-krusedol.jpg", new Location(45.1192, 19.9431), null, true);
        PublishedMonasteries.AddKeyPoint("Grgeteg", "Drugi manastir na turi.", "/images/exploration/seed-grgeteg.jpg", new Location(45.1408, 19.8956), null, false);
        PublishedMonasteries.Publish(new TourPrice(false, 150m));

        PublishedBikeRideToPetrovaradin = new(WellKnownUsers.SecondExplorer, "Biciklom do Petrovaradina", "Vožnja kreće sa Keja žrtava racije, prelazi Varadinski most i penje se do Petrovaradinske tvrđave, uz obilazak Sahat kule i povratak kroz Podgrađe.", TourDifficulty.Easy, ["grad", "istorija"], new RecommendedGroupSize(1, 6));
        PublishedBikeRideToPetrovaradin.AddTransportTime(TransportMode.Bicycle, 40);
        PublishedBikeRideToPetrovaradin.AddTransportTime(TransportMode.Walking, 120);
        PublishedBikeRideToPetrovaradin.AddKeyPoint("Kej žrtava racije", "Polazna tačka vožnje.", "/images/exploration/seed-kej.jpg", new Location(45.2510, 19.8550), null, true);
        PublishedBikeRideToPetrovaradin.AddKeyPoint("Sahat kula", "Cilj vožnje, na vrhu tvrđave.", "/images/exploration/seed-sahat-kula.jpg", new Location(45.2532, 19.8614), null, false);
        PublishedBikeRideToPetrovaradin.Publish(new TourPrice(false, 50m));

        PublishedOldTownWalk = new(WellKnownUsers.SecondExplorer, "Peške kroz staro jezgro", "Šetnja vodi od Trga slobode, kroz Zmaj Jovinu i Dunavsku ulicu, pored Vladičanskog dvora i Saborne crkve, do Dunavskog parka i Muzeja Vojvodine.", TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 15));
        PublishedOldTownWalk.AddTransportTime(TransportMode.Walking, 75);
        PublishedOldTownWalk.AddKeyPoint("Trg slobode", "Polazna tačka šetnje.", "/images/exploration/seed-trg-slobode.jpg", new Location(45.2551, 19.8451), null, true);
        PublishedOldTownWalk.AddKeyPoint("Dunavski park", "Kraj šetnje.", "/images/exploration/seed-dunavski-park.jpg", new Location(45.2547, 19.8508), null, false);
        PublishedOldTownWalk.Publish(new TourPrice(true, null));

        ArchivedLakeside = new(WellKnownUsers.Explorer, "Obala Palićkog jezera", "Tura prati obalu Palićkog jezera od Velike terase do Ženskog štranda, uz obilazak Vodotornja i zoološkog vrta na severnoj strani jezera.", TourDifficulty.Easy, ["priroda", "jezero"], new RecommendedGroupSize(2, 8));
        ArchivedLakeside.AddTransportTime(TransportMode.Walking, 75);
        ArchivedLakeside.AddKeyPoint("Velika terasa", "Polazna tačka ture.", "/images/exploration/seed-velika-terasa.jpg", new Location(46.1003, 19.7620), null, true);
        ArchivedLakeside.AddKeyPoint("Ženski štrand", "Kraj ture.", "/images/exploration/seed-zenski-strand.jpg", new Location(46.0990, 19.7700), null, false);
        ArchivedLakeside.Publish(new TourPrice(true, null));
        ArchivedLakeside.Archive();

        PublishedFreeTourWithHiddenFirstKeyPoint = new(WellKnownUsers.SecondExplorer, "Kanal Dunav-Tisa-Dunav", "Šetnja prati nasip kanala Dunav-Tisa-Dunav od ustave na Šajkaškoj strani do starog mosta, uz pogled na barske ptice i ribolovce.", TourDifficulty.Easy, ["priroda"], new RecommendedGroupSize(1, 10));
        PublishedFreeTourWithHiddenFirstKeyPoint.AddTransportTime(TransportMode.Walking, 60);
        PublishedFreeTourWithHiddenFirstKeyPoint.AddKeyPoint("Ustava", "Polazna tačka šetnje, skrivena dok se tura ne pokrene.", "/images/exploration/seed-ustava.jpg", new Location(45.2700, 19.8800), "Ispod ustave je stari natpis graditelja.", false);
        PublishedFreeTourWithHiddenFirstKeyPoint.AddKeyPoint("Stari most", "Kraj šetnje.", "/images/exploration/seed-stari-most.jpg", new Location(45.2750, 19.8900), null, true);
        PublishedFreeTourWithHiddenFirstKeyPoint.Publish(new TourPrice(true, null));
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, PublishedBikeRideToPetrovaradin, PublishedOldTownWalk, ArchivedLakeside, PublishedFreeTourWithHiddenFirstKeyPoint];
}
