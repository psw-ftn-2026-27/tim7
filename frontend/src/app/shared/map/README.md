# Deljena mapa (`app-map`)

Prikazna komponenta platformskog tima zasnovana na Leaflet 1.9. Prikazuje pinove, putanju
između njih i omogućava izbor lokacije klikom. Ne ubrizgava joj se nijedan servis, ne razgovara sa
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
  label?: string; // prikazuje se iznad pina samo kada je zadat
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

## Izlazi

| Izlaz            | Tip      | Kada se javlja                                                                                                                                                                                                                                     |
| ---------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `pathLength`     | `number` | Samo kada je `showPath` uključen: pri prvom iscrtavanju i posle svake promene `pins`. Zbir vazdušnih rastojanja uzastopnih pinova u km, nezaokružen; za nijedan ili jedan pin 0. Izabrani pin se ne računa. Zaokruživanje za prikaz radi stranica. |
| `locationPicked` | `MapPin` | Posle svakog klika u režimu izbora, sa koordinatama izabranog pina (bez `label`).                                                                                                                                                                  |

Ostali pinovi u režimu izbora su vizuelno različiti od izabranog i klik ih ne pomera.

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

## Platformsko odstupanje: `inject(ElementRef)`

Komponenta Leafletu predaje sopstveni element kroz `inject(ElementRef)`, pa svaka instanca
crta u svoj tag. Iz istog razloga koristi `effect()`: signali moraju da pokreću Leaflet, koji
je van stabla komponenti, a `ngOnDestroy()` uklanja Leaflet mapu kada komponenta nestane. To je odobreno odstupanje od Angular podskupa samo za ovu
komponentu. **Moduli ovo ne kopiraju**: prikazna komponenta u modulu ne ubrizgava ništa.
