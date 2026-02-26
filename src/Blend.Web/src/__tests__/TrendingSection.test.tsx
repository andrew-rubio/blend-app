import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { TrendingSection } from '@/components/features/explore/TrendingSection';
import { SearchResult } from '@/types/search';

const mockRecipes: SearchResult[] = [
  { id: '1', title: 'Pasta', image: '', cuisines: ['Italian'], readyInMinutes: 20, likes: 100, dataSource: 'community' },
  { id: '2', title: 'Tacos', image: '', cuisines: ['Mexican'], readyInMinutes: 15, likes: 80, dataSource: 'spoonacular' },
];

describe('TrendingSection', () => {
  it('renders heading', () => {
    render(<TrendingSection />);
    expect(screen.getByText('Trending recipes')).toBeInTheDocument();
  });

  it('renders skeleton cards when loading', () => {
    const { container } = render(<TrendingSection isLoading={true} />);
    expect(container.querySelectorAll('.animate-pulse')).toHaveLength(5);
  });

  it('renders recipe cards when data is provided', () => {
    render(<TrendingSection recipes={mockRecipes} />);
    expect(screen.getByText('Pasta')).toBeInTheDocument();
    expect(screen.getByText('Tacos')).toBeInTheDocument();
  });
});
