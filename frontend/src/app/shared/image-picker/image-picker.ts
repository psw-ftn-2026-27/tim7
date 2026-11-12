import { Component, computed, output, signal } from '@angular/core';

// A file the user picked: a valid one has a preview, an invalid one has the reason instead.
export interface PickedImage {
  id: number;
  file: File;
  preview: string | null;
  error: string | null;
}

// The same limits the server checks; they are equal for every place that uses the picker.
const MAX_IMAGES = 10;
const MAX_BYTES = 5 * 1024 * 1024;
const MAX_DIMENSION = 6000;
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

@Component({
  selector: 'app-image-picker',
  templateUrl: './image-picker.html',
  styleUrl: './image-picker.scss',
})
export class ImagePicker {
  readonly filesChange = output<File[]>();
  readonly validChange = output<boolean>();

  protected readonly maxImages = MAX_IMAGES;
  protected readonly entries = signal<PickedImage[]>([]);
  protected readonly droppedCount = signal(0);

  protected readonly validCount = computed(
    () => this.entries().filter((entry) => entry.error === null).length,
  );
  protected readonly full = computed(() => this.validCount() >= MAX_IMAGES);

  private nextId = 1;

  protected async onFilesPicked(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    // Clearing the input lets the user pick the same file again after removing it.
    input.value = '';
    this.droppedCount.set(0);

    const checked = await Promise.all(files.map((file) => this.check(file)));

    let validCount = this.validCount();
    let dropped = 0;
    const added: PickedImage[] = [];
    for (const entry of checked) {
      if (entry.error !== null) {
        added.push(entry);
      } else if (validCount < MAX_IMAGES) {
        added.push(entry);
        validCount++;
      } else {
        dropped++;
      }
    }

    this.entries.update((entries) => [...entries, ...added]);
    this.droppedCount.set(dropped);
    this.notify();
  }

  protected remove(entry: PickedImage): void {
    this.entries.update((entries) => entries.filter((other) => other.id !== entry.id));
    this.droppedCount.set(0);
    this.notify();
  }

  private notify(): void {
    const entries = this.entries();
    this.filesChange.emit(entries.filter((entry) => entry.error === null).map((entry) => entry.file));
    this.validChange.emit(entries.every((entry) => entry.error === null));
  }

  private async check(file: File): Promise<PickedImage> {
    const entry: PickedImage = { id: this.nextId++, file, preview: null, error: null };
    if (!ALLOWED_TYPES.includes(file.type)) {
      return { ...entry, error: 'only JPEG, PNG and WebP are allowed' };
    }
    if (file.size > MAX_BYTES) {
      return { ...entry, error: 'larger than 5 MB' };
    }
    const preview = await readAsDataUrl(file);
    const size = preview === null ? null : await readSize(preview);
    if (preview === null || size === null) {
      return { ...entry, error: 'could not be read as an image' };
    }
    if (size.width > MAX_DIMENSION || size.height > MAX_DIMENSION) {
      return { ...entry, error: 'wider or taller than 6000 px' };
    }
    return { ...entry, preview };
  }
}

// Reads the file as a data: URL, which the preview shows; null when it cannot be read.
function readAsDataUrl(file: File): Promise<string | null> {
  return new Promise((resolve) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = () => resolve(null);
    reader.readAsDataURL(file);
  });
}

// Loads the data: URL as an image to learn its size; null when the browser cannot decode it.
function readSize(url: string): Promise<{ width: number; height: number } | null> {
  return new Promise((resolve) => {
    const image = new Image();
    image.onload = () => resolve({ width: image.naturalWidth, height: image.naturalHeight });
    image.onerror = () => resolve(null);
    image.src = url;
  });
}
