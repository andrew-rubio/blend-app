import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { SearchResultCard } from '@/components/features/explore/SearchResultCard';
import { SearchResult } from '@/types/search';

const mockRecipe: SearchResult = {
  id: '1',
  title: 'Spaghetti Carbonara',
  image: '',
  cuisines: ['Italian'],
  readyInMinutes: 30,
  likes: 1240,
  dataSource: 'community',
};

describe('SearchResultCard', () => {
  it('renders recipe title', () => {
    render(<SearchResultCard recipe={mockRecipe} />);
    expect(screen.getByText('Spaghetti Carbonara')).toBeInTheDocument();
  });

  it('renders cuisine tag', () => {
    render(<SearchResultCard recipe={mockRecipe} />);
    expect(screen.getByText('Italian')).toBeInTheDocument();
  });

  it('renders prep time', () => {
    render(<SearchResultCard recipe={mockRecipe} />);
    expect(screen.getByText(/30 min/)).toBeInTheDocument();
  });

  it('renders likes count', () => {
    render(<SearchResultCard recipe={mockRecipe} />);
    expect(screen.getByText(/1240/)).toBeInTheDocument();
  });

  it('renders community badge for community source', () => {
    render(<SearchResultCard recipe={mockRecipe} />);
    expect(screen.getByText('Community')).toBeInTheDocument();
  });

  it('renders spoonacular badge for spoonacular source', () => {
    render(<SearchResultCard recipe={{ ...mockRecipe, dataSource: 'spoonacular' }} />);
    expect(screen.getByText('Spoonacular')).toBeInTheDocument();
  });

  it('links to the recipe page', () => {
    render(<SearchResultCard recipe={mockRecipe} />);
    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', '/recipes/1');
  });
});
