import { test, expect } from '@playwright/test';

test.describe('Gherkin Specs & ADRs Viewer', () => {

  test('should display executable Gherkin feature scenarios', async ({ page }) => {
    await page.goto('/');
    await page.click('button:has-text("Gherkin Specs & ADRs")');

    await expect(page.locator('h2')).toContainText('Gherkin BDD Specifications');
    const specBox = page.locator('.breakdown-box');
    await expect(specBox).toBeVisible();
    await expect(specBox).toContainText('Feature: Shipping Quote Calculation');
    await expect(specBox).toContainText('Scenario: Calculate standard shipping quote for lightweight item');
  });

});
