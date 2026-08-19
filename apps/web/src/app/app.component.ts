import '@angular/compiler';
import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ShippingApiService } from './core/services/shipping-api.service';
import { CustomerDto, ShipmentDto, ShippingQuote } from './core/models/shipping.models';
import { CopCurrencyPipe } from './shared/pipes/cop-currency.pipe';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, CopCurrencyPipe],
  template: `
    <div class="app-container">
      <!-- Sidebar Navigation -->
      <aside class="sidebar">
        <div class="sidebar-logo">
          <div class="sidebar-logo-icon">📦</div>
          <div>
            <div>Logistics<span class="gradient-text">Pro</span></div>
            <div style="font-size: 0.7rem; color: var(--text-muted); font-weight: 400;">v2.0 Gherkin AI Engine</div>
          </div>
        </div>

        <ul class="nav-menu">
          <li class="nav-item" [class.active]="activeTab() === 'dashboard'">
            <button (click)="activeTab.set('dashboard')">
              <span>📊</span> Dashboard
            </button>
          </li>
          <li class="nav-item" [class.active]="activeTab() === 'create-shipment'">
            <button (click)="activeTab.set('create-shipment')">
              <span>⚡</span> Quote & Create Shipment
            </button>
          </li>
          <li class="nav-item" [class.active]="activeTab() === 'shipments'">
            <button (click)="activeTab.set('shipments')">
              <span>🚚</span> Shipments & Traceability
            </button>
          </li>
          <li class="nav-item" [class.active]="activeTab() === 'customers'">
            <button (click)="activeTab.set('customers')">
              <span>👥</span> Customers
            </button>
          </li>
          <li class="nav-item" [class.active]="activeTab() === 'gherkin-docs'">
            <button (click)="activeTab.set('gherkin-docs')">
              <span>🥒</span> Gherkin Specs & ADRs
            </button>
          </li>
        </ul>
      </aside>

      <!-- Main Content Container -->
      <main class="main-content">

        <!-- Header -->
        <header style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px;">
          <div>
            <h1 style="font-size: 1.8rem;">Logistics Shipping & Quotation Platform</h1>
            <p style="color: var(--text-muted); font-size: 0.9rem; margin-top: 4px;">
              Technical Interview Case Study — Clean Architecture .NET 10, Stand-Alone Angular & Gherkin AI Engine
            </p>
          </div>
          <button class="btn btn-primary" (click)="activeTab.set('create-shipment')">
            + New Shipment
          </button>
        </header>

        <!-- TAB 1: DASHBOARD -->
        <section *ngIf="activeTab() === 'dashboard'">
          <div class="metrics-grid">
            <div class="card metric-card">
              <div class="metric-icon metric-blue">📦</div>
              <div>
                <div class="metric-val">{{ totalShipments() }}</div>
                <div class="metric-lbl">Total Shipments</div>
              </div>
            </div>
            <div class="card metric-card">
              <div class="metric-icon metric-amber">⏳</div>
              <div>
                <div class="metric-val">{{ pendingShipments() }}</div>
                <div class="metric-lbl">Quoted / Pending</div>
              </div>
            </div>
            <div class="card metric-card">
              <div class="metric-icon metric-purple">🚚</div>
              <div>
                <div class="metric-val">{{ inTransitShipments() }}</div>
                <div class="metric-lbl">In Transit</div>
              </div>
            </div>
            <div class="card metric-card">
              <div class="metric-icon metric-emerald">✅</div>
              <div>
                <div class="metric-val">{{ avgShippingCost() | copCurrency }}</div>
                <div class="metric-lbl">Average Shipping Cost</div>
              </div>
            </div>
          </div>

          <div class="card" style="margin-top: 24px;">
            <h3 style="margin-bottom: 16px;">Recent Shipments</h3>
            <div class="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Shipment ID</th>
                    <th>Origin ➔ Destination</th>
                    <th>Actual / Volumetric Weight</th>
                    <th>Delivery Type</th>
                    <th>Total Cost</th>
                    <th>Status</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let s of shipmentsList()">
                    <td style="font-family: monospace; font-weight: 700; color: var(--accent-cyan);">{{ s.trackingNumber || s.id }}</td>
                    <td>{{ s.originCity }} ➔ {{ s.destinationCity }}</td>
                    <td>{{ s.weightKg }} kg</td>
                    <td><span class="rule-pill">{{ s.deliveryType }} ({{ s.deliveryWindow }})</span></td>
                    <td style="font-weight: 700; color: var(--accent-emerald);">{{ s.quotedPrice | copCurrency }}</td>
                    <td>
                      <span class="badge" [ngClass]="getBadgeClass(s.status)">{{ s.status }}</span>
                    </td>
                    <td>
                      <button class="btn btn-secondary" style="padding: 4px 10px; font-size: 0.8rem;" (click)="selectShipment(s)">View Details</button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </section>

        <!-- TAB 2: COTIZADOR & CREAR ENVÍO -->
        <section *ngIf="activeTab() === 'create-shipment'">
          <div class="card">
            <h2 style="margin-bottom: 8px;">Shipping Rate Calculator</h2>
            <p style="color: var(--text-muted); margin-bottom: 24px; font-size: 0.9rem;">
              Enter package dimensions, actual weight, origin, destination, and commercial value to calculate itemized shipping cost breakdown.
            </p>

            <form [formGroup]="shipmentForm" (ngSubmit)="onSubmitShipment()">
              <div class="form-grid">

                <div class="form-group">
                  <label class="form-label">Customer *</label>
                  <select class="form-control" formControlName="customerId">
                    <option *ngFor="let c of customersList()" [value]="c.id">{{ c.fullName }} ({{ c.email }})</option>
                  </select>
                </div>

                <div class="form-group">
                  <label class="form-label">Actual Weight (kg) *</label>
                  <input type="number" class="form-control" formControlName="weightKg" (input)="onFormChange()" step="0.1">
                </div>

                <div class="form-group">
                  <label class="form-label">Length (cm) *</label>
                  <input type="number" class="form-control" formControlName="lengthCm" (input)="onFormChange()">
                </div>

                <div class="form-group">
                  <label class="form-label">Width (cm) *</label>
                  <input type="number" class="form-control" formControlName="widthCm" (input)="onFormChange()">
                </div>

                <div class="form-group">
                  <label class="form-label">Height (cm) *</label>
                  <input type="number" class="form-control" formControlName="heightCm" (input)="onFormChange()">
                </div>

                <div class="form-group">
                  <label class="form-label">Commercial Declared Value (COP) *</label>
                  <input type="number" class="form-control" formControlName="commercialValue" (input)="onFormChange()">
                </div>

                <div class="form-group">
                  <label class="form-label">Estimated Distance (km) *</label>
                  <input type="number" class="form-control" formControlName="distanceKm" (input)="onFormChange()">
                </div>

                <div class="form-group">
                  <label class="form-label">Delivery Type *</label>
                  <select class="form-control" formControlName="deliveryType" (change)="onFormChange()">
                    <option [value]="0">Standard (0%)</option>
                    <option [value]="1">Express (+30%)</option>
                    <option [value]="2">SameDay (+60%)</option>
                  </select>
                </div>

                <div class="form-group">
                  <label class="form-label">Time Window *</label>
                  <select class="form-control" formControlName="deliveryWindow" (change)="onFormChange()">
                    <option [value]="0">Standard (0%)</option>
                    <option [value]="1">Extended (+10%)</option>
                    <option [value]="2">Night (+20%)</option>
                    <option [value]="3">Weekend (+25%)</option>
                  </select>
                </div>

              </div>

              <!-- Real-time Live Price Breakdown Explanation Box -->
              <div class="breakdown-box" *ngIf="liveQuote()">
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
                  <h3 style="font-size: 1.1rem; color: #60a5fa;">Itemized Cost Breakdown (Rules Engine)</h3>
                  <span class="badge badge-quoted">Billable Weight: {{ liveQuote()?.billableWeightKg }} kg</span>
                </div>

                <div style="font-size: 0.85rem; color: var(--text-muted); margin-bottom: 12px;">
                  Pricing Version: <strong>{{ liveQuote()?.pricingVersion || '2026.08' }}</strong> | Actual Weight: <strong>{{ liveQuote()?.actualWeightKg }} kg</strong> | Volumetric Weight: <strong>{{ liveQuote()?.volumetricWeightKg }} kg</strong> (Divisor 5000)
                </div>

                <div class="breakdown-row" *ngFor="let item of liveQuote()?.breakdownComponents">
                  <div>
                    <strong>{{ item.description }}</strong>
                    <div class="rule-pill" style="margin-top: 2px;">{{ item.ruleApplied }}</div>
                  </div>
                  <div style="font-weight: 700; color: #f8fafc;">
                    +{{ item.amount | copCurrency }}
                  </div>
                </div>

                <div class="breakdown-row total">
                  <span>ESTIMATED TOTAL SHIPPING COST:</span>
                  <span>{{ liveQuote()?.total?.amount | copCurrency }}</span>
                </div>
              </div>

              <div style="margin-top: 24px; display: flex; gap: 12px;">
                <button type="submit" class="btn btn-primary" [disabled]="shipmentForm.invalid">
                  ✓ Confirm & Register Shipment
                </button>
              </div>
            </form>
          </div>
        </section>

        <!-- TAB 3: ENVÍOS & HISTORIAL TIMELINE -->
        <section *ngIf="activeTab() === 'shipments'">
          <div class="card">
            <h2>Shipment Management & Traceability</h2>
            <p style="color: var(--text-muted); margin-bottom: 20px; font-size: 0.9rem;">
              Inspect status transition logs and domain invariants for the <code>Shipment</code> aggregate root.
            </p>

            <div class="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Shipment ID</th>
                    <th>Origin ➔ Destination</th>
                    <th>Customer</th>
                    <th>Weight</th>
                    <th>Total Cost</th>
                    <th>Current Status</th>
                    <th>Transition State</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let s of shipmentsList()">
                    <td style="font-family: monospace; font-weight: 700; color: var(--accent-cyan);">{{ s.trackingNumber || s.id }}</td>
                    <td>{{ s.originCity }} ➔ {{ s.destinationCity }}</td>
                    <td>{{ s.customerName }}</td>
                    <td>{{ s.weightKg }} kg</td>
                    <td style="font-weight: 700; color: var(--accent-emerald);">{{ s.quotedPrice | copCurrency }}</td>
                    <td><span class="badge" [ngClass]="getBadgeClass(s.status)">{{ s.status }}</span></td>
                    <td>
                      <select class="form-control" style="padding: 4px 8px; font-size: 0.8rem;" (change)="onStatusChange(s.id, $event)">
                        <option value="">-- Change Status --</option>
                        <option value="Confirmed" *ngIf="s.status === 'Quoted' || s.status === 'Created'">Confirm</option>
                        <option value="InTransit" *ngIf="s.status === 'Confirmed'">In Transit</option>
                        <option value="Delivered" *ngIf="s.status === 'InTransit'">Delivered</option>
                        <option value="Cancelled" *ngIf="s.status !== 'Delivered' && s.status !== 'Cancelled'">Cancel</option>
                      </select>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Detail Modal / Drawer -->
            <div *ngIf="selectedShipment()" class="breakdown-box" style="margin-top: 32px; background: rgba(18, 24, 38, 0.95);">
              <div style="display: flex; justify-content: space-between; align-items: center;">
                <h3>Traceability Timeline & Details: {{ selectedShipment()?.trackingNumber || selectedShipment()?.id }}</h3>
                <button class="btn btn-secondary" style="padding: 4px 10px;" (click)="selectedShipment.set(null)">✕ Close</button>
              </div>

              <div style="margin-top: 16px;">
                <h4 style="color: var(--accent-cyan); margin-bottom: 8px;">Status Change Timeline:</h4>
                <div style="display: flex; flex-direction: column; gap: 10px; margin-top: 12px;">
                  <div *ngFor="let h of selectedShipment()?.statusHistory" style="padding: 10px; background: rgba(255,255,255,0.03); border-left: 3px solid var(--accent-primary); border-radius: 4px;">
                    <div style="display: flex; justify-content: space-between; font-size: 0.85rem;">
                      <strong>State: {{ h.status }}</strong>
                      <span style="color: var(--text-dim);">{{ h.timestamp | date:'medium' }}</span>
                    </div>
                    <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 2px;">{{ h.notes }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- TAB 4: CLIENTES -->
        <section *ngIf="activeTab() === 'customers'">
          <div class="card">
            <h2>Customer Registry</h2>
            <div class="table-container" style="margin-top: 16px;">
              <table>
                <thead>
                  <tr>
                    <th>Customer ID</th>
                    <th>Name / Company</th>
                    <th>Email</th>
                    <th>Phone</th>
                    <th>City / Country</th>
                    <th>Registered At</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let c of customersList()">
                    <td style="font-family: monospace; font-weight: 700; color: var(--accent-purple);">{{ c.id }}</td>
                    <td style="font-weight: 600;">{{ c.fullName }}</td>
                    <td>{{ c.email }}</td>
                    <td>{{ c.phoneNumber }}</td>
                    <td>{{ c.city }}, {{ c.country }}</td>
                    <td style="color: var(--text-dim); font-size: 0.85rem;">{{ c.createdAt | date:'shortDate' }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </section>

        <!-- TAB 5: SPECS GHERKIN & ADRS -->
        <section *ngIf="activeTab() === 'gherkin-docs'">
          <div class="card">
            <h2>🥒 Gherkin BDD Specifications (gherkin-ai v2.0.0-beta.1)</h2>
            <p style="color: var(--text-muted); margin-bottom: 20px; font-size: 0.9rem;">
              Executable business specifications orchestrated by <strong>gherkin-ai</strong> engine (<a href="https://fennereduardo.com/pages/GherkinIATool/" target="_blank" style="color: var(--accent-cyan);">fennereduardo.com</a>).
            </p>

            <div class="breakdown-box" style="font-family: monospace; font-size: 0.85rem; line-height: 1.5; color: #a78bfa;">
              <pre>Feature: Shipping Quote Calculation
  As a logistics operator or customer
  I want to calculate shipment costs with detailed cost breakdowns
  So that I understand exact pricing factors, surcharges, and total rates

  Scenario: Calculate standard shipping quote for lightweight item
    Given a customer with ID "cust-001"
    And a shipment with actual weight 3.0 kg
    And package dimensions length 20 cm, width 15 cm, height 10 cm
    And a commercial value of 200000 COP
    And a delivery distance of 25 km
    And a delivery type "Standard"
    When the shipping quote is calculated
    Then the billable weight should be 3.0 kg
    And the base cost should be 15000 COP
    And the total shipping cost should be 16500 COP</pre>
            </div>
          </div>
        </section>

      </main>
    </div>
  `,
  styles: []
})
export class AppComponent {
  apiService = inject(ShippingApiService);
  fb = inject(FormBuilder);

