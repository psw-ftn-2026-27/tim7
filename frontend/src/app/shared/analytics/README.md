# Analitika (`Analytics`)

Servis platformskog tima, `providedIn: 'root'`. Drži odluku korisnika o praćenju i, samo
kada je korisnik pristao, šalje Google Analytics-u pregled svake stranice i događaje koje
stranice zabeleže. Sa našim serverom ne razgovara: odluka se čita i upisuje u pregledaču, a
pregledi i događaji idu iz pregledača pravo Google-u.

```ts
import { Analytics } from '../../shared/analytics/analytics'; // broj ../ zavisi od dubine datoteke

protected readonly analytics = inject(Analytics);
```

## Odluka o saglasnosti

Odluka je drugo deljeno stanje pored prijavljenog korisnika. Čita se kroz signal `consent`,
koji ima tri vrednosti:

| Vrednost      | Značenje                                                                |
| ------------- | ----------------------------------------------------------------------- |
| `'undecided'` | Korisnik još nije odlučio. Ništa se ne šalje: ćutanje nije pristanak.   |
| `'granted'`   | Korisnik je pristao. Pregledi stranica i događaji se šalju.             |
| `'denied'`    | Korisnik je odbio ili je povukao saglasnost. Ništa se ne šalje.         |

Odluku menjaju metode `grant()` i `deny()`. Zove ih samo platformski kod: ljuska aplikacije,
kada joj kartica `consent-banner` javi izbor korisnika, i stranica `/privacy`. Kartica je
prikazna komponenta, pa joj se servis ne ubrizgava; ljuska je prikazuje dok je odluka
`'undecided'`. Moduli odluku smeju da čitaju, ali je ne menjaju. Pre beleženja događaja
modul odluku ne proverava: to radi servis.

Odluka se pamti u `localStorage` pod ključem `explorer.analytics-consent`, po pregledaču i
bez roka trajanja. Kada skladište pregledača nije dostupno, odluka važi samo dok je
aplikacija otvorena; pri sledećem dolasku odluke nema, ništa se ne šalje i kartica se
ponovo prikaže.

## Pregledi stranica

Dok je odluka `'granted'`, servis šalje tačno jedan događaj `page_view` posle svake završene
navigacije, i kada se promeni samo parametar adrese (`/social/1` → `/social/2`). Zato sluša
događaj `NavigationEnd` rutera, a ne učitavanje aplikacije.

- Šalje se putanja bez dela iza `?` i `#` (`page_path` i `page_location`), jer taj deo može
  da sadrži lične podatke.
- Kada korisnik pristane, beleži se i stranica na kojoj se tada nalazi, da se prva stranica
  novog posetioca ne izgubi.
- Google-ova skripta `gtag.js` učitava se bez npm paketa i tek kada korisnik pristane. Pre
  toga se Google-u ne šalje nijedan zahtev.
- Uz svaki pregled ide i uloga korisnika (`user_role`), kao uz svaki drugi događaj; vidi
  „Šta servis dodaje sam“.
- Ako je Google Analytics nedostupan ili blokiran, korisnik ne vidi ništa i aplikacija radi
  isto; trag su upozorenje u konzoli da skripta nije učitana i neuspeo zahtev u kartici
  Network.

## Povlačenje saglasnosti

`deny()` odmah zaustavlja slanje, postavlja Google-ov zvanični prekidač
`window['ga-disable-<ID>'] = true` i briše kolačiće analitike (imena koja počinju sa `_ga`).
Ako korisnik naknadno pristane, `grant()` vraća prekidač i slanje se nastavlja.

## Beleženje događaja iz modula

Uputstvo za timove modula. Modul beleži događaj samo kroz ovaj servis, metodom `record()`,
i nikada ne poziva Google Analytics direktno (`gtag`, `window.dataLayer`). Nov događaj se
dodaje u četiri koraka.

### 1. Izbor imena

Ime je na engleskom, malim slovima sa donjim crtama, u obliku stvar pa radnja u prošlom
vremenu: `tour_published`, `challenge_solved`. Ime kaže šta se već desilo, a ne šta je
korisnik kliknuo, pa nije ni `publish_tour` ni `publish_clicked`.

### 2. Popis događaja i parametara

Svi događaji i njihovi parametri popisani su na jednom mestu, u
`shared/analytics/analytics-events.ts`. Uz svako ime stoji oblik njegovih parametara:

```ts
export interface AnalyticsEvents {
  tour_created: { tour_id: string };
  tour_published: { tour_id: string };
  user_registered: Record<string, never>;
}
```

