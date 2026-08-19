import { test, expect } from '@playwright/test';

test.describe('Shipping Cost Calculator Business Rules', () => {

  test('should correctly compute volumetric weight exceeding actual weight (L*W*H / 5000)', async ({ page }) => {
    await page.goto('/');
    await page.click('button:has-text("Quote & Create Shipment")');

    // Actual weight: 2 kg, Dimensions: 50x40x30 cm -> Volumetric weight = 60,000 / 5000 = 12 kg
    await page.fill('input[formControlName="weightKg"]', '2.0');
    await page.fill('input[formControlName="lengthCm"]', '50');
    await page.fill('input[formControlName="widthCm"]', '40');
    await page.fill('input[formControlName="heightCm"]', '30');
    await page.fill('input[formControlName="commercialValue"]', '100000');
    await page.fill('input[formControlName="distanceKm"]', '10');

    const breakdownBox = page.locator('.breakdown-box');
    await expect(breakdownBox).toBeVisible();
    await expect(breakdownBox).toContainText('Actual Weight: 2 kg | Volumetric Weight: 12 kg');
    await expect(breakdownBox).toContainText('Billable Weight: 12 kg');
    await expect(breakdownBox).toContainText('Tier >10-20 kg -> 35,000 COP');
  });

  test('should apply distance and commercial value surcharges accurately', async ({ page }) => {
    await page.goto('/');
    await page.click('button:has-text("Quote & Create Shipment")');

    await page.fill('input[formControlName="weightKg"]', '3.0');
    await page.fill('input[formControlName="distanceKm"]', '200'); // >150 km -> +50% distance surcharge
    await page.fill('input[formControlName="commercialValue"]', '3000000'); // 2M-5M -> +2% commercial value surcharge

    const breakdownBox = page.locator('.breakdown-box');
    await expect(breakdownBox).toBeVisible();
    await expect(breakdownBox).toContainText('Distance >150 km -> +50%');
    await expect(breakdownBox).toContainText('Commercial value 2M-5M -> +2%');
  });

});
