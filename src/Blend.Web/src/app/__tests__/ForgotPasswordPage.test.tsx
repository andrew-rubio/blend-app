import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import ForgotPasswordPage from '@/app/(auth)/forgot-password/page';

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn() }),
}));

vi.mock('@/lib/apiClient', () => ({
  authApi: {
    forgotPassword: vi.fn(),
  },
}));

import { authApi } from '@/lib/apiClient';

describe('ForgotPasswordPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders email field and submit button', () => {
    render(<ForgotPasswordPage />);
    expect(screen.getByLabelText('Email')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /send reset link/i })).toBeInTheDocument();
  });

  it('shows validation error for invalid email', async () => {
    render(<ForgotPasswordPage />);
    fireEvent.change(screen.getByLabelText('Email'), { target: { value: 'not-an-email' } });
    fireEvent.click(screen.getByRole('button', { name: /send reset link/i }));
    await waitFor(() => {
      expect(screen.getByText('Enter a valid email address')).toBeInTheDocument();
    });
  });

  it('shows success message after submission (even on error)', async () => {
    vi.mocked(authApi.forgotPassword).mockRejectedValueOnce(new Error('Not found'));
    render(<ForgotPasswordPage />);
    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'any@example.com' },
    });
    fireEvent.click(screen.getByRole('button', { name: /send reset link/i }));
    await waitFor(() => {
      expect(screen.getByText('Check your email')).toBeInTheDocument();
    });
  });

  it('shows success message on successful submission', async () => {
    vi.mocked(authApi.forgotPassword).mockResolvedValueOnce({ data: {} } as never);
    render(<ForgotPasswordPage />);
    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'user@example.com' },
    });
    fireEvent.click(screen.getByRole('button', { name: /send reset link/i }));
    await waitFor(() => {
      expect(screen.getByText('Check your email')).toBeInTheDocument();
    });
  });
});