Nov događaj je nov red u tom interfejsu. Datoteka je platformski kod, pa nov događaj ulazi
kroz pull request koji pregleda platformski tim.

Događaj bez parametara, kao `user_registered`, ima oblik `Record<string, never>`. Poziv je
`record('user_registered', {})`, a svaki prosleđen parametar je greška pri prevođenju.

Prevodilac odbija ime koje nije popisano i parametar koji nije popisan za to ime: greška
se vidi u editoru i pri `npm run build`, pa takav kod ne stiže do korisnika.

### 3. Šta sme da bude parametar

Uz događaj ne sme da putuje ništa što otkriva ko je korisnik.

| Sme                                                        | Ne sme                                    |
| ---------------------------------------------------------- | ----------------------------------------- |
| Identifikator stvari na koju se događaj odnosi (`tour_id`) | Ime korisnika, mejl, identifikator naloga |
| Broj                                                       | Tekst koji je korisnik sam uneo           |
| Vrednost sa unapred zatvorenog spiska                      |                                           |

Imena parametara pišu se kao imena događaja: engleski, mala slova, donje crte.

### 4. Beleženje: tek kada radnja uspe

Događaj se beleži kada server potvrdi radnju, a ne pri kliku. Beleži ga stranica, u obradi
komande, odmah posle `await` poziva servisa grupe. Primer je objavljivanje ture:

```ts
import { Analytics } from '../../../../shared/analytics/analytics'; // broj ../ zavisi od dubine stranice

export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly analytics = inject(Analytics);

  protected async publish(tourId: string): Promise<void> {
    this.error.set(null);
    try {
      await this.tourAuthoring.publish(tourId, { isFree: true, amount: null });
      // Server je potvrdio objavljivanje: tek sada se događaj beleži.
      this.analytics.record('tour_published', { tour_id: tourId });
    } catch (failure) {
      // Server je odbio: ništa se ne beleži.
      this.error.set(serverMessage(failure, 'Could not publish the tour.'));
    } finally {
      this.tours.reload();
    }
  }
}
```

`record()` ne vraća ništa i nikada ne baca grešku, pa se ne čeka (`await`) i ne obavija
dodatnim `try`. Funkcionalnost radi isto bez obzira na to da li je događaj poslat: korisnik
ne vidi ništa ni kada beleženje uspe ni kada ne uspe.

### Šta servis dodaje sam

Modul prosleđuje samo ime i parametre popisane za to ime. Ostalo radi servis, pa modul ne
može ni da ga izostavi ni da ga pogrešno pošalje:

- **Proveru saglasnosti.** Dok odluka nije `'granted'`, ili dok je ID merenja prazan,
  događaj se ne šalje. Modul ne čita `consent` pre poziva.
- **Ulogu korisnika.** Uz svaki događaj ide tačno jedna vrednost parametra `user_role`.
  Čita se iz `Auth` (`core/auth`), bez upita serveru, i modul je ne prosleđuje.

| Korisnik                                     | `user_role`       |
| -------------------------------------------- | ----------------- |
| Niko nije prijavljen                         | `'visitor'`       |
| Prijavljen, među ulogama ima `administrator` | `'administrator'` |
| Prijavljen, bez uloge `administrator`        | `'explorer'`      |

`visitor` nije uloga u sistemu: servis je sam upisuje kada niko nije prijavljen. Korisniku
sa više uloga beleži se ona sa najvećim ovlašćenjima.

### Provera novog događaja

Uz pokrenut `npm start`, pristati na analitiku i obaviti radnju. Događaj se proverava na
tri mesta:

1. **Konzola pregledača.** U razvoju servis ispisuje jedan red po događaju. Kada je poslat:
   `Analytics event sent: tour_published` i parametri (`tour_id`, `user_role`,
   `debug_mode`). Kada je preskočen, sa razlogom:
   `Analytics event skipped (no consent): tour_published` ili
   `Analytics event skipped (no measurement ID): tour_published`. Red „sent“ znači da je
   događaj predat Google-ovoj skripti, a ne da je stigao do Google-a. Van razvoja
   (`npm run build`) servis za događaje ne ispisuje ništa.
2. **Kartica Network.** Filtrirati po `collect`. Zahtev ka Google-u nosi ime događaja
   (`en=tour_published`) i parametre (`ep.tour_id`, `ep.user_role`), u adresi ili u telu
   zahteva. Ako je Google Analytics nedostupan ili blokiran, ovde se vidi neuspeo zahtev.
