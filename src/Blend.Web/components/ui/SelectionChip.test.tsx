import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { SelectionChip } from './SelectionChip';

describe('SelectionChip', () => {
  it('renders the label', () => {
    render(<SelectionChip label="Italian" selected={false} onToggle={() => {}} />);
    expect(screen.getByText('Italian')).toBeInTheDocument();
  });

  it('has aria-pressed=false when unselected', () => {
    render(<SelectionChip label="Italian" selected={false} onToggle={() => {}} />);
    expect(screen.getByRole('button')).toHaveAttribute('aria-pressed', 'false');
  });

  it('has aria-pressed=true when selected', () => {
    render(<SelectionChip label="Italian" selected={true} onToggle={() => {}} />);
    expect(screen.getByRole('button')).toHaveAttribute('aria-pressed', 'true');
  });

  it('calls onToggle when clicked', async () => {
    const onToggle = vi.fn();
    render(<SelectionChip label="Italian" selected={false} onToggle={onToggle} />);
    await userEvent.click(screen.getByRole('button'));
    expect(onToggle).toHaveBeenCalledTimes(1);
  });
});
