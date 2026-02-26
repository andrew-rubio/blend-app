'use client';
import { useState, useRef, useEffect } from 'react';
import { searchIngredients } from '@/lib/api/preferences';
import type { Ingredient } from '@/types/preferences';

interface IngredientTypeaheadProps {
  selectedIngredients: Ingredient[];
  onAdd: (ingredient: Ingredient) => void;
  onRemove: (id: string) => void;
}

export function IngredientTypeahead({
  selectedIngredients,
  onAdd,
  onRemove,
}: IngredientTypeaheadProps) {
  const [query, setQuery] = useState('');
  const [suggestions, setSuggestions] = useState<Ingredient[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (!query.trim()) {
      setSuggestions([]);
      setShowDropdown(false);
      return;
    }
    debounceRef.current = setTimeout(async () => {
      setIsLoading(true);
      try {
        const results = await searchIngredients(query);
        const filtered = results.filter(
          (r) => !selectedIngredients.some((s) => s.id === r.id)
        );
        setSuggestions(filtered);
        setShowDropdown(filtered.length > 0);
      } catch {
        setSuggestions([]);
      } finally {
        setIsLoading(false);
      }
    }, 300);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [query, selectedIngredients]);

  function handleSelect(ingredient: Ingredient) {
    onAdd(ingredient);
    setQuery('');
    setSuggestions([]);
    setShowDropdown(false);
    inputRef.current?.focus();
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (e.key === 'Escape') {
      setShowDropdown(false);
      setQuery('');
    }
  }

  return (
    <div className="space-y-3">
      <div className="relative">
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Search ingredients..."
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
          aria-label="Search for ingredients to dislike"
          aria-autocomplete="list"
          aria-expanded={showDropdown}
          role="combobox"
          aria-controls="ingredient-suggestions"
        />
        {isLoading && (
          <div className="absolute right-3 top-1/2 -translate-y-1/2">
            <div className="w-4 h-4 border-2 border-primary border-t-transparent rounded-full animate-spin" aria-label="Loading" />
          </div>
        )}
        {showDropdown && (
          <div
            ref={dropdownRef}
            id="ingredient-suggestions"
            role="listbox"
            className="absolute z-10 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg max-h-48 overflow-y-auto"
          >
            {suggestions.map((ingredient) => (
              <button
                key={ingredient.id}
                type="button"
                role="option"
                aria-selected={false}
                onClick={() => handleSelect(ingredient)}
                className="w-full text-left px-4 py-2 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none text-sm"
              >
                {ingredient.name}
              </button>
            ))}
          </div>
        )}
      </div>

      {selectedIngredients.length > 0 && (
        <div className="flex flex-wrap gap-2" aria-label="Disliked ingredients">
          {selectedIngredients.map((ingredient) => (
            <span
              key={ingredient.id}
              className="inline-flex items-center gap-1 px-3 py-1 bg-gray-100 text-gray-700 rounded-full text-sm"
            >
              {ingredient.name}
              <button
                type="button"
                onClick={() => onRemove(ingredient.id)}
                className="ml-1 text-gray-500 hover:text-gray-700 focus:outline-none focus:ring-1 focus:ring-gray-400 rounded-full"
                aria-label={`Remove ${ingredient.name}`}
              >
                ×
              </button>
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