3. **DebugView u Google Analytics-u** (Admin → DebugView). U razvoju servis uz svaki
   događaj šalje `debug_mode: true`, pa se događaj tu pojavi posle nekoliko sekundi, a
   klikom na njega vide se parametri.

Dva pravila se proveravaju ručno:

- **Bez saglasnosti.** Na stranici `/privacy` povući saglasnost i ponoviti radnju. Radnja
  uspeva kao i inače, u konzoli stoji red „skipped (no consent)“, a u kartici Network nema
  zahteva `collect`.
- **Tek kada radnja uspe.** Izazvati da server odbije radnju. Korisnik vidi poruku o
  grešci kao i inače, a u konzoli nema reda o tom događaju.

## ID merenja

ID merenja stoji na jednom mestu, u `frontend/src/environments/environment.ts`:

```ts
export const environment = {
  analyticsMeasurementId: '', // na primer 'G-XXXXXXXXXX'
};
```

Dok je ID prazan, servis ne učitava Google-ovu skriptu i ne šalje ništa, a odluka, kartica
i stranica `/privacy` rade isto. Projekat nema produkciju, pa nema ni druge datoteke
okruženja ni pravila zamene u `angular.json`.

ID nije tajna: vidi ga svako ko otvori stranicu, pa sme da stoji u repozitorijumu.

## Podešavanje u Google Analytics-u

Servis sam šalje svaki `page_view`, pa Google ne sme da šalje nijedan svoj. To se ne
postiže kodom, nego podešavanjem merenja, i radi se jednom, kada se merenje otvori. Bez
ovih koraka pregledi se broje dvaput.

U strimu (Admin → Data streams → strim → Enhanced measurement, zupčanik):

1. Isključiti sve stavke osim „Page views“.
2. U „Page views“ → „Show advanced settings“ isključiti „Page changes based on browser
   history events“. Inače Google sam beleži svaku promenu adrese.

Na Google oznaci (isti strim → „Configure tag settings“, što otvara Tag Manager):

3. Settings → „Manage automatic event detection“: isključiti sve stavke.
4. Settings → „Allow user-provided data capabilities“: isključiti. Inače oznaka na stranici
   traži mejl, telefon i adresu, a lični podaci ne smeju da idu Google-u.
5. Tags → Google oznaka sa ID-jem merenja → „Configuration settings“: dodati parametar
   `send_page_view` sa vrednošću `false`, sačuvati i objaviti („Submit“).

Korak 5 je neophodan iako servis isti parametar zadaje i iz koda: Google oznaka ima
ugrađenu konfiguraciju i onu sa stranice odbacuje kao duplu, pa bi pri svakom učitavanju
aplikacije poslala i svoj `page_view`.

Provera: u kartici Network pregledača filtrirati po `collect`. Pre pristanka nema nijednog
zahteva ka Google-u, a posle pristanka ide tačno jedan `page_view` po stranici, sa
parametrom `dp` jednakim putanji. Događaji koje stranice zabeleže idu ka istoj adresi; vidi
„Provera novog događaja“.

## Ključni događaji i izveštaj toka

Uputstvo za platformski tim. Ovaj deo funkcionalnosti nije kod: uloga se prijavljuje i
izveštaj se pravi ručno, u Google Analytics-u, i u repozitorijumu od toga ostaje samo ovaj
opis. Po njemu izveštaj može ponovo da se napravi.

**Napomena:** nazivi polja u dijalogu za dimenziju i u uređivaču koraka prepisani su sa
živog interfejsa (novembar 2026). Putanje kroz menije i nazivi „Breakdown“ i „Share“ nisu
provereni, pa mogu malo da se razlikuju od onoga što ovde piše.

### Događaji koji se danas beleže

| Događaj           | Gde se beleži                                             | Parametri                    |
| ----------------- | --------------------------------------------------------- | ---------------------------- |
| `page_view`       | Servis `Analytics`, posle svake završene navigacije       | `page_path`, `page_location` |
| `tour_published`  | Stranica `MyTours`, kada server potvrdi objavljivanje     | `tour_id`                    |
| `tour_created`    | Stranica `CreateTour`, kada server potvrdi pravljenje     | `tour_id`                    |
| `user_registered` | Stranica `Register`, kada server potvrdi registraciju     | nema                         |

Uz svaki od njih servis dodaje i `user_role`. Uz `user_registered` to je `explorer`, jer je
korisnik u tom trenutku već prijavljen.

### Uloga kao prilagođena dimenzija

