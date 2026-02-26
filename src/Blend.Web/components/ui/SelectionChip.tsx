'use client';

interface SelectionChipProps {
  label: string;
  selected: boolean;
  onToggle: () => void;
}

export function SelectionChip({ label, selected, onToggle }: SelectionChipProps) {
  return (
    <button
      type="button"
      onClick={onToggle}
      aria-pressed={selected}
      className={`px-4 py-2 rounded-full border text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary ${
        selected
          ? 'bg-primary text-white border-primary'
          : 'bg-white text-gray-700 border-gray-300 hover:border-primary'
      }`}
    >
      {label}
    </button>
  );
}
