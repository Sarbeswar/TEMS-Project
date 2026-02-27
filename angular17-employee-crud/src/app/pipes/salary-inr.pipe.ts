import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'salaryInr',
  standalone: true
})
export class SalaryInrPipe implements PipeTransform {
  transform(value: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0
    }).format(value);
  }
}
