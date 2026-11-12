import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-blog-detail',
  styleUrl: './my-blog-detail.scss',
  templateUrl: './my-blog-detail.html',
})
export class MyBlogDetail {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/mine/${this.id()}`);

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = form(signal({ title: '', description: '', images: '' }), (path) => {
    required(path.title, { message: 'Title is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected startEdit(blog: BlogDto): void {
    this.error.set(null);
    this.form().reset({
      title: blog.title,
      description: blog.description,
      images: blog.images.join('\n'),
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
      await this.blogAuthoring.edit(this.id(), {
        title: this.form.title().value(),
        description: this.form.description().value(),
        images,
      });
      await this.router.navigate(['/social/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the changes.'));
    } finally {
      this.pending.set(false);
    }
  }
}