  activeTab = signal<'dashboard' | 'create-shipment' | 'shipments' | 'customers' | 'gherkin-docs'>('dashboard');
  selectedShipment = signal<ShipmentDto | null>(null);

  customersList = signal<CustomerDto[]>([
    {
      id: 'cust-101',
      fullName: 'Empresa Logística Alfa',
      companyName: 'Alfa Cargo S.A.S.',
      email: 'logistica@alfa.com.co',
      phoneNumber: '+57 300 123 4567',
      street: 'Calle 26 # 68-90',
      city: 'Bogotá',
      state: 'Cundinamarca',
      zipCode: '110911',
      country: 'Colombia',
      isVIP: true,
      createdAt: new Date().toISOString()
    },
    {
      id: 'cust-102',
      fullName: 'Distribuidora Medellín Express',
      companyName: 'Medellín Express',
      email: 'operaciones@medellinexpress.com',
      phoneNumber: '+57 310 987 6543',
      street: 'Carrera 43A # 1-50',
      city: 'Medellín',
      state: 'Antioquia',
      zipCode: '050021',
      country: 'Colombia',
      isVIP: false,
      createdAt: new Date().toISOString()
    }
  ]);

  shipmentsList = signal<ShipmentDto[]>([
    {
      id: 'ship-801',
      trackingNumber: 'TRK-BOG-MDE-801',
      customerId: 'cust-101',
      customerName: 'Empresa Logística Alfa',
      originCity: 'Bogotá',
      destinationCity: 'Medellín',
      weightKg: 4.5,
      lengthCm: 30,
      widthCm: 25,
      heightCm: 20,
      commercialValue: 1200000,
      distanceKm: 420,
      deliveryType: 'Express',
      deliveryWindow: 'Standard',
      status: 'InTransit',
      quotedPrice: 38350,
      pricingVersion: '2026.08',
      createdAt: new Date().toISOString(),
      statusHistory: [
        { id: 'h-1', status: 'Quoted', notes: 'Quote calculated via rules engine v2026.08', changedBy: 'System', timestamp: new Date().toISOString() },
        { id: 'h-2', status: 'Confirmed', notes: 'Payment confirmed and label printed', changedBy: 'Operator', timestamp: new Date().toISOString() },
        { id: 'h-3', status: 'InTransit', notes: 'Package dispatched on truck #402', changedBy: 'Driver', timestamp: new Date().toISOString() }
      ]
    }
  ]);

