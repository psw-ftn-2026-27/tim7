import { DestroyRef, inject } from '@angular/core';

// Calls the callback every `milliseconds` while the component that called it exists, and
// stops when the component is destroyed, so leaving the page stops the repetition.
// Call it in the constructor of a page, like effect(). Platform helper: modules use this
// and never setInterval or RxJS. See README.md next to this file.
export function repeatEvery(milliseconds: number, callback: () => void): void {
  const id = setInterval(callback, milliseconds);
  inject(DestroyRef).onDestroy(() => clearInterval(id));
}
