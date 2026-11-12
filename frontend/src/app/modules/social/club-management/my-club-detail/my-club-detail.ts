import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubManagement } from '../club-management';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-club-detail',
  styleUrl: './my-club-detail.scss',
  templateUrl: './my-club-detail.html',
})
export class MyClubDetail {
  private readonly clubManagement = inject(ClubManagement);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<ClubDto>(() => `/api/social/clubs/mine/${this.id()}`);

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = form(signal({ name: '', description: '', images: '' }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected startEdit(club: ClubDto): void {
    this.error.set(null);
    this.form().reset({
      name: club.name,
      description: club.description,
      images: club.images.join('\n'),
    });
    this.editing.set(true);
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      const images = this.form
        .images()
        .value()
        .split('\n')
        .map((address) => address.trim())
        .filter((address) => address !== '');
      await this.clubManagement.update(this.id(), {
        name: this.form.name().value(),
        description: this.form.description().value(),
        images,
      });
      await this.router.navigate(['/social/clubs/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the changes.'));
    } finally {
      this.pending.set(false);
    }
  }
}
