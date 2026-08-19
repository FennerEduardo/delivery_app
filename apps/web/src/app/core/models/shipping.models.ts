export interface Money {
  amount: number;
  currency: string;
}

export interface QuoteComponentBreakdown {
  componentName: string;
  description: string;
  amount: number;
  percentage: number;
  ruleApplied: string;
}

export interface ShippingQuote {
  pricingVersion: string;
  quotedAt: string;
  baseCost: Money;
  weightSurcharge: Money;
  distanceSurcharge: Money;
  commercialValueSurcharge: Money;
  deliveryTypeSurcharge: Money;
  timeWindowSurcharge: Money;
  discount: Money;
  total: Money;
  actualWeightKg: number;
  volumetricWeightKg: number;
  billableWeightKg: number;
  appliedRuleIds: string[];
  breakdownComponents: QuoteComponentBreakdown[];
}

export interface CustomerDto {
  id: string;
  fullName: string;
  companyName?: string;
  email: string;
  phoneNumber: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  isVIP: boolean;
  createdAt: string;
}

export interface ShipmentStatusHistoryDto {
  id: string;
  status: string;
  notes: string;
  changedBy: string;
  timestamp: string;
}

export interface ShipmentDto {
  id: string;
  trackingNumber: string;
  customerId: string;
  customerName: string;
  originCity: string;
  destinationCity: string;
  weightKg: number;
  lengthCm: number;
  widthCm: number;
  heightCm: number;
  commercialValue: number;
  distanceKm: number;
  deliveryType: string;
  deliveryWindow: string;
  status: string;
  quotedPrice: number;
  pricingVersion: string;
  createdAt: string;
  statusHistory: ShipmentStatusHistoryDto[];
}
