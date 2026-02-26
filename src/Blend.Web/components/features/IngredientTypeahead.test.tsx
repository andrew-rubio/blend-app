import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { IngredientTypeahead } from './IngredientTypeahead';
import type { Ingredient } from '@/types/preferences';

const selectedIngredients: Ingredient[] = [
  { id: '1', name: 'Garlic' },
  { id: '2', name: 'Onion' },
];

describe('IngredientTypeahead', () => {
  it('renders the search input', () => {
    render(
      <IngredientTypeahead
        selectedIngredients={[]}
        onAdd={() => {}}
        onRemove={() => {}}
      />
    );
    expect(screen.getByRole('combobox')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Search ingredients...')).toBeInTheDocument();
  });

  it('shows selected ingredients as chips', () => {
    render(
      <IngredientTypeahead
        selectedIngredients={selectedIngredients}
        onAdd={() => {}}
        onRemove={() => {}}
      />
    );
    expect(screen.getByText('Garlic')).toBeInTheDocument();
    expect(screen.getByText('Onion')).toBeInTheDocument();
  });

  it('calls onRemove when remove button is clicked', async () => {
    const onRemove = vi.fn();
    render(
      <IngredientTypeahead
        selectedIngredients={selectedIngredients}
        onAdd={() => {}}
        onRemove={onRemove}
      />
    );
    await userEvent.click(screen.getByLabelText('Remove Garlic'));
    expect(onRemove).toHaveBeenCalledWith('1');
  });

  it('renders the disliked ingredients label when items are selected', () => {
    render(
      <IngredientTypeahead
        selectedIngredients={selectedIngredients}
        onAdd={() => {}}
        onRemove={() => {}}
      />
    );
    expect(screen.getByLabelText('Disliked ingredients')).toBeInTheDocument();
  });
});
