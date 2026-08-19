import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomerDto, ShipmentDto, ShippingQuote } from '../models/shipping.models';

@Injectable({
  providedIn: 'root'
})
export class ShippingApiService {
  private readonly baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  getCustomers(): Observable<CustomerDto[]> {
    return this.http.get<CustomerDto[]>(`${this.baseUrl}/customers`);
  }

  getShipments(): Observable<ShipmentDto[]> {
    return this.http.get<ShipmentDto[]>(`${this.baseUrl}/shipments`);
  }

  getShipmentById(id: string): Observable<ShipmentDto> {
    return this.http.get<ShipmentDto>(`${this.baseUrl}/shipments/${id}`);
  }

  calculateQuote(payload: any): Observable<ShippingQuote> {
    return this.http.post<ShippingQuote>(`${this.baseUrl}/shipments/quote`, payload);
  }

  createShipment(payload: any): Observable<ShipmentDto> {
    return this.http.post<ShipmentDto>(`${this.baseUrl}/shipments`, payload);
  }

  confirmShipment(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/shipments/${id}/confirm`, {});
  }

  changeStatus(id: string, status: string, notes: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/shipments/${id}/status`, { newStatus: status, notes });
  }
}
