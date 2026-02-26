import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { SearchInput } from '@/components/features/explore/SearchInput';

describe('SearchInput', () => {
  it('renders search input', () => {
    render(
      <SearchInput
        value=""
        onChange={vi.fn()}
        onClear={vi.fn()}
        activeFilterCount={0}
        onFilterClick={vi.fn()}
      />,
    );
    expect(screen.getByRole('searchbox')).toBeInTheDocument();
  });

  it('shows clear button when value is present', () => {
    render(
      <SearchInput
        value="pasta"
        onChange={vi.fn()}
        onClear={vi.fn()}
        activeFilterCount={0}
        onFilterClick={vi.fn()}
      />,
    );
    expect(screen.getByLabelText('Clear search')).toBeInTheDocument();
  });

  it('does not show clear button when value is empty', () => {
    render(
      <SearchInput
        value=""
        onChange={vi.fn()}
        onClear={vi.fn()}
        activeFilterCount={0}
        onFilterClick={vi.fn()}
      />,
    );
    expect(screen.queryByLabelText('Clear search')).not.toBeInTheDocument();
  });

  it('calls onClear when clear button is clicked', async () => {
    const onClear = vi.fn();
    render(
      <SearchInput
        value="pasta"
        onChange={vi.fn()}
        onClear={onClear}
        activeFilterCount={0}
        onFilterClick={vi.fn()}
      />,
    );
    await userEvent.click(screen.getByLabelText('Clear search'));
    expect(onClear).toHaveBeenCalledTimes(1);
  });

  it('shows active filter count badge', () => {
    render(
      <SearchInput
        value=""
        onChange={vi.fn()}
        onClear={vi.fn()}
        activeFilterCount={3}
        onFilterClick={vi.fn()}
      />,
    );
    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('calls onFilterClick when filter button is clicked', async () => {
    const onFilterClick = vi.fn();
    render(
      <SearchInput
        value=""
        onChange={vi.fn()}
        onClear={vi.fn()}
        activeFilterCount={0}
        onFilterClick={onFilterClick}
      />,
    );
    await userEvent.click(screen.getByLabelText(/Filters/));
    expect(onFilterClick).toHaveBeenCalledTimes(1);
  });

  it('calls onChange when typing', async () => {
    const onChange = vi.fn();
    render(
      <SearchInput
        value=""
        onChange={onChange}
        onClear={vi.fn()}
        activeFilterCount={0}
        onFilterClick={vi.fn()}
      />,
    );
    await userEvent.type(screen.getByRole('searchbox'), 'pasta');
    expect(onChange).toHaveBeenCalled();
  });
});
