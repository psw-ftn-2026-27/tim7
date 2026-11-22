import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocationChallengeDto } from '../../api/games-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-location-challenge-list',
  templateUrl: './location-challenge-list.html',
})
export class LocationChallengeList {
  protected readonly challenges = httpResource<LocationChallengeDto[]>(
    () => '/api/games/location-challenges',
  );
}
