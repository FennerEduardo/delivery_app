import { Injectable, signal } from '@angular/core';
import { Customer, Shipment, ShippingQuote, CreateShipmentRequest } from '../../../../libs/frontend/models/shipping.models';

@Injectable({
  providedIn: 'root'
})
export class ShippingApiService {
  private baseUrl = 'http://localhost:5000/api';

  // Reactive State Signals
  customers = signal<Customer[]>([
    {
      id: 'cust-101',
      name: 'Empresa Logística Alfa S.A.S.',
      email: 'logistica@alfa.com.co',
      phone: '+57 300 987 6543',
      address: { street: 'Calle 26 # 68-90', city: 'Bogotá', state: 'Cundinamarca', zipCode: '110931', country: 'Colombia' },
      createdAt: new Date().toISOString()
    },
    {
      id: 'cust-102',
      name: 'Comercializadora Andina',
      email: 'compras@andina.co',
      phone: '+57 312 456 7890',
      address: { street: 'Carrera 43A # 1-50', city: 'Medellín', state: 'Antioquia', zipCode: '050021', country: 'Colombia' },
      createdAt: new Date().toISOString()
    }
  ]);

  shipments = signal<Shipment[]>([
    {
      id: 'shp-8801',
      customerId: 'cust-101',
      origin: { street: 'Calle 26 # 68-90', city: 'Bogotá', state: 'Cundinamarca', zipCode: '110931', country: 'Colombia' },
      destination: { street: 'Carrera 43A # 1-50', city: 'Medellín', state: 'Antioquia', zipCode: '050021', country: 'Colombia' },
      weightKg: 4.5,
      lengthCm: 30,
      widthCm: 25,
      heightCm: 20,
      commercialValue: 1200000,
      distanceKm: 420,
      deliveryType: 'Express',
      deliveryWindow: 'Standard',
      status: 'Confirmed',
      baseCost: 15000,
      totalCost: 31850,
      quote: {
        baseCost: 15000,
        weightSurcharge: 0,
        distanceSurcharge: 7500,
        commercialValueSurcharge: 150,
        deliveryTypeSurcharge: 6795,
        timeWindowSurcharge: 0,
        discount: 0,
        total: 29445,
        actualWeightKg: 4.5,
        volumetricWeightKg: 3.0,
        billableWeightKg: 4.5,
        breakdown: [
          { componentName: 'BaseCost', description: 'Tarifa base peso facturable 4.5 kg', amount: 15000, percentage: 0, ruleApplied: 'Tier >2-5 kg -> 15,000 COP' },
          { componentName: 'DistanceSurcharge', description: 'Recargo por distancia 420 km', amount: 7500, percentage: 50, ruleApplied: 'Distance >150 km -> +50%' },
          { componentName: 'CommercialValueSurcharge', description: 'Recargo valor declarado 1.2M', amount: 225, percentage: 1, ruleApplied: 'Commercial value 500k-2M -> +1%' },
          { componentName: 'DeliveryTypeSurcharge', description: 'Modo Express', amount: 6795, percentage: 30, ruleApplied: 'Express delivery -> +30%' }
        ]
      },
      history: [
        { id: 'h1', shipmentId: 'shp-8801', previousStatus: 'Created', newStatus: 'Created', comment: 'Pedido registrado', changedAt: new Date(Date.now() - 86400000).toISOString() },
        { id: 'h2', shipmentId: 'shp-8801', previousStatus: 'Created', newStatus: 'Quoted', comment: 'Cotización calculada', changedAt: new Date(Date.now() - 80000000).toISOString() },
        { id: 'h3', shipmentId: 'shp-8801', previousStatus: 'Quoted', newStatus: 'Confirmed', comment: 'Confirmado por el cliente', changedAt: new Date(Date.now() - 40000000).toISOString() }
      ],
      createdAt: new Date(Date.now() - 86400000).toISOString(),
      updatedAt: new Date(Date.now() - 40000000).toISOString()
    }
  ]);

