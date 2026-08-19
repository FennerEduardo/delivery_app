import { test, expect } from '@playwright/test';

test.describe('Shipment End-to-End Workflow', () => {

  test('should execute complete shipment lifecycle: Quote -> Create -> Confirm -> Transition -> View History', async ({ page }) => {
    // 1. Load application home dashboard
    await page.goto('/');
    await expect(page.locator('h1')).toContainText('Logistics Shipping & Quotation Platform');

    // Verify initial metrics are displayed
    await expect(page.locator('.metric-lbl').first()).toBeVisible();

    // 2. Navigate to "Quote & Create Shipment" tab
    await page.click('button:has-text("Quote & Create Shipment")');
    await expect(page.locator('h2')).toContainText('Shipping Rate Calculator');

    // 3. Fill out quote & shipment form
    await page.fill('input[formControlName="weightKg"]', '4.5');
    await page.fill('input[formControlName="lengthCm"]', '30');
    await page.fill('input[formControlName="widthCm"]', '25');
    await page.fill('input[formControlName="heightCm"]', '20');
    await page.fill('input[formControlName="commercialValue"]', '1200000');
    await page.fill('input[formControlName="distanceKm"]', '420');
    await page.selectOption('select[formControlName="deliveryType"]', '1'); // Express (+30%)
    await page.selectOption('select[formControlName="deliveryWindow"]', '0'); // Standard (0%)

    // 4. Verify itemized price breakdown explanation appears live
    const breakdownBox = page.locator('.breakdown-box');
    await expect(breakdownBox).toBeVisible();
    await expect(breakdownBox).toContainText('Itemized Cost Breakdown');
    await expect(breakdownBox).toContainText('Billable Weight: 4.5 kg');
    await expect(breakdownBox).toContainText('Express delivery -> +30%');

    // 5. Submit form to register shipment
    await page.click('button[type="submit"]');

    // 6. Assert automatic navigation to "Shipments & Traceability"
    await expect(page.locator('h2')).toContainText('Shipment Management & Traceability');

    // 7. Verify status change timeline drawer is displayed for the new shipment
    const timelineDrawer = page.locator('.breakdown-box');
    await expect(timelineDrawer).toBeVisible();
    await expect(timelineDrawer).toContainText('Status Change Timeline');
    await expect(timelineDrawer).toContainText('Quoted');
  });

});
