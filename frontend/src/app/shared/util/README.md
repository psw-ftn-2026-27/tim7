# Pomoćne funkcije (`shared/util`)

## `serverMessage`

Iz greške komande izvlači poruku servera (`title` iz ProblemDetails), a kada je nema, vraća
zadatu podrazumevanu poruku.

## `repeatEvery`

Poziva zadatu funkciju na svakih zadatih milisekundi dok postoji komponenta koja ju je
pozvala, a kada korisnik napusti stranicu, ponavljanje se samo zaustavlja. Koristi se za
proveru koja mora da se ponavlja i kada korisnik ništa ne radi, na primer:

- E22: na ekranu aktivne ture provera da li je turista blizu ključne tačke, uz beleženje poslednje aktivnosti;
- G21: kod društvenog izazova slanje lokacije i dobijanje broja aktivnih korisnika u opsegu.

Moduli ponavljanje rade isključivo kroz ovu funkciju, nikad kroz `setInterval` ni RxJS
(`interval`, `timer`). Poziva se u konstruktoru stranice, kao `effect()`, jer mora da se
pozove dok Angular pravi komponentu.

Primer stranice aktivne ture koja poziciju dobija od simulatora pozicije
(`shared/position-simulator`) i šalje je svom serveru na svakih 10 sekundi:

```ts
export class ActiveTour {
  private readonly tourExecution = inject(TourExecution);

  protected readonly position = signal<MapPin | null>(null);
  protected readonly error = signal<string | null>(null);

  constructor() {
    repeatEvery(10_000, () => this.check());
  }

  protected onPositionChanged(position: MapPin | null): void {
    this.position.set(position);
  }

  private async check(): Promise<void> {
    const position = this.position();
    if (!position) {
      return;
    }
    try {
      await this.tourExecution.checkProximity(this.id(), position.latitude, position.longitude);
      this.execution.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not check your position.'));
    }
  }
}
```

```html
<app-position-simulator [pins]="keyPoints()" (positionChanged)="onPositionChanged($event)" />
```

U primeru treba uočiti sledeće:

- Simulator javlja poziciju samo kada se promeni, a stranica je pamti u signalu. Provera
  se ponavlja i kada turista stoji, pa server beleži poslednju aktivnost i kod G21 vidi
  korisnike koji su u međuvremenu došli.
- Dok pozicije nema, provera se preskače, a stranica prikazuje poziv simulatora da se
  pozicija označi.
- Funkcija ne čeka da prethodna provera završi; razmak od 10 sekundi je dovoljno veliki da
  se provere ne preklapaju.
- `TourExecution`, `checkProximity` i `execution` pripadaju modulu Exploration i ovde su
  samo ilustracija.

### Platformsko odstupanje

Funkcija iznutra koristi `setInterval` i `DestroyRef`, koji nisu u Angular podskupu.
Odstupanje je odobreno samo ovde, da bi moduli dobili ponavljanje bez učenja životnog
ciklusa komponente. **Moduli ovo ne kopiraju**: ako im treba nešto drugo što se ponavlja,
traže to od platformskog tima.
