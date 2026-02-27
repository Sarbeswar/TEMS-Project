import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface TopicEntry {
  topic: string;
  whereToOpen: string;
  files: string[];
  whatToLookFor: string;
}

@Component({
  selector: 'app-topic-coverage',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <h2>Angular 17 Topic Coverage Map</h2>
      <p>
        Open this page (<code>/topics</code>) whenever you want to quickly identify exactly where each topic is implemented.
      </p>

      <table>
        <thead>
          <tr>
            <th>Topic</th>
            <th>Which page / flow to open</th>
            <th>TS/HTML files to inspect</th>
            <th>What to look for</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let item of topicMap">
            <td><strong>{{ item.topic }}</strong></td>
            <td>{{ item.whereToOpen }}</td>
            <td>
              <ul>
                <li *ngFor="let file of item.files"><code>{{ file }}</code></li>
              </ul>
            </td>
            <td>{{ item.whatToLookFor }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class TopicCoverageComponent {
  readonly topicMap: TopicEntry[] = [
    {
      topic: '1) Angular Framework & CLI basics',
      whereToOpen: 'Project root setup and app startup',
      files: ['angular17-employee-crud/package.json', 'angular17-employee-crud/src/main.ts'],
      whatToLookFor: 'Scripts, dependencies, and standalone bootstrap via bootstrapApplication.'
    },
    {
      topic: '2) Components & Routing (module-era equivalent)',
      whereToOpen: 'Navigate between /employees and /topics',
      files: ['src/app/app.component.ts', 'src/app/app.routes.ts', 'src/app/components/*.component.ts'],
      whatToLookFor: 'Standalone components, selector/template usage, route configuration.'
    },
    {
      topic: '3) Data Binding',
      whereToOpen: '/employees page',
      files: ['src/app/components/employee-dashboard.component.ts', 'src/app/components/employee-form.component.ts'],
      whatToLookFor: 'Interpolation {{ }}, property binding [ ], event binding ( ), and two-way binding [(ngModel)].'
    },
    {
      topic: '4) Directives',
      whereToOpen: '/employees page',
      files: ['src/app/components/employee-dashboard.component.ts', 'src/app/directives/highlight.directive.ts'],
      whatToLookFor: '*ngFor, *ngIf, [ngClass], [ngStyle], and custom attribute directive appHighlight.'
    },
    {
      topic: '5) Decorators & Pipes',
      whereToOpen: '/employees page',
      files: ['src/app/pipes/salary-inr.pipe.ts', 'src/app/components/employee-dashboard.component.ts'],
      whatToLookFor: '@Component/@Injectable/@Directive/@Pipe usage and custom salaryInr pipe.'
    },
    {
      topic: '6) Services & Dependency Injection',
      whereToOpen: '/employees page CRUD actions',
      files: ['src/app/services/employee.service.ts', 'src/app/components/employee-dashboard.component.ts'],
      whatToLookFor: 'Constructor injection, providedIn root, service method calls from component.'
    },
    {
      topic: '7) Lifecycle Hooks',
      whereToOpen: '/employees page load + edit mode',
      files: ['src/app/components/employee-dashboard.component.ts', 'src/app/components/employee-form.component.ts'],
      whatToLookFor: 'ngOnInit data loading and ngOnChanges form patching while editing.'
    },
    {
      topic: '8) Observable / HttpClient / RxJS',
      whereToOpen: 'CRUD create/update/delete in /employees',
      files: ['src/app/services/employee.service.ts', 'src/app/components/employee-dashboard.component.ts', 'src/main.ts'],
      whatToLookFor: 'Observable returns, RxJS operators (of/delay/map), HttpClient provider and injection.'
    },
    {
      topic: '9) TypeScript Basics',
      whereToOpen: 'Model/service/component files',
      files: ['src/app/models/employee.model.ts', 'src/app/services/employee.service.ts'],
      whatToLookFor: 'Interfaces, union types, strong typing, readonly/private modifiers.'
    }
  ];
}