  liveQuote = signal<ShippingQuote | null>(null);

  totalShipments = computed(() => this.shipmentsList().length);
  pendingShipments = computed(() => this.shipmentsList().filter(s => s.status === 'Quoted' || s.status === 'Created').length);
  inTransitShipments = computed(() => this.shipmentsList().filter(s => s.status === 'InTransit').length);
  avgShippingCost = computed(() => {
    const list = this.shipmentsList();
    if (!list.length) return 0;
    return list.reduce((acc, s) => acc + s.quotedPrice, 0) / list.length;
  });

  shipmentForm = this.fb.group({
    customerId: ['cust-101', Validators.required],
    weightKg: [4.5, [Validators.required, Validators.min(0.1)]],
    lengthCm: [30, [Validators.required, Validators.min(1)]],
    widthCm: [25, [Validators.required, Validators.min(1)]],
    heightCm: [20, [Validators.required, Validators.min(1)]],
    commercialValue: [1200000, [Validators.required, Validators.min(0)]],
    distanceKm: [420, [Validators.required, Validators.min(0)]],
    deliveryType: [1, Validators.required],
    deliveryWindow: [0, Validators.required],
    originStreet: ['Calle 26 # 68-90'],
    originCity: ['Bogotá'],
    destStreet: ['Carrera 43A # 1-50'],
    destCity: ['Medellín']
  });

