import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import LoginPage from '@/app/(auth)/login/page';

const mockPush = vi.fn();
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}));

const mockSetUser = vi.fn();
vi.mock('@/stores/authStore', () => ({
  useAuthStore: () => ({ setUser: mockSetUser }),
}));

vi.mock('@/lib/apiClient', () => ({
  authApi: {
    login: vi.fn(),
    logout: vi.fn(),
  },
}));

import { authApi } from '@/lib/apiClient';

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders email and password fields', () => {
    render(<LoginPage />);
    expect(screen.getByLabelText('Email')).toBeInTheDocument();
    expect(screen.getByLabelText('Password')).toBeInTheDocument();
  });

  it('renders forgot password link', () => {
    render(<LoginPage />);
    expect(screen.getByText('Forgot your password?')).toBeInTheDocument();
  });

  it('renders register link', () => {
    render(<LoginPage />);
    expect(screen.getByText('Register')).toBeInTheDocument();
  });

  it('shows validation error for empty submit', async () => {
    render(<LoginPage />);
    fireEvent.click(screen.getByRole('button', { name: /log in/i }));
    await waitFor(() => {
      expect(screen.getByText('Enter a valid email address')).toBeInTheDocument();
    });
  });

  it('shows server error on failed login', async () => {
    vi.mocked(authApi.login).mockRejectedValueOnce(new Error('Unauthorized'));
    render(<LoginPage />);

    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'test@example.com' },
    });
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'password123' },
    });
    fireEvent.click(screen.getByRole('button', { name: /log in/i }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument();
      expect(screen.getByText(/Invalid email or password/)).toBeInTheDocument();
    });
  });

  it('redirects to home on successful login', async () => {
    vi.mocked(authApi.login).mockResolvedValueOnce({
      data: { user: { id: '1', name: 'Alice', email: 'alice@example.com', role: 'user' }, accessToken: 'token' },
    } as never);

    render(<LoginPage />);

    fireEvent.change(screen.getByLabelText('Email'), {
      target: { value: 'alice@example.com' },
    });
    fireEvent.change(screen.getByLabelText('Password'), {
      target: { value: 'password123' },
    });
    fireEvent.click(screen.getByRole('button', { name: /log in/i }));

    await waitFor(() => {
      expect(mockSetUser).toHaveBeenCalled();
      expect(mockPush).toHaveBeenCalledWith('/');
    });
  });
});
