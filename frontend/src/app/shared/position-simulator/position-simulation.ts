import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SetPositionDto } from '../api/set-position-dto';

const BASE_URL = '/api/identity/position';

@Injectable({ providedIn: 'root' })
export class PositionSimulation {
  private readonly http = inject(HttpClient);

  async setPosition(dto: SetPositionDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(BASE_URL, dto));
  }
}