  constructor() {
    this.onFormChange();
  }

  onFormChange() {
    if (this.shipmentForm.invalid) return;
    const v = this.shipmentForm.value;

    // Local rules evaluation for immediate UI response
    const actualKg = Number(v.weightKg || 1);
    const volKg = (Number(v.lengthCm || 10) * Number(v.widthCm || 10) * Number(v.heightCm || 10)) / 5000;
    const billableKg = Math.max(actualKg, volKg);

    let baseCost = 15000;
    if (billableKg <= 2) baseCost = 10000;
    else if (billableKg <= 5) baseCost = 15000;
    else if (billableKg <= 10) baseCost = 22000;
    else if (billableKg <= 20) baseCost = 35000;
    else baseCost = 35000 + Math.ceil(billableKg - 20) * 2000;

    const distKm = Number(v.distanceKm || 0);
    let distPct = 0;
    if (distKm > 150) distPct = 0.50;
    else if (distKm > 80) distPct = 0.35;
    else if (distKm > 30) distPct = 0.20;
    else if (distKm > 10) distPct = 0.10;

    const distAmount = baseCost * distPct;

    const val = Number(v.commercialValue || 0);
    let valPct = 0;
    if (val > 5000000) valPct = 0.03;
    else if (val > 2000000) valPct = 0.02;
    else if (val > 50000) valPct = 0.01;

    const valAmount = baseCost * valPct;

    const subtotal = baseCost + distAmount + valAmount;

    const delType = Number(v.deliveryType || 0);
    const delPct = delType === 2 ? 0.60 : (delType === 1 ? 0.30 : 0);
    const delAmount = subtotal * delPct;

    const winType = Number(v.deliveryWindow || 0);
    const winPct = winType === 3 ? 0.25 : (winType === 2 ? 0.20 : (winType === 1 ? 0.10 : 0));
    const winAmount = subtotal * winPct;

    const total = subtotal + delAmount + winAmount;

    this.liveQuote.set({
      pricingVersion: '2026.08',
      quotedAt: new Date().toISOString(),
      actualWeightKg: actualKg,
      volumetricWeightKg: Math.round(volKg * 100) / 100,
      billableWeightKg: Math.round(billableKg * 100) / 100,
      baseCost: { amount: baseCost, currency: 'COP' },
      weightSurcharge: { amount: 0, currency: 'COP' },
      distanceSurcharge: { amount: distAmount, currency: 'COP' },
      commercialValueSurcharge: { amount: valAmount, currency: 'COP' },
      deliveryTypeSurcharge: { amount: delAmount, currency: 'COP' },
      timeWindowSurcharge: { amount: winAmount, currency: 'COP' },
      discount: { amount: 0, currency: 'COP' },
      total: { amount: total, currency: 'COP' },
      appliedRuleIds: ['RULE_WEIGHT_TIER', 'RULE_DISTANCE_SURCHARGE', 'RULE_COMMERCIAL_VALUE_SURCHARGE', 'RULE_DELIVERY_TYPE_SURCHARGE'],
      breakdownComponents: [
        { componentName: 'BaseCost', description: 'Base rate per billable weight', amount: baseCost, percentage: 0, ruleApplied: `Tier ${billableKg} kg -> ${baseCost} COP` },
        { componentName: 'DistanceSurcharge', description: 'Distance range surcharge', amount: distAmount, percentage: distPct * 100, ruleApplied: `Distance ${distKm} km -> +${distPct * 100}%` },
        { componentName: 'CommercialValueSurcharge', description: 'Declared commercial value surcharge', amount: valAmount, percentage: valPct * 100, ruleApplied: `Commercial value -> +${valPct * 100}%` },
        { componentName: 'DeliveryTypeSurcharge', description: 'Delivery speed multiplier', amount: delAmount, percentage: delPct * 100, ruleApplied: `Delivery type -> +${delPct * 100}%` }
      ]
    });
  }

