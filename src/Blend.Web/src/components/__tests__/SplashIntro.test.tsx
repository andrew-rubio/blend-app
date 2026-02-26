import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { SplashIntro, useSplashIntro } from '../features/SplashIntro';
import { renderHook, act } from '@testing-library/react';

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: vi.fn((key: string) => store[key] ?? null),
    setItem: vi.fn((key: string, value: string) => {
      store[key] = value;
    }),
    removeItem: vi.fn((key: string) => {
      delete store[key];
    }),
    clear: vi.fn(() => {
      store = {};
    }),
  };
})();

Object.defineProperty(window, 'localStorage', { value: localStorageMock });

describe('SplashIntro', () => {
  const onComplete = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    localStorageMock.clear();
  });

  it('renders the first step', () => {
    render(<SplashIntro onComplete={onComplete} />);
    expect(screen.getByText('Welcome to Blend')).toBeInTheDocument();
  });

  it('shows step count indicator', () => {
    render(<SplashIntro onComplete={onComplete} />);
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('advances to next step on Next click', () => {
    render(<SplashIntro onComplete={onComplete} />);
    fireEvent.click(screen.getByText('Next'));
    expect(screen.getByText('Explore & Search')).toBeInTheDocument();
  });

  it('shows Get started on last step', () => {
    render(<SplashIntro onComplete={onComplete} />);
    // Advance to last step
    for (let i = 0; i < 3; i++) {
      fireEvent.click(screen.getByText('Next'));
    }
    expect(screen.getByText('Get started')).toBeInTheDocument();
  });

  it('shows actions after Get started click', () => {
    render(<SplashIntro onComplete={onComplete} />);
    for (let i = 0; i < 3; i++) {
      fireEvent.click(screen.getByText('Next'));
    }
    fireEvent.click(screen.getByText('Get started'));
    expect(screen.getByText('Create an account')).toBeInTheDocument();
    expect(screen.getByText('Log in')).toBeInTheDocument();
    expect(screen.getByText('Continue as guest')).toBeInTheDocument();
  });

  it('calls onComplete with register when Create account is clicked', () => {
    render(<SplashIntro onComplete={onComplete} />);
    for (let i = 0; i < 3; i++) {
      fireEvent.click(screen.getByText('Next'));
    }
    fireEvent.click(screen.getByText('Get started'));
    fireEvent.click(screen.getByText('Create an account'));
    expect(onComplete).toHaveBeenCalledWith('register');
  });

  it('calls onComplete with login when Log in is clicked', () => {
    render(<SplashIntro onComplete={onComplete} />);
    for (let i = 0; i < 3; i++) {
      fireEvent.click(screen.getByText('Next'));
    }
    fireEvent.click(screen.getByText('Get started'));
    fireEvent.click(screen.getByText('Log in'));
    expect(onComplete).toHaveBeenCalledWith('login');
  });

  it('calls onComplete with guest when Continue as guest is clicked', () => {
    render(<SplashIntro onComplete={onComplete} />);
    for (let i = 0; i < 3; i++) {
      fireEvent.click(screen.getByText('Next'));
    }
    fireEvent.click(screen.getByText('Get started'));
    fireEvent.click(screen.getByText('Continue as guest'));
    expect(onComplete).toHaveBeenCalledWith('guest');
  });

  it('skips to actions when Skip is clicked', () => {
    render(<SplashIntro onComplete={onComplete} />);
    fireEvent.click(screen.getByText('Skip'));
    expect(screen.getByText('Create an account')).toBeInTheDocument();
  });
});

describe('useSplashIntro', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorageMock.clear();
  });

  it('shouldShow is true when splash has not been seen', async () => {
    localStorageMock.getItem.mockReturnValueOnce(null as unknown as string);
    const { result } = renderHook(() => useSplashIntro());

    // Wait for useEffect
    await act(async () => {});

    expect(result.current.shouldShow).toBe(true);
  });

  it('shouldShow is false when splash has been seen', async () => {
    localStorageMock.getItem.mockReturnValueOnce('1');
    const { result } = renderHook(() => useSplashIntro());

    await act(async () => {});

    expect(result.current.shouldShow).toBe(false);
  });

  it('markSeen sets localStorage and hides splash', async () => {
    localStorageMock.getItem.mockReturnValueOnce(null as unknown as string);
    const { result } = renderHook(() => useSplashIntro());

    await act(async () => {});
    act(() => result.current.markSeen());

    expect(localStorageMock.setItem).toHaveBeenCalledWith('blend_splash_seen', '1');
    expect(result.current.shouldShow).toBe(false);
  });

  it('showAgain sets shouldShow to true', async () => {
    localStorageMock.getItem.mockReturnValueOnce('1');
    const { result } = renderHook(() => useSplashIntro());

    await act(async () => {});
    act(() => result.current.showAgain());

    expect(result.current.shouldShow).toBe(true);
  });
});
