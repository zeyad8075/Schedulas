import { test, expect } from '@playwright/test';

test.describe('Activities & Scheduling', () => {
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

    // Mock activities list
    await page.route('**/api/Activities*', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [{ id: 1, title: 'Math 101 Lecture', type: 'Lecture', status: 'Scheduled' }],
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

  test('should display activities list', async ({ page }) => {
    await page.click('text=Activities & Scheduling');
    await page.click('text=Manage Activities');

    await expect(page).toHaveURL('/activities');
    await expect(page.getByText('Math 101 Lecture')).toBeVisible();
  });
});
