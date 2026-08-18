export interface Address {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

export interface Customer {
  id: string;
  name: string;
  email: string;
  phone: string;
  address: Address;
  createdAt: string;
}

export interface QuoteBreakdown {
  componentName: string;
  description: string;
  amount: number;
  percentage: number;
  ruleApplied: string;
}

export interface ShippingQuote {
  baseCost: number;
  weightSurcharge: number;
  distanceSurcharge: number;
  commercialValueSurcharge: number;
  deliveryTypeSurcharge: number;
  timeWindowSurcharge: number;
  discount: number;
  total: number;
  actualWeightKg: number;
  volumetricWeightKg: number;
  billableWeightKg: number;
  breakdown: QuoteBreakdown[];
}

export interface ShipmentStatusHistory {
  id: string;
  shipmentId: string;
  previousStatus: string;
  newStatus: string;
  comment: string;
  changedAt: string;
}

export interface Shipment {
  id: string;
  customerId: string;
  origin: Address;
  destination: Address;
  weightKg: number;
  lengthCm: number;
  widthCm: number;
  heightCm: number;
  commercialValue: number;
  distanceKm: number;
  deliveryType: 'Standard' | 'Express' | 'SameDay';
  deliveryWindow: 'Standard' | 'Extended' | 'Night' | 'Weekend';
  status: 'Created' | 'Quoted' | 'Confirmed' | 'InTransit' | 'Delivered' | 'Cancelled';
  baseCost: number;
  totalCost: number;
  quote?: ShippingQuote;
  history: ShipmentStatusHistory[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateShipmentRequest {
  customerId: string;
  origin: Address;
  destination: Address;
  weightKg: number;
  lengthCm: number;
  widthCm: number;
  heightCm: number;
  commercialValue: number;
  distanceKm: number;
  deliveryType: number; // 0=Standard, 1=Express, 2=SameDay
  deliveryWindow: number; // 0=Standard, 1=Extended, 2=Night, 3=Weekend
}
