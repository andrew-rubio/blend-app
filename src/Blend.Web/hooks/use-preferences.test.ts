import { PREFERENCES_QUERY_KEY } from './use-preferences';

describe('use-preferences', () => {
  it('PREFERENCES_QUERY_KEY is ["preferences"]', () => {
    expect(PREFERENCES_QUERY_KEY).toEqual(['preferences']);
  });
});
