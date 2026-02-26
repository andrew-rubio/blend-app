import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { FilterPanel } from '@/components/features/explore/FilterPanel';
import { SearchFilters } from '@/types/search';

const defaultFilters: SearchFilters = {
  cuisines: [],
  diets: [],
  dishTypes: [],
  maxReadyTime: undefined,
};

describe('FilterPanel', () => {
  it('does not render when closed', () => {
    render(
      <FilterPanel
        isOpen={false}
        onClose={vi.fn()}
        filters={defaultFilters}
        onFiltersChange={vi.fn()}
        onClearAll={vi.fn()}
      />,
    );
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('renders when open', () => {
    render(
      <FilterPanel
        isOpen={true}
        onClose={vi.fn()}
        filters={defaultFilters}
        onFiltersChange={vi.fn()}
        onClearAll={vi.fn()}
      />,
    );
    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('Filters')).toBeInTheDocument();
  });

  it('calls onClose when close button is clicked', async () => {
    const onClose = vi.fn();
    render(
      <FilterPanel
        isOpen={true}
        onClose={onClose}
        filters={defaultFilters}
        onFiltersChange={vi.fn()}
        onClearAll={vi.fn()}
      />,
    );
    await userEvent.click(screen.getByLabelText('Close filters'));
    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('calls onFiltersChange when a cuisine is selected', async () => {
    const onFiltersChange = vi.fn();
    render(
      <FilterPanel
        isOpen={true}
        onClose={vi.fn()}
        filters={defaultFilters}
        onFiltersChange={onFiltersChange}
        onClearAll={vi.fn()}
      />,
    );
    await userEvent.click(screen.getByRole('button', { name: 'Italian' }));
    expect(onFiltersChange).toHaveBeenCalledWith({
      ...defaultFilters,
      cuisines: ['Italian'],
    });
  });

  it('calls onClearAll when clear all button is clicked', async () => {
    const onClearAll = vi.fn();
    render(
      <FilterPanel
        isOpen={true}
        onClose={vi.fn()}
        filters={defaultFilters}
        onFiltersChange={vi.fn()}
        onClearAll={onClearAll}
      />,
    );
    await userEvent.click(screen.getByRole('button', { name: 'Clear all filters' }));
    expect(onClearAll).toHaveBeenCalledTimes(1);
  });
});
