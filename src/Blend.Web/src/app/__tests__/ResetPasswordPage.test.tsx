import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ResetPasswordForm } from '@/app/(auth)/reset-password/ResetPasswordForm';

const mockPush = vi.fn();
let mockToken: string | null = 'valid-token';

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
  useSearchParams: () => ({ get: (key: string) => (key === 'token' ? mockToken : null) }),
}));

vi.mock('@/lib/apiClient', () => ({
  authApi: {
    resetPassword: vi.fn(),
  },
}));

import { authApi } from '@/lib/apiClient';

describe('ResetPasswordForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockToken = 'valid-token';
  });

  it('shows error when no token is present', () => {
    mockToken = null;
    render(<ResetPasswordForm />);
    expect(screen.getByText('Invalid link')).toBeInTheDocument();
    expect(screen.getByText('Request new link')).toBeInTheDocument();
  });

  it('renders password fields when token is present', () => {
    render(<ResetPasswordForm />);
    expect(screen.getByLabelText('New password')).toBeInTheDocument();
    expect(screen.getByLabelText('Confirm new password')).toBeInTheDocument();
  });

  it('shows success on valid password reset', async () => {
    vi.mocked(authApi.resetPassword).mockResolvedValueOnce({ data: {} } as never);
    render(<ResetPasswordForm />);

    fireEvent.change(screen.getByLabelText('New password'), {
      target: { value: 'NewPass1word' },
    });
    fireEvent.change(screen.getByLabelText('Confirm new password'), {
      target: { value: 'NewPass1word' },
    });
    fireEvent.click(screen.getByRole('button', { name: /reset password/i }));

    await waitFor(() => {
      expect(screen.getByText('Password updated')).toBeInTheDocument();
    });
  });

  it('shows error on invalid/expired token', async () => {
    vi.mocked(authApi.resetPassword).mockRejectedValueOnce(new Error('Invalid token'));
    render(<ResetPasswordForm />);

    fireEvent.change(screen.getByLabelText('New password'), {
      target: { value: 'NewPass1word' },
    });
    fireEvent.change(screen.getByLabelText('Confirm new password'), {
      target: { value: 'NewPass1word' },
    });
    fireEvent.click(screen.getByRole('button', { name: /reset password/i }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument();
    });
  });

  it('shows validation error when passwords do not match', async () => {
    render(<ResetPasswordForm />);

    fireEvent.change(screen.getByLabelText('New password'), {
      target: { value: 'NewPass1word' },
    });
    fireEvent.change(screen.getByLabelText('Confirm new password'), {
      target: { value: 'DifferentPass1' },
    });
    fireEvent.click(screen.getByRole('button', { name: /reset password/i }));

    await waitFor(() => {
      expect(screen.getByText('Passwords do not match')).toBeInTheDocument();
    });
  });
});
