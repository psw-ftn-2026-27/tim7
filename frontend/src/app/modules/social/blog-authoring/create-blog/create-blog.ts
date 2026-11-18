import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { ImagePicker } from '../../../../shared/image-picker/image-picker';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  imports: [FormField, ImagePicker],
  selector: 'app-create-blog',
  styleUrl: './create-blog.scss',
  templateUrl: './create-blog.html',
})
export class CreateBlog {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(signal({ title: '', description: '' }), (path) => {
    required(path.title, { message: 'Title is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected readonly images = signal<File[]>([]);
  protected readonly imagesValid = signal(true);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.create(
        {
          title: this.form.title().value(),
          description: this.form.description().value(),
        },
        this.images(),
      );
      await this.router.navigate(['/social/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
