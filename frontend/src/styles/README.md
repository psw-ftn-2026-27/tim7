# Deljene klase

Stranicu slažeš od klasa iz ovog foldera. One već odlučuju o razmacima, redovima i bojama, pa
ih ti ne odlučuješ ponovo u svom `.scss`. Klase su definisane u `_layout.scss` (raspored) i
`_components.scss` (elementi koji se ponavljaju), a sve vrednosti dolaze iz `_tokens.scss`.

## Treba mi…

| Treba mi… | Klasa | Kada je ne koristiti |
| --- | --- | --- |
| sadržaj stranice centriran, ograničene širine | `.container` | Unutar stranice modula: okvir aplikacije je već stavio `.container` na `<main>`. |
| razmak između elemenata jedan ispod drugog | `.stack` | Kada deca treba da stoje jedno pored drugog; tada `.actions` ili `.grid`. |
| usku formu ili karticu u sredini stranice | `.narrow` | Za listu ili tabelu, koje koriste punu širinu stranice. |
| kartice poređane u kolone koje se same prelamaju | `.grid` | Za red dugmadi ili za tabelarne podatke; tada `.actions` ili `.table`. |
| red dugmadi ili linkova jedan pored drugog | `.actions` | Za glavnu navigaciju i za elemente koji nisu akcije (oznake, sličice). |
| dugme ili link koji izgleda kao dugme | `.button` | Za običan link u tekstu; on ostaje `<a>` bez klase. |
| glavnu akciju forme ili stranice | `.button--primary` (uz `.button`) | Za više dugmadi u istom redu; glavno je samo jedno. |
| uokviren blok sadržaja (forma, stavka liste) | `.card` | Za ceo sadržaj stranice; on je već u `.container`. |
| polje forme sa labelom | `.field` | Za polje bez labele; svako polje ima labelu. |
| podatke u redovima i kolonama | `.table` | Za raspored stranice; tabela je samo za podatke. |
| poruku o grešci (validacija, odgovor servera) | `.error` | Za obaveštenje koje nije greška; tada `.meta`. |
| sporedan tekst: datum, status, „još nema stavki“ | `.meta` | Za glavni sadržaj i za poruku o grešci. |
| oznaku modula u kom se korisnik nalazi | `.badge` uz `.badge--exploration`, `.badge--games`, `.badge--social` ili `.badge--payment` | Na dugmetu ili linku, i sa varijantom tuđeg modula. |

## Kada tvoja stranica ima nešto samo svoje

Stavi deljenu klasu na element, a ono po čemu se tvoja stranica razlikuje (centriranje,
pomeraj, drugačiji razmak) dopiši u `.scss` te stranice, pod istim ili svojim selektorom.
U lokalnom pravilu piše samo razlika, nikad ponovljena deljena pravila. Ne pravi lokalnu
kopiju deljene klase, ne dodaj novu globalnu varijantu za jednu stranicu i nikad ne koristi
`::ng-deep`. Primer je `.actions` na naslovnoj stranici (`core/home/home.scss`), koja lokalno
dodaje samo centriranje i pomeraj.

## Kontrolna lista

Za stranicu na kojoj radiš proveri, čitajući samo njen kod:

- [ ] Nema lokalne kopije deljenog obrasca: `.scss` stranice ne ponavlja pravila nijedne
      klase iz tabele, a lokalno pravilo za deljenu klasu sadrži samo razliku.
- [ ] Stranica modula nosi oznaku svog modula (`.badge--<modul>`), a ne tuđeg.
- [ ] Boje i razmaci samo iz tokena: u `.scss` nema heks boja, `rgb()` ni `px`/`rem` vrednosti
      za boju ili razmak, već samo `var(--…)` iz `_tokens.scss`.
- [ ] Dugme i link nikad u boji modula: `--color-module-*` se javlja samo kroz `.badge`.
- [ ] Nema `::ng-deep`.
