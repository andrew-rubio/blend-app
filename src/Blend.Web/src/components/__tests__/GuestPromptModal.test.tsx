import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { GuestPromptModal } from '../features/GuestPromptModal';

// Mock Next.js router
const mockPush = vi.fn();
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}));

describe('GuestPromptModal', () => {
  const onClose = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('does not render when isOpen is false', () => {
    render(<GuestPromptModal isOpen={false} onClose={onClose} />);
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('renders when isOpen is true', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />);
    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('Login required')).toBeInTheDocument();
  });

  it('shows default message', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />);
    expect(screen.getByText('Create an account to access this feature')).toBeInTheDocument();
  });

  it('shows custom message', () => {
    render(
      <GuestPromptModal
        isOpen={true}
        onClose={onClose}
        message="Please log in to use Cook Mode"
      />,
    );
    expect(screen.getByText('Please log in to use Cook Mode')).toBeInTheDocument();
  });

  it('calls onClose and navigates to register', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />);
    fireEvent.click(screen.getByText('Create an account'));
    expect(onClose).toHaveBeenCalled();
    expect(mockPush).toHaveBeenCalledWith('/register');
  });

  it('calls onClose and navigates to login', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />);
    fireEvent.click(screen.getByText('Log in'));
    expect(onClose).toHaveBeenCalled();
    expect(mockPush).toHaveBeenCalledWith('/login');
  });

  it('calls onClose when Maybe later is clicked', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />);
    fireEvent.click(screen.getByText('Maybe later'));
    expect(onClose).toHaveBeenCalled();
    expect(mockPush).not.toHaveBeenCalled();
  });

  it('calls onClose when backdrop is clicked', () => {
    render(<GuestPromptModal isOpen={true} onClose={onClose} />);
    const backdrop = screen.getByRole('dialog');
    fireEvent.click(backdrop);
    expect(onClose).toHaveBeenCalled();
  });
});
