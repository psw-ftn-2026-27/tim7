import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { FoundUserDto } from '../../api/identity-api-types';
import { Following } from '../following';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-user-search',
  styleUrl: './user-search.scss',
  templateUrl: './user-search.html',
})
export class UserSearch {
  private readonly following = inject(Following);

  protected readonly form = form(signal({ email: '' }));

  // Stays null until the first search, so the page loads no users before that.
  private readonly searchedEmail = signal<string | null>(null);

  protected readonly users = httpResource<PageResult<FoundUserDto>>(() => {
    const email = this.searchedEmail();
    if (email === null) {
      return undefined;
    }
    return `/api/identity/users?email=${encodeURIComponent(email)}&page=1&pageSize=20`;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected search(event: Event): void {
    event.preventDefault();
    this.error.set(null);
    this.searchedEmail.set(this.form.email().value());
  }

  protected async sendRequest(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.sendRequest(userId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Zahtev nije poslat.'));
    } finally {
      this.users.reload();
      this.pending.set(false);
    }
  }
}
