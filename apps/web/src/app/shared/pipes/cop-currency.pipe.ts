import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'copCurrency',
  standalone: true
})
export class CopCurrencyPipe implements PipeTransform {
  transform(value: number | undefined | null): string {
    if (value === null || value === undefined) return '$ 0 COP';
    return `$ ${new Intl.NumberFormat('es-CO', { maximumFractionDigits: 0 }).format(value)} COP`;
  }
}
