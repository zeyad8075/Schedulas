import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/Auth/login', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          token: 'mock-jwt-token',
          refreshToken: 'mock-refresh-token',
          expiration: new Date(Date.now() + 3600000).toISOString(),
          user: {
            id: 1,
            email: 'admin@schedulas.com',
            firstName: 'Admin',
            lastName: 'User',
            role: 'Admin',
            institutionId: 1
          }
        })
      });
    });
  });

  test('should login successfully and redirect to dashboard', async ({ page }) => {
    await page.goto('/login');
    
    await page.getByLabel(/email/i).fill('admin@schedulas.com');
    await page.getByLabel(/password/i).fill('Password123!');
    await page.getByRole('button', { name: /login/i }).click();

    await expect(page).toHaveURL('/');
  });
});
