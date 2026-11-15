# Analitika (`Analytics`)

Servis platformskog tima, `providedIn: 'root'`. Drži odluku korisnika o praćenju i, samo
kada je korisnik pristao, šalje Google Analytics-u pregled svake stranice. Sa našim
serverom ne razgovara: odluka se čita i upisuje u pregledaču, a pregledi idu iz pregledača
pravo Google-u.

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
| `'granted'`   | Korisnik je pristao. Pregledi stranica se šalju.                        |
| `'denied'`    | Korisnik je odbio ili je povukao saglasnost. Ništa se ne šalje.         |

Odluku menjaju metode `grant()` i `deny()`. Zove ih samo platformski kod: ljuska aplikacije,
kada joj kartica `consent-banner` javi izbor korisnika, i stranica `/privacy`. Kartica je
prikazna komponenta, pa joj se servis ne ubrizgava; ljuska je prikazuje dok je odluka
`'undecided'`. Moduli odluku smeju da čitaju, ali je ne menjaju.

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
- Ako je Google Analytics nedostupan ili blokiran, korisnik ne vidi ništa i aplikacija radi
  isto; jedini trag je red u konzoli.

## Povlačenje saglasnosti

`deny()` odmah zaustavlja slanje, postavlja Google-ov zvanični prekidač
`window['ga-disable-<ID>'] = true` i briše kolačiće analitike (imena koja počinju sa `_ga`).
Ako korisnik naknadno pristane, `grant()` vraća prekidač i slanje se nastavlja.

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

Pri otvaranju merenja u Google Analytics-u treba isključiti opciju „Page changes based on
browser history events“ (Enhanced measurement → Page views). Inače Google sam beleži
promenu adrese, pa bi se svaki pregled brojao dvaput.

## Platformsko odstupanje: pretplata na ruter, `document` i `window`

Servis se pretplaćuje na `Router.events` da bi čuo `NavigationEnd`, a to je RxJS, kog u
Angular podskupu nema. Pristupa i objektima `document` i `window`: dodaje Google-ov
`<script>` u dokument, puni `window.dataLayer`, postavlja `window['ga-disable-<ID>']` i
briše kolačiće kroz `document.cookie`. Sve to je zatvoreno u `analytics.ts`; ljuska i
stranica `/privacy` rade samo sa signalom `consent` i metodama `grant()` i `deny()`. To je
odobreno odstupanje od Angular podskupa samo za ovaj servis. **Moduli ovo ne kopiraju**:
kod u modulu se ne pretplaćuje na ruter i ne dira `document` ni `window`.
