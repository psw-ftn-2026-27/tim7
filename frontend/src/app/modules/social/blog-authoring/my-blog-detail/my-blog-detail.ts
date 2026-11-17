import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { marked } from 'marked';
import { ImagePicker } from '../../../../shared/image-picker/image-picker';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  imports: [FormField, ImagePicker, RouterLink],
  selector: 'app-my-blog-detail',
  styleUrl: './my-blog-detail.scss',
  templateUrl: './my-blog-detail.html',
})
export class MyBlogDetail {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/mine/${this.id()}`);

  protected readonly descriptionHtml = computed(() =>
    marked.parse(this.detail.value()?.description ?? '', { async: false }),
  );

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = form(signal({ title: '', description: '' }), (path) => {
    required(path.title, { message: 'Title is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  // Addresses of the blog's current images that the author keeps, and newly picked files.
  protected readonly keptImages = signal<string[]>([]);
  protected readonly newImages = signal<File[]>([]);
  protected readonly newImagesValid = signal(true);

  protected startEdit(blog: BlogDto): void {
    this.error.set(null);
    this.form().reset({
      title: blog.title,
      description: blog.description,
    });
    this.keptImages.set(blog.images);
    this.newImages.set([]);
    this.newImagesValid.set(true);
    this.editing.set(true);
  }

  protected removeImage(address: string): void {
    this.keptImages.update((images) => images.filter((image) => image !== address));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.edit(
        this.id(),
        this.form.title().value(),
        this.form.description().value(),
        this.keptImages(),
        this.newImages(),
      );
      await this.router.navigate(['/social/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the changes.'));
    } finally {
      this.pending.set(false);
    }
  }
}
