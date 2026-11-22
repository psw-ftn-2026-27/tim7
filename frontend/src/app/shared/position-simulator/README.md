# Simulator pozicije (`app-position-simulator`)

Sekcija platformskog tima kojom turista, dok GPS ne postoji, klikom na mapu javlja gde se
nalazi. Ugrađuje deljenu mapu (`app-map`, vidi `shared/map/README.md`) u režimu izbora, čita
sačuvanu poziciju prijavljenog korisnika sa `GET /api/identity/position` i čuva novu kroz
`PUT /api/identity/position`. Može da je koristi svaki prijavljen korisnik. Primer je na
razvojnoj stranici `/map-demo`.

Simulator ne zna za ture ni izazove, ne šalje poziciju modulima i ne meri vreme. Stranica
modula dobija poziciju kroz izlaz i sama je šalje u svom zahtevu svom serveru.

```ts
import { MapPin } from '../../../../shared/map/map'; // broj ../ zavisi od dubine stranice
import { PositionSimulator } from '../../../../shared/position-simulator/position-simulator';

@Component({ imports: [PositionSimulator], ... })
```

## Ponašanje

- Sačuvana pozicija je izabrani pin mape (crven), pa se razlikuje od pinova stranice (plavi).
- Bez sačuvane pozicije pina turiste nema, a sekcija piše „Mark your position by clicking the map.“
- Klik na mapu pomera pin turiste kao nacrt; server se ne menja dok turista ne klikne
  „Confirm position“. Dugme je onemogućeno dok nacrta nema i dok komanda traje.
- Posle uspeha sekcija ponovo učita poziciju i ispod mape piše „Position saved.“ Posle neuspeha
  ispod mape stoji poruka servera, a nacrt ostaje za nov pokušaj.
- Ako pozicija ne može da se učita, sekcija umesto pina piše da poziciju nije moguće učitati.

## Ulazi

| Ulaz   | Tip        | Podrazumevano | Značenje                                                                  |
| ------ | ---------- | ------------- | ------------------------------------------------------------------------- |
| `pins` | `MapPin[]` | `[]`          | Pinovi stranice (ključne tačke, izazovi), prosleđuju se mapi kao ostali pinovi. |

## Izlazi

| Izlaz             | Tip              | Kada se javlja                                                                                                    |
| ----------------- | ---------------- | ----------------------------------------------------------------------------------------------------------------- |
| `positionChanged` | `MapPin \| null` | Posle prvog učitavanja i posle svake uspešne potvrde, sa sačuvanom pozicijom (bez `label`), ili `null` kada je nema. |

Nacrt se ne javlja: stranica dobija samo poziciju koju je server sačuvao.

## Primer ugradnje na stranici na terenu

Isti obrazac važi za pokretanje i izvođenje ture (E22), društveni izazov i kviz (G21) i
lokacijski izazov (G22). Stranica čuva poslednju javljenu poziciju u signalu i šalje je u
komandi svoje grupe. Da akcija ne može bez pozicije, pravilo je modula, ne simulatora.

```html
<app-position-simulator [pins]="challengePins()" (positionChanged)="position.set($event)" />

<button
  class="button button--primary"
  type="button"
  [disabled]="!position() || pending()"
  (click)="activate()"
>
  Activate challenge
</button>
```

```ts
protected readonly position = signal<MapPin | null>(null);

protected async activate(): Promise<void> {
  const position = this.position();
  if (!position) {
    return;
  }
  // ...
  await this.challengeSolving.activate(this.id(), {
    latitude: position.latitude,
    longitude: position.longitude,
  });
  // ...
}
```

Za E22 stranica aktivne ture prosleđuje ključne tačke ture kao `pins` i šalje poziciju u
zahtevu kojim proverava blizinu sledeće ključne tačke; za G21 i G22 stranica izazova
prosleđuje lokacije izazova i šalje poziciju pri aktivaciji ili rešavanju. Koliko često
stranica šalje poziciju, odlučuje modul.

## Platformsko odstupanje: sekcija koja razgovara sa serverom

Ova komponenta nije stranica, a ipak ubrizgava servis (`PositionSimulation`, sa komandom
`setPosition`), čita poziciju kroz `httpResource` i sama čuva poziciju. Pored toga koristi
`effect()` da poziciju javi stranici kroz izlaz posle svakog učitavanja. Prikazna komponenta
iz podskupa ne ubrizgava ništa i ne razgovara sa serverom; ovo je odobreno odstupanje samo za
ovu platformsku sekciju, da bi svaka stranica na terenu dobila isti simulator bez ponavljanja
koda. **Moduli ovo ne kopiraju**: u modulu stranica čita svoj resurs i poziva servis svoje
grupe, a prikazna komponenta samo prima ulaze i javlja izlaze.
