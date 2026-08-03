import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, it, expect, vi } from 'vitest';
import LoginPage from '../features/auth/pages/LoginPage';
import { AuthProvider } from '../features/auth/hooks/AuthProvider';
import { ThemeContextProvider } from '../theme/ThemeContextProvider';

// Mock translation
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (_key: string, fallback: string) => fallback,
    i18n: { changeLanguage: vi.fn() },
  }),
}));

const queryClient = new QueryClient();

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <QueryClientProvider client={queryClient}>
    <BrowserRouter>
      <ThemeContextProvider>
        <AuthProvider>
          {children}
        </AuthProvider>
      </ThemeContextProvider>
    </BrowserRouter>
  </QueryClientProvider>
);

describe('LoginPage', () => {
  it('renders login form correctly', () => {
    render(<LoginPage />, { wrapper });
    
    expect(screen.getByRole('heading', { name: /welcome back/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/email address/i)).toBeInTheDocument();
    expect(screen.getByLabelText('Password')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });
});