Servis uz svaki događaj šalje parametar `user_role`, ali Google Analytics po njemu deli
izveštaje tek kada se parametar prijavi kao prilagođena dimenzija. Prijava se radi jednom,
ručno, pre nego što podaci počnu da stižu:

1. Admin → Data display → Custom definitions → otvoriti dijalog „New custom definition“.
2. „Definition type“: Dimension.
3. „Scope“: Event.
4. „Event parameter name“: `user_role`.
5. „Dimension name“: ime pod kojim će se videti u izveštajima, na primer `User role`, pa
   „Save“.

Prijava ne važi unazad: podaci koji su stigli pre nje ostaju bez uloge, pa se u izveštaju ne
mogu podeliti po ulozi.

### Izveštaj toka

Izveštaj se pravi ručno, u delu Explore, kao „Funnel exploration“, sa tri koraka:

| Korak | Ime koraka     | Uslovi                                                                               |
| ----- | -------------- | ------------------------------------------------------------------------------------ |
| 1     | „Naslovna“     | `page_view`, pa „And“: „Page path and screen class“ „exactly matches (=)“ `/`            |
| 2     | „Registracija“ | `user_registered`                                                                    |
| 3     | „Spisak tura“  | `page_view`, pa „And“: „Page path and screen class“ „exactly matches (=)“ `/exploration` |

Koraci se zadaju u dijalogu „Edit funnel steps“. Putanja je drugi uslov u koraku, dodat
dugmetom „And“ i izabran među ugrađenim dimenzijama. Dugme „Add parameter“ uz događaj nudi
samo prilagođene parametre, pa `page_location` tamo ne postoji; ne treba ga prijavljivati
kao novu definiciju. Između koraka ostaje „is indirectly followed by“.

Putanja se zadaje kao tačno poklapanje, a ne kao „contains“: `/` je deo svake putanje, a
`/exploration` je i početak adresa `/exploration/mine` i `/exploration/create`.

Za svaki korak izveštaj pokazuje koliko je korisnika do njega stiglo i koliko je odustalo
pre sledećeg. Da bi se isti brojevi videli po ulogama (`visitor`, `explorer`,
`administrator`), u „Breakdown“ se stavlja prijavljena dimenzija uloge.

Izveštaj se sačuva i podeli („Share“), da ostatak tima može da ga otvori.

### Kada izveštaj treba ispraviti

Uslovi koraka su u Google Analytics-u upisani kao običan tekst i sa kodom ih ništa ne
povezuje. Izveštaj treba ispraviti kada se promeni:

- ime događaja iz toka (`user_registered`),
- adresa `/` ili adresa `/exploration`.

Bez ispravke kod se i dalje prevodi i aplikacija radi isto, a izveštaj na tom koraku ćutke
pokazuje nulu.

### Provera posle podešavanja

Platformski tim jednom, posle prijave dimenzije i pravljenja izveštaja, prođe ceo tok:

1. Kao neprijavljen posetilac otvoriti naslovnu stranicu `/` i pristati na analitiku.
2. Registrovati se na `/register`.
3. Otvoriti spisak tura na `/exploration`.

Zatim u izveštaju proveriti da se vide sva tri koraka i podela po ulozi.

Izveštaji u delu Explore pokazuju podatke tek kada ih Google obradi, što može da potraje
dan ili duže. Zato se izveštaj ne proverava odmah posle prolaska kroz tok. DebugView isti
događaj pokazuje posle nekoliko sekundi, pa se njime odmah proverava da su događaji poslati;
vidi „Provera novog događaja“.

## Platformsko odstupanje: pretplata na ruter, `document`, `window` i `isDevMode()`

Servis se pretplaćuje na `Router.events` da bi čuo `NavigationEnd`, a to je RxJS, kog u
Angular podskupu nema. Pristupa i objektima `document` i `window`: dodaje Google-ov
`<script>` u dokument, puni `window.dataLayer`, postavlja `window['ga-disable-<ID>']` i
briše kolačiće kroz `document.cookie`. Poziva i `isDevMode()` iz `@angular/core`, kog u
Angular podskupu takođe nema, da bi samo u razvoju ispisao red u konzolu za svaki događaj i
dodao mu `debug_mode`. Sve to je zatvoreno u `analytics.ts`; ljuska i stranica `/privacy`
rade samo sa signalom `consent` i metodama `grant()` i `deny()`, a moduli samo sa metodom
`record()`. To je odobreno odstupanje od Angular podskupa samo za ovaj servis. **Moduli ovo
ne kopiraju**: kod u modulu se ne pretplaćuje na ruter, ne dira `document` ni `window` i ne
poziva `isDevMode()`.
