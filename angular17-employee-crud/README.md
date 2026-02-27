# Angular 17 Employee Management CRUD

This app demonstrates an **Employee Management CRUD** in Angular 17 and intentionally covers all major interview topics from your attached list.

## How to understand exactly where each topic is covered

Open `http://localhost:4200/topics` after running the app.

That page contains a **Topic Coverage Map table** with:
- Topic name
- Which page/flow to open
- Which `.ts` files to inspect
- What exactly to look for in those files

So you can directly map each interview topic to implementation files.

## Quick topic-to-file reference

1. Angular framework basics and CLI/npm scripts
   - `package.json`
   - `src/main.ts`
2. Components and routing
   - `src/app/app.component.ts`
   - `src/app/app.routes.ts`
   - `src/app/components/*.component.ts`
3. Data binding
   - `src/app/components/employee-dashboard.component.ts`
   - `src/app/components/employee-form.component.ts`
4. Directives
   - `src/app/components/employee-dashboard.component.ts`
   - `src/app/directives/highlight.directive.ts`
5. Decorators and pipes
   - `src/app/pipes/salary-inr.pipe.ts`
   - `src/app/components/employee-dashboard.component.ts`
6. Services and dependency injection
   - `src/app/services/employee.service.ts`
   - `src/app/components/employee-dashboard.component.ts`
7. Lifecycle hooks
   - `src/app/components/employee-dashboard.component.ts`
   - `src/app/components/employee-form.component.ts`
8. Observable + HttpClient + RxJS operators
   - `src/app/services/employee.service.ts`
   - `src/main.ts`
9. TypeScript fundamentals and strict typing
   - `src/app/models/employee.model.ts`
   - service/component TypeScript files

## Features

- Add employee
- Edit employee
- Delete employee
- List employees
- Custom salary currency pipe (`salaryInr`)
- Custom highlight directive for active rows
- Topic coverage page

## Run

```bash
npm install
npm start
```

Open: `http://localhost:4200`

> Note: In this environment, registry access may be blocked. If so, run locally where npm has access.
