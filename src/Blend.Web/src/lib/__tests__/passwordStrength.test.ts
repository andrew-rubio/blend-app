import { describe, it, expect } from 'vitest';
import { getPasswordStrength, PASSWORD_REQUIREMENTS } from '../passwordStrength';

describe('getPasswordStrength', () => {
  it('returns score 0 for empty password', () => {
    const result = getPasswordStrength('');
    expect(result.score).toBe(0);
    expect(result.label).toBe('Very weak');
  });

  it('returns score 1 for password with only length >= 8', () => {
    const result = getPasswordStrength('aaaaaaaa');
    expect(result.score).toBe(1);
  });

  it('returns higher score for complex password', () => {
    const result = getPasswordStrength('MyP@ssw0rd!');
    expect(result.score).toBeGreaterThanOrEqual(3);
  });

  it('returns score 4 (max) for a very strong password', () => {
    const result = getPasswordStrength('MyStr0ngP@ssword!');
    expect(result.score).toBe(4);
    expect(result.label).toBe('Very strong');
  });

  it('score never exceeds 4', () => {
    const result = getPasswordStrength('aAbBcC1!23456789012345');
    expect(result.score).toBeLessThanOrEqual(4);
  });
});

describe('PASSWORD_REQUIREMENTS', () => {
  it('has 5 requirements', () => {
    expect(PASSWORD_REQUIREMENTS).toHaveLength(5);
  });

  it('validates minimum length', () => {
    const req = PASSWORD_REQUIREMENTS.find((r) => r.label === 'At least 8 characters')!;
    expect(req.test('1234567')).toBe(false);
    expect(req.test('12345678')).toBe(true);
  });

  it('validates uppercase', () => {
    const req = PASSWORD_REQUIREMENTS.find((r) => r.label === 'Uppercase letter')!;
    expect(req.test('nouppercase')).toBe(false);
    expect(req.test('HasUpper')).toBe(true);
  });

  it('validates number', () => {
    const req = PASSWORD_REQUIREMENTS.find((r) => r.label === 'Number')!;
    expect(req.test('NoNumbers')).toBe(false);
    expect(req.test('Has1Number')).toBe(true);
  });

  it('validates special character', () => {
    const req = PASSWORD_REQUIREMENTS.find((r) => r.label === 'Special character')!;
    expect(req.test('NoSpecial1')).toBe(false);
    expect(req.test('Has@Special')).toBe(true);
  });
});
