import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ShippingApiService } from './services/shipping-api.service';
import { Shipment, ShippingQuote } from '../../../libs/frontend/models/shipping.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
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
              Technical Interview Demonstration — Clean Architecture .NET 10, Angular & Gherkin AI Engine
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
                <div class="metric-val">$ {{ avgShippingCost() | number:'1.0-0' }}</div>
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
                  <tr *ngFor="let s of apiService.shipments()">
                    <td style="font-family: monospace; font-weight: 700; color: var(--accent-cyan);">{{ s.id }}</td>
                    <td>{{ s.origin.city }} ➔ {{ s.destination.city }}</td>
                    <td>{{ s.weightKg }} kg / {{ s.quote?.volumetricWeightKg || 0 }} kg</td>
                    <td><span class="rule-pill">{{ s.deliveryType }} ({{ s.deliveryWindow }})</span></td>
                    <td style="font-weight: 700; color: var(--accent-emerald);">$ {{ s.totalCost | number:'1.0-0' }} COP</td>
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
                    <option *ngFor="let c of apiService.customers()" [value]="c.id">{{ c.name }} ({{ c.email }})</option>
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
                  Actual Weight: <strong>{{ liveQuote()?.actualWeightKg }} kg</strong> | Volumetric Weight: <strong>{{ liveQuote()?.volumetricWeightKg }} kg</strong> (Divisor 5000)
                </div>

                <div class="breakdown-row" *ngFor="let item of liveQuote()?.breakdown">
                  <div>
                    <strong>{{ item.description }}</strong>
                    <div class="rule-pill" style="margin-top: 2px;">{{ item.ruleApplied }}</div>
                  </div>
                  <div style="font-weight: 700; color: #f8fafc;">
                    +$ {{ item.amount | number:'1.0-0' }} COP
                  </div>
                </div>

                <div class="breakdown-row total">
                  <span>ESTIMATED TOTAL SHIPPING COST:</span>
                  <span>$ {{ liveQuote()?.total | number:'1.0-0' }} COP</span>
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
                    <th>Billable Weight</th>
                    <th>Total Cost</th>
                    <th>Current Status</th>
                    <th>Transition State</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let s of apiService.shipments()">
                    <td style="font-family: monospace; font-weight: 700; color: var(--accent-cyan);">{{ s.id }}</td>
                    <td>{{ s.origin.city }} ➔ {{ s.destination.city }}</td>
                    <td>{{ getCustomerName(s.customerId) }}</td>
                    <td>{{ s.quote?.billableWeightKg || s.weightKg }} kg</td>
                    <td style="font-weight: 700; color: var(--accent-emerald);">$ {{ s.totalCost | number:'1.0-0' }}</td>
                    <td><span class="badge" [ngClass]="getBadgeClass(s.status)">{{ s.status }}</span></td>
                    <td>
                      <select class="form-control" style="padding: 4px 8px; font-size: 0.8rem;" (change)="onStatusChange(s.id, $event)">
                        <option value="">-- Change Status --</option>
                        <option value="Confirmed" *ngIf="s.status === 'Quoted'">Confirm</option>
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
                <h3>Traceability Timeline & Details: {{ selectedShipment()?.id }}</h3>
                <button class="btn btn-secondary" style="padding: 4px 10px;" (click)="selectedShipment.set(null)">✕ Close</button>
              </div>

              <div style="margin-top: 16px;">
                <h4 style="color: var(--accent-cyan); margin-bottom: 8px;">Status Change Timeline:</h4>
                <div style="display: flex; flex-direction: column; gap: 10px; margin-top: 12px;">
                  <div *ngFor="let h of selectedShipment()?.history" style="padding: 10px; background: rgba(255,255,255,0.03); border-left: 3px solid var(--accent-primary); border-radius: 4px;">
                    <div style="display: flex; justify-content: space-between; font-size: 0.85rem;">
                      <strong>{{ h.previousStatus }} ➔ {{ h.newStatus }}</strong>
                      <span style="color: var(--text-dim);">{{ h.changedAt | date:'medium' }}</span>
                    </div>
                    <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 2px;">{{ h.comment }}</div>
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
                  <tr *ngFor="let c of apiService.customers()">
                    <td style="font-family: monospace; font-weight: 700; color: var(--accent-purple);">{{ c.id }}</td>
                    <td style="font-weight: 600;">{{ c.name }}</td>
                    <td>{{ c.email }}</td>
                    <td>{{ c.phone }}</td>
                    <td>{{ c.address.city }}, {{ c.address.country }}</td>
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
  selectedShipment = signal<Shipment | null>(null);

  liveQuote = signal<ShippingQuote | null>(null);

  totalShipments = computed(() => this.apiService.shipments().length);
  pendingShipments = computed(() => this.apiService.shipments().filter(s => s.status === 'Created' || s.status === 'Quoted').length);
  inTransitShipments = computed(() => this.apiService.shipments().filter(s => s.status === 'InTransit').length);
  avgShippingCost = computed(() => {
    const list = this.apiService.shipments();
    if (!list.length) return 0;
    return list.reduce((acc, s) => acc + s.totalCost, 0) / list.length;
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

    const delTypeMap = ['Standard', 'Express', 'SameDay'] as const;
    const delWinMap = ['Standard', 'Extended', 'Night', 'Weekend'] as const;

    const q = this.apiService.calculateQuoteLocal(
      Number(v.weightKg || 1),
      Number(v.lengthCm || 10),
      Number(v.widthCm || 10),
      Number(v.heightCm || 10),
      Number(v.commercialValue || 0),
      Number(v.distanceKm || 0),
      delTypeMap[Number(v.deliveryType || 0)],
      delWinMap[Number(v.deliveryWindow || 0)]
    );

    this.liveQuote.set(q);
  }

  onSubmitShipment() {
    if (this.shipmentForm.invalid) return;
    const v = this.shipmentForm.value;

    const req = {
      customerId: v.customerId,
      origin: { street: v.originStreet, city: v.originCity, state: '', zipCode: '', country: 'Colombia' },
      destination: { street: v.destStreet, city: v.destCity, state: '', zipCode: '', country: 'Colombia' },
      weightKg: Number(v.weightKg),
      lengthCm: Number(v.lengthCm),
      widthCm: Number(v.widthCm),
      heightCm: Number(v.heightCm),
      commercialValue: Number(v.commercialValue),
      distanceKm: Number(v.distanceKm),
      deliveryType: Number(v.deliveryType),
      deliveryWindow: Number(v.deliveryWindow)
    };

    const newShipment = this.apiService.createShipment(req);
    this.selectedShipment.set(newShipment);
    this.activeTab.set('shipments');
  }

  onStatusChange(shipmentId: string, event: Event) {
    const target = event.target as HTMLSelectElement;
    if (!target.value) return;

    this.apiService.updateStatus(shipmentId, target.value, `Status manually updated to ${target.value}`);
    target.value = '';
  }

  selectShipment(s: Shipment) {
    this.selectedShipment.set(s);
    this.activeTab.set('shipments');
  }

  getCustomerName(id: string): string {
    const c = this.apiService.customers().find(cust => cust.id === id);
    return c ? c.name : id;
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
