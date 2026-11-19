# Deljena mapa (`app-map`)

Prikazna komponenta platformskog tima zasnovana na Leaflet 1.9. Prikazuje pinove, putanju
između njih i krug oko jedne tačke i omogućava izbor lokacije klikom. Ne ubrizgava joj se nijedan servis, ne razgovara sa
serverom i samo javlja kroz izlaze: čuvanje koordinata i dužine radi stranica modula koji
je koristi. Svi slučajevi su prikazani na razvojnoj stranici `/map-demo`.

```ts
import { Map, MapPin } from '../../../../shared/map/map'; // broj ../ zavisi od dubine stranice

@Component({ imports: [Map], ... })
```

## Tip `MapPin`

```ts
interface MapPin {
  latitude: number;
  longitude: number;
  label?: string; // prikazuje se nad pinom i u oblačiću samo kada je zadat, kao čist tekst
}
```

Stranica pravi pinove od DTO-a svog modula, na primer u `computed()`:

```ts
protected readonly keyPoints = computed<MapPin[]>(() =>
  (this.tour.value()?.keyPoints ?? []).map((point) => ({
    latitude: point.latitude,
    longitude: point.longitude,
    label: point.name,
  })),
);
```

## Ulazi

| Ulaz              | Tip              | Podrazumevano | Značenje                                                                                                                                                        |
| ----------------- | ---------------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `pins`            | `MapPin[]`       | `[]`          | Pinovi koji se prikazuju. Van režima izbora mapa se uokviri tako da se vide svi; bez pinova je centrirana na Novi Sad.                                          |
| `showPath`        | `boolean`        | `false`       | Povezuje pinove linijom po redosledu niza (bez zatvaranja kruga) i uključuje izlaz `pathLength`.                                                                |
| `picking`         | `boolean`        | `false`       | Režim izbora: klik na mapu postavlja ili pomera izabrani pin. Van ovog režima klik ne radi ništa.                                                               |
| `initialLocation` | `MapPin \| null` | `null`        | U režimu izbora izabrani pin se odmah prikazuje ovde i mapa se centrira na njega. Bez nje izabranog pina nema do prvog klika, a mapa je centrirana na Novi Sad. |
| `selectedIcon`    | `'pin' \| 'person'` | `'pin'`    | Izgled izabranog pina: crveni krug ili crvena figura čoveka. Figuru koristi simulator pozicije, da se pozicija korisnika ne meša sa tačkama koje se biraju. |
| `circleCenter`    | `MapPin \| null` | `null`        | Centar kruga van režima izbora. U režimu izbora se ne koristi: centar kruga je izabrani pin, pa krug prati svaki klik.                                          |
| `circleRadiusKm`  | `number`         | `0`           | Poluprečnik kruga u km, kao i dužina putanje. Krug se crta samo kada postoji centar i poluprečnik je veći od 0.                                                 |
| `userPosition`    | `MapPin \| null` | `null`        | Pozicija korisnika, crtana kao figura čoveka. Kada je `showPath` uključen, putanja počinje od nje, ali je `pathLength` i dalje dužina samo između pinova. |

Na mapi je najviše jedan krug i svaka promena centra ili poluprečnika ga ponovo crta. Kada se
krug pojavi ili mu se promeni poluprečnik, mapa se uokviri tako da se ceo krug vidi; pomeranje
centra ne menja prikaz. Klik prolazi kroz krug do mape ili pina ispod njega. Mapa ne filtrira
pinove i ne šalje pretragu: koji su pinovi u krugu određuje server modula, a stranica ih predaje
kroz `pins`.

## Izlazi

| Izlaz            | Tip      | Kada se javlja                                                                                                                                                                                                                                     |
| ---------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `pathLength`     | `number` | Samo kada je `showPath` uključen: pri prvom iscrtavanju i posle svake promene `pins`. Zbir vazdušnih rastojanja uzastopnih pinova u km, nezaokružen; za nijedan ili jedan pin 0. Izabrani pin se ne računa. Zaokruživanje za prikaz radi stranica. |
| `locationPicked` | `MapPin` | Posle svakog klika u režimu izbora, sa koordinatama izabranog pina (bez `label`).                                                                                                                                                                  |

Ostali pinovi u režimu izbora su vizuelno različiti od izabranog (plavi naspram crvenog) i klik ih ne pomera.

## Klik na pin

