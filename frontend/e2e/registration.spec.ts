import { test, expect } from '@playwright/test';

test.describe('User Registration E2E Flow', () => {
  
  test.beforeEach(async ({ page }) => {
    // Navigate to the Vite React dev server
    await page.goto('/');
  });

  test('Happy Path: Complete a valid registration with multiple addresses and submit successfully', async ({ page }) => {
    // 1. Assert Header is visible
    await expect(page.locator('h1')).toHaveText('User Profile Registration');

    // 2. Fill out Personal Details
    await page.fill('#firstName', 'Mohamed');
    await page.fill('#middleName', 'Ahmed');
    await page.fill('#lastName', 'Ali');
    
    // Choose birthdate that makes age >= 20 (e.g. 1995-05-15)
    await page.fill('#birthDate', '1995-05-15');
    
    // Fill out valid Egypt E.164 phone
    const uniqueEmail = `mohamed.ali.${Date.now()}@example.com`;
    const uniquePhone = `+20100${Math.floor(1000000 + Math.random() * 9000000)}`;
    await page.fill('#mobileNumber', uniquePhone);
    await page.fill('#email', uniqueEmail);

    // 3. Complete first address (Default address card #1 is automatically primary)
    // Wait for lookups to load from database and select the first available Governorate option
    const govSelect = page.locator('#addresses-0-governorate');
    await expect(govSelect).toBeVisible();
    
    // Select first option after placeholder (index 1)
    await govSelect.selectOption({ index: 1 });

    // Wait for dynamic City lookup loading spinner/activation and select first City
    const citySelect = page.locator('#addresses-0-city');
    await expect(citySelect).toBeEnabled({ timeout: 5000 });
    await citySelect.selectOption({ index: 1 });

    // Fill address details
    await page.fill('#addresses-0-street', '90 South Tahrir St.');
    await page.fill('#addresses-0-building', '14B');
    await page.fill('#addresses-0-flat', '12');

    // 4. Test Dynamic Array: Add a second address card
    await page.click('button:has-text("Add Address")');
    
    // Assert second address card was added
    await expect(page.locator('h3:has-text("Address #2")')).toBeVisible();

    // Populate second address card
    const gov2Select = page.locator('#addresses-1-governorate');
    await gov2Select.selectOption({ index: 1 });
    
    const city2Select = page.locator('#addresses-1-city');
    await expect(city2Select).toBeEnabled({ timeout: 5000 });
    await city2Select.selectOption({ index: 1 });

    await page.fill('#addresses-1-street', '50 Sphinx St.');
    await page.fill('#addresses-1-building', '10/2');
    await page.fill('#addresses-1-flat', '3B');

    // 5. Test Primary Address Toggle: Mark the second address as primary
    await page.click('.address-card:has-text("Address #2") button:has-text("Set Primary")');
    
    // Assert Address #2 is now visually primary and Address #1 is demoted
    await expect(page.locator('.address-card:has-text("Address #2") .primary-badge')).toBeVisible();
    await expect(page.locator('.address-card:has-text("Address #1") .primary-badge')).not.toBeVisible();

    // 6. Submit the Registration Form
    await page.click('button[type="submit"]');

    // 7. Assert Success Overlay is displayed
    const successHeader = page.locator('.success-overlay h2');
    await expect(successHeader).toBeVisible({ timeout: 10000 });
    await expect(successHeader).toHaveText('Registration Successful!');

    // Assert that the generated GUID ID is visible
    const guidId = page.locator('.id-value');
    await expect(guidId).toBeVisible();
    const guidText = await guidId.innerText();
    expect(guidText.length).toBeGreaterThan(10); // Standard GUID length is 36
  });

  test('Validation Failure Path: Show correct error alerts on invalid data and blank inputs', async ({ page }) => {
    // 1. Submit blank form to trigger required field validations
    await page.click('button[type="submit"]');

    // Assert standard inline validation errors
    await expect(page.locator('#firstName-error')).toHaveText('First name is required.');
    await expect(page.locator('#lastName-error')).toHaveText('Last name is required.');
    await expect(page.locator('#birthDate-error')).toHaveText('Birth date is required.');
    await expect(page.locator('#mobileNumber-error')).toHaveText('Mobile number is required.');
    await expect(page.locator('#email-error')).toHaveText('Email is required.');

    // 2. Fill invalid formats to trigger schema validation errors
    await page.fill('#firstName', 'Mohamed123'); // digits in name
    await page.fill('#birthDate', '2015-05-15'); // age is under 20
    await page.fill('#mobileNumber', '01006158'); // invalid E.164 format
    await page.fill('#email', 'invalid-email'); // invalid email format

    // Click outside to trigger onTouched checks
    await page.click('h1');

    // Assert schema rule violations
    await expect(page.locator('#firstName-error')).toContainText('must only contain');
    await expect(page.locator('#birthDate-error')).toHaveText('Minimum age is 20 years old at submission date.');
    await expect(page.locator('#mobileNumber-error')).toHaveText('Mobile number must be in E.164 format (e.g. +201006158123).');
    await expect(page.locator('#email-error')).toHaveText('Email must be in a valid format.');
  });

});
