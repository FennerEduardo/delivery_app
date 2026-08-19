import { test, expect } from '@playwright/test';

test.describe('Customer Registry Feature', () => {

  test('should display registered customer list', async ({ page }) => {
    await page.goto('/');
    await page.click('button:has-text("Customers")');

    await expect(page.locator('h2')).toContainText('Customer Registry');
    const table = page.locator('table');
    await expect(table).toBeVisible();
    await expect(table).toContainText('Empresa Logística Alfa');
    await expect(table).toContainText('logistica@alfa.com.co');
  });

});