  onSubmitShipment() {
    if (this.shipmentForm.invalid) return;
    const v = this.shipmentForm.value;
    const q = this.liveQuote();

    const newShipment: ShipmentDto = {
      id: `ship-${Date.now().toString().slice(-4)}`,
      trackingNumber: `TRK-COL-${Date.now().toString().slice(-4)}`,
      customerId: v.customerId || 'cust-101',
      customerName: this.getCustomerName(v.customerId || 'cust-101'),
      originCity: v.originCity || 'Bogotá',
      destinationCity: v.destCity || 'Medellín',
      weightKg: Number(v.weightKg),
      lengthCm: Number(v.lengthCm),
      widthCm: Number(v.widthCm),
      heightCm: Number(v.heightCm),
      commercialValue: Number(v.commercialValue),
      distanceKm: Number(v.distanceKm),
      deliveryType: v.deliveryType === 2 ? 'SameDay' : (v.deliveryType === 1 ? 'Express' : 'Standard'),
      deliveryWindow: 'Standard',
      status: 'Confirmed',
      quotedPrice: q?.total?.amount || 25000,
      pricingVersion: q?.pricingVersion || '2026.08',
      createdAt: new Date().toISOString(),
      statusHistory: [
        { id: 'h-1', status: 'Quoted', notes: `Quoted via pricing engine v${q?.pricingVersion || '2026.08'}`, changedBy: 'System', timestamp: new Date().toISOString() },
        { id: 'h-2', status: 'Confirmed', notes: 'Shipment confirmed and registered', changedBy: 'User', timestamp: new Date().toISOString() }
      ]
    };

    this.shipmentsList.update(list => [newShipment, ...list]);
    this.selectedShipment.set(newShipment);
    this.activeTab.set('shipments');
  }

