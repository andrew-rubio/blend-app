import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { SearchResult } from '@/types/search';

// Mock next/navigation
vi.mock('next/navigation', () => ({
  useRouter: () => ({ replace: vi.fn() }),
  useSearchParams: () => new URLSearchParams(),
}));

// Mock the search API
vi.mock('@/lib/api/search', () => ({
  searchRecipes: vi.fn(),
}));

import { searchRecipes } from '@/lib/api/search';
import ExplorePage from '@/app/(main)/explore/page';

const mockResults: SearchResult[] = [
  { id: '1', title: 'Pasta Bolognese', image: '', cuisines: ['Italian'], readyInMinutes: 40, likes: 500, dataSource: 'community' },
];

function renderWithProviders(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
  );
}

describe('ExplorePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders explore heading', () => {
    renderWithProviders(<ExplorePage />);
    expect(screen.getByText('Explore')).toBeInTheDocument();
  });

  it('renders search input', () => {
    renderWithProviders(<ExplorePage />);
    expect(screen.getByRole('searchbox')).toBeInTheDocument();
  });

  it('shows explore view when no search query', () => {
    renderWithProviders(<ExplorePage />);
    expect(screen.getByText('Trending recipes')).toBeInTheDocument();
  });

  it('shows search results when query is entered', async () => {
    vi.mocked(searchRecipes).mockResolvedValue({
      results: mockResults,
      totalResults: 1,
      nextCursor: null,
      quotaExhausted: false,
    });

    renderWithProviders(<ExplorePage />);
    const input = screen.getByRole('searchbox');
    await userEvent.type(input, 'pasta');

    await waitFor(
      () => {
        expect(screen.getByText('Pasta Bolognese')).toBeInTheDocument();
      },
      { timeout: 2000 },
    );
  });

  it('shows filter panel when filter button is clicked', async () => {
    renderWithProviders(<ExplorePage />);
    await userEvent.click(screen.getByLabelText(/Filters/));
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('clears search when clear button is clicked', async () => {
    renderWithProviders(<ExplorePage />);
    const input = screen.getByRole('searchbox');
    await userEvent.type(input, 'pasta');
    const clearBtn = await screen.findByLabelText('Clear search');
    await userEvent.click(clearBtn);
    expect(screen.getByRole('searchbox')).toHaveValue('');
  });
});
