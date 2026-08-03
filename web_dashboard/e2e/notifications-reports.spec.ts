import { test, expect } from '@playwright/test';

test.describe('Notifications & Reports', () => {
  test.beforeEach(async ({ page }) => {
    // Mock authentication
    await page.route('**/api/Auth/login', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          token: 'mock-jwt-token',
          refreshToken: 'mock-refresh-token',
          expiration: new Date(Date.now() + 3600000).toISOString(),
          user: { id: 1, email: 'admin@schedulas.com', role: 'Admin', institutionId: 1 }
        })
      });
    });

    // Mock notifications
    await page.route('**/api/Notifications*', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [{ id: 1, title: 'System Alert', message: 'Maintenance scheduled', isRead: false }],
          totalCount: 1,
          pageNumber: 1,
          pageSize: 10
        })
      });
    });

    await page.goto('/login');
    await page.getByLabel(/email/i).fill('admin@schedulas.com');
    await page.getByLabel(/password/i).fill('Pass123!');
    await page.getByRole('button', { name: /login/i }).click();
    await expect(page).toHaveURL('/');
  });

  test('should display notifications', async ({ page }) => {
    await page.click('text=Notifications');
    
    await expect(page).toHaveURL('/notifications');
    await expect(page.getByText('System Alert')).toBeVisible();
  });
});