  // Client-side Shipping Cost Engine Calculation (Matches .NET backend IShippingCostCalculator exactly)
  calculateQuoteLocal(
    weightKg: number,
    lengthCm: number,
    widthCm: number,
    heightCm: number,
    commercialValue: number,
    distanceKm: number,
    deliveryType: 'Standard' | 'Express' | 'SameDay',
    timeWindow: 'Standard' | 'Extended' | 'Night' | 'Weekend'
  ): ShippingQuote {
    const volumetricKg = (lengthCm * widthCm * heightCm) / 5000;
    const billableKg = Math.max(weightKg, volumetricKg);

    let baseCost = 15000;
    let baseRule = 'Tier >2-5 kg -> 15,000 COP';
    if (billableKg <= 2) { baseCost = 10000; baseRule = 'Tier 0-2 kg -> 10,000 COP'; }
    else if (billableKg <= 5) { baseCost = 15000; baseRule = 'Tier >2-5 kg -> 15,000 COP'; }
    else if (billableKg <= 10) { baseCost = 22000; baseRule = 'Tier >5-10 kg -> 22,000 COP'; }
    else if (billableKg <= 20) { baseCost = 35000; baseRule = 'Tier >10-20 kg -> 35,000 COP'; }
    else {
      const extra = Math.ceil(billableKg - 20);
      baseCost = 35000 + extra * 2000;
      baseRule = `Tier >20 kg -> 35,000 + (${extra} kg x 2,000 COP)`;
    }

    const breakdown = [
      { componentName: 'BaseCost', description: 'Tarifa base peso facturable', amount: baseCost, percentage: 0, ruleApplied: baseRule }
    ];

    let distancePct = 0;
    let distRule = 'Distance 0-10 km -> 0%';
    if (distanceKm > 150) { distancePct = 0.50; distRule = 'Distance >150 km -> +50%'; }
    else if (distanceKm > 80) { distancePct = 0.35; distRule = 'Distance >80-150 km -> +35%'; }
    else if (distanceKm > 30) { distancePct = 0.20; distRule = 'Distance >30-80 km -> +20%'; }
    else if (distanceKm > 10) { distancePct = 0.10; distRule = 'Distance >10-30 km -> +10%'; }

    const distanceSurcharge = baseCost * distancePct;
    if (distanceSurcharge > 0) {
      breakdown.push({ componentName: 'DistanceSurcharge', description: 'Recargo por distancia', amount: distanceSurcharge, percentage: distancePct * 100, ruleApplied: distRule });
    }

    let valPct = 0;
    let valRule = 'Commercial value <= 500k -> 0%';
    if (commercialValue > 5000000) { valPct = 0.03; valRule = 'Commercial value > 5M -> +3%'; }
    else if (commercialValue > 2000000) { valPct = 0.02; valRule = 'Commercial value 2M-5M -> +2%'; }
    else if (commercialValue > 500000) { valPct = 0.01; valRule = 'Commercial value 500k-2M -> +1%'; }

    const valSurcharge = baseCost * valPct;
    if (valSurcharge > 0) {
      breakdown.push({ componentName: 'CommercialValueSurcharge', description: 'Recargo valor comercial declarado', amount: valSurcharge, percentage: valPct * 100, ruleApplied: valRule });
    }

    let typePct = 0;
    let typeRule = 'Standard delivery -> 0%';
    if (deliveryType === 'Express') { typePct = 0.30; typeRule = 'Express delivery -> +30%'; }
    else if (deliveryType === 'SameDay') { typePct = 0.60; typeRule = 'Same-day delivery -> +60%'; }

    const currentSub = baseCost + distanceSurcharge + valSurcharge;
    const typeSurcharge = currentSub * typePct;
    if (typeSurcharge > 0) {
      breakdown.push({ componentName: 'DeliveryTypeSurcharge', description: 'Recargo velocidad entrega', amount: typeSurcharge, percentage: typePct * 100, ruleApplied: typeRule });
    }

    let winPct = 0;
    let winRule = 'Standard window -> 0%';
    if (timeWindow === 'Extended') { winPct = 0.10; winRule = 'Extended window -> +10%'; }
    else if (timeWindow === 'Night') { winPct = 0.20; winRule = 'Night delivery -> +20%'; }
    else if (timeWindow === 'Weekend') { winPct = 0.25; winRule = 'Weekend delivery -> +25%'; }

    const windowSurcharge = currentSub * winPct;
    if (windowSurcharge > 0) {
      breakdown.push({ componentName: 'TimeWindowSurcharge', description: 'Recargo ventana horaria', amount: windowSurcharge, percentage: winPct * 100, ruleApplied: winRule });
    }

    const total = baseCost + distanceSurcharge + valSurcharge + typeSurcharge + windowSurcharge;

    return {
      baseCost,
      weightSurcharge: 0,
      distanceSurcharge,
      commercialValueSurcharge: valSurcharge,
      deliveryTypeSurcharge: typeSurcharge,
      timeWindowSurcharge: windowSurcharge,
      discount: 0,
      total,
      actualWeightKg: weightKg,
      volumetricWeightKg: Math.round(volumetricKg * 100) / 100,
      billableWeightKg: Math.round(billableKg * 100) / 100,
      breakdown
    };
  }

  createShipment(req: any): Shipment {
    const deliveryTypeMap = ['Standard', 'Express', 'SameDay'] as const;
    const deliveryWindowMap = ['Standard', 'Extended', 'Night', 'Weekend'] as const;

    const delType = deliveryTypeMap[req.deliveryType] || 'Standard';
    const delWin = deliveryWindowMap[req.deliveryWindow] || 'Standard';

    const quote = this.calculateQuoteLocal(
      req.weightKg, req.lengthCm, req.widthCm, req.heightCm,
      req.commercialValue, req.distanceKm, delType, delWin
    );

    const newShipment: Shipment = {
      id: `shp-${Math.floor(1000 + Math.random() * 9000)}`,
      customerId: req.customerId,
      origin: req.origin,
      destination: req.destination,
      weightKg: req.weightKg,
      lengthCm: req.lengthCm,
      widthCm: req.widthCm,
      heightCm: req.heightCm,
      commercialValue: req.commercialValue,
      distanceKm: req.distanceKm,
      deliveryType: delType,
      deliveryWindow: delWin,
      status: 'Quoted',
      baseCost: quote.baseCost,
      totalCost: quote.total,
      quote: quote,
      history: [
        { id: `h-${Date.now()}-1`, shipmentId: '', previousStatus: 'Created', newStatus: 'Created', comment: 'Envío creado', changedAt: new Date().toISOString() },
        { id: `h-${Date.now()}-2`, shipmentId: '', previousStatus: 'Created', newStatus: 'Quoted', comment: 'Cotización inicial calculada', changedAt: new Date().toISOString() }
      ],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };

    this.shipments.update(list => [newShipment, ...list]);
    return newShipment;
  }

  updateStatus(shipmentId: string, newStatus: any, comment: string) {
    this.shipments.update(list => list.map(s => {
      if (s.id === shipmentId) {
        const oldStatus = s.status;
        return {
          ...s,
          status: newStatus,
          updatedAt: new Date().toISOString(),
          history: [
            ...s.history,
            { id: `h-${Date.now()}`, shipmentId, previousStatus: oldStatus, newStatus, comment: comment || `Estado cambiado a ${newStatus}`, changedAt: new Date().toISOString() }
          ]
        };
      }
      return s;
    }));
  }
}
