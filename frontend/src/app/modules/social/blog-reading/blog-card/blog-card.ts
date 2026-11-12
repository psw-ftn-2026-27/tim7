import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { marked } from 'marked';
import { BlogDto } from '../../api/social-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-blog-card',
  styleUrl: './blog-card.scss',
  templateUrl: './blog-card.html',
})
export class BlogCard {
  readonly blog = input.required<BlogDto>();

  protected readonly descriptionHtml = computed(() =>
    marked.parse(this.blog().description, { async: false }),
  );
}
