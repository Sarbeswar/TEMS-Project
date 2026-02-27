import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="nav">
      <a routerLink="/employees" routerLinkActive="active">Employee CRUD</a>
      <a routerLink="/topics" routerLinkActive="active">Angular 17 Topic Coverage</a>
    </nav>
    <div class="container">
      <router-outlet />
    </div>
  `
})
export class AppComponent {}