Klik na pin otvara iznad njega oblačić sa oznakom i koordinatama. Kada je `showPath`
uključen, oblačić piše i redni broj pina u putanji („Point 2 of 5“); bez putanje, kao kod
izazova u gradu, rednog broja nema. Klik na pin ne bira lokaciju, ni u režimu izbora.
Oznaka se vidi i kada se miš zadrži nad pinom.

## Visina

Podrazumevana visina je u `:host` pravilu u `map.scss`. Stranica je menja u svom `.scss`:

```scss
app-map {
  height: 16rem;
}
```

## Primeri ugradnje

### Prikaz

```html
<app-map [pins]="keyPoints()" />
```

### Putanja

```html
<app-map [pins]="keyPoints()" [showPath]="true" (pathLength)="length.set($event)" />
<p>Length: {{ length().toFixed(2) }} km</p>
```

### Izbor lokacije

Stranica u obradi izlaza upisuje koordinate u polja svog modela forme, pa forma ne zna za
mapu, a mapa ne zna za formu.

```html
<app-map
  [pins]="otherKeyPoints()"
  [picking]="true"
  [initialLocation]="savedLocation()"
  (locationPicked)="onLocationPicked($event)"
/>
```

```ts
protected onLocationPicked(location: MapPin): void {
  this.model.update((value) => ({ ...value, latitude: location.latitude, longitude: location.longitude }));
}
```

### Oblast lokacijskog izazova (G22)

Krug oko polazne tačke izazova, bez režima izbora. Poluprečnik računa Games i mapi predaje
gotov broj u km; skrivene tačke se ne prikazuju.

```ts
protected readonly startPins = computed<MapPin[]>(() => [this.start()]);
```

```html
<app-map [pins]="startPins()" [circleCenter]="start()" [circleRadiusKm]="radiusKm()" />
```

### Lokacijska pretraga (E23, G23)

Režim izbora sa krugom: klik postavlja centar, a krug se crta oko izabranog pina. Polje za
poluprečnik i dugme „Search“ pripadaju stranici, pa stranica čuva centar iz `locationPicked`
i poluprečnik iz svoje forme i tek na klik šalje pretragu. Pinovi rezultata stižu sa servera
i predaju se kroz `pins`.

```html
<app-map
  [pins]="results()"
  [picking]="true"
  [circleRadiusKm]="model().radiusKm ?? 0"
  (locationPicked)="center.set($event)"
/>

<div class="field">
  <label for="radius">Radius in km</label>
  <input id="radius" type="number" [formField]="form.radiusKm" />
</div>

<button class="button" type="button" [disabled]="!center() || !form().valid()" (click)="search()">
  Search
</button>
```

```ts
protected readonly center = signal<MapPin | null>(null);
protected readonly model = signal({ radiusKm: null as number | null });
protected readonly form = form(this.model, (path) => {
  required(path.radiusKm, { message: 'Enter a radius.' });
  validate(path.radiusKm, ({ value }) => {
    const radiusKm = value();
    if (radiusKm !== null && radiusKm <= 0) {
      return { kind: 'notPositive', message: 'The radius must be greater than 0.' };
    }
    return null;
  });
});

protected search(): void {
  // Pretragu šalje stranica sa centrom i poluprečnikom; mapa samo crta krug.
}
```

### Putanja od korisnika do sledeće tačke (E22)

Pozicija korisnika dolazi iz simulatora pozicije i predaje se kroz `userPosition`. Mapa je
crta kao istu figuru čoveka kao u simulatoru, a putanja počinje od nje i ide do sledeće
tačke. Blizinu tački proverava modul Exploration.

```ts
protected readonly nextKeyPoint = computed<MapPin[]>(() => [
  { latitude: this.next().latitude, longitude: this.next().longitude, label: this.next().name },
]);
```

```html
<app-map [pins]="nextKeyPoint()" [userPosition]="position()" [showPath]="true" />
```

## Platformsko odstupanje: `inject(ElementRef)`

Komponenta Leafletu predaje sopstveni element kroz `inject(ElementRef)`, pa svaka instanca
crta u svoj tag. Iz istog razloga koristi `effect()`: signali moraju da pokreću Leaflet, koji
je van stabla komponenti, a `ngOnDestroy()` uklanja Leaflet mapu kada komponenta nestane. To je odobreno odstupanje od Angular podskupa samo za ovu
komponentu. **Moduli ovo ne kopiraju**: prikazna komponenta u modulu ne ubrizgava ništa.
