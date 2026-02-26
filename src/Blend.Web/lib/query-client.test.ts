import { describe, it, expect } from 'vitest';
import { createQueryClient } from '@/lib/query-client';

describe('createQueryClient', () => {
  it('creates a QueryClient with default options', () => {
    const client = createQueryClient();
    const options = client.getDefaultOptions();
    expect(options.queries?.staleTime).toBe(60 * 1000);
    expect(options.queries?.retry).toBe(1);
  });
});