  onStatusChange(shipmentId: string, event: Event) {
    const target = event.target as HTMLSelectElement;
    const newStatus = target.value;
    if (!newStatus) return;

    this.shipmentsList.update(list => list.map(s => {
      if (s.id === shipmentId) {
        return {
          ...s,
          status: newStatus,
          statusHistory: [
            ...s.statusHistory,
            { id: `h-${Date.now()}`, status: newStatus, notes: `Status changed to ${newStatus}`, changedBy: 'Operator', timestamp: new Date().toISOString() }
          ]
        };
      }
      return s;
    }));

    const updated = this.shipmentsList().find(s => s.id === shipmentId);
    if (updated) this.selectedShipment.set(updated);
    target.value = '';
  }

  selectShipment(s: ShipmentDto) {
    this.selectedShipment.set(s);
    this.activeTab.set('shipments');
  }

  getCustomerName(id: string): string {
    const c = this.customersList().find(cust => cust.id === id);
    return c ? c.fullName : id;
  }

  getBadgeClass(status: string): string {
    switch (status) {
      case 'Created': return 'badge-created';
      case 'Quoted': return 'badge-quoted';
      case 'Confirmed': return 'badge-confirmed';
      case 'InTransit': return 'badge-intransit';
      case 'Delivered': return 'badge-delivered';
      case 'Cancelled': return 'badge-cancelled';
      default: return 'badge-created';
    }
  }
}
