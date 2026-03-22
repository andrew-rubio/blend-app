import { describe, it, expect } from 'vitest'
import {
  checkPasswordRequirements,
  getPasswordStrength,
  isPasswordValid,
  strengthLabels,
} from '@/lib/passwordStrength'

describe('checkPasswordRequirements', () => {
  it('returns all false for empty string', () => {
    const reqs = checkPasswordRequirements('')
    expect(reqs.minLength).toBe(false)
    expect(reqs.hasUppercase).toBe(false)
    expect(reqs.hasLowercase).toBe(false)
    expect(reqs.hasNumber).toBe(false)
    expect(reqs.hasSpecial).toBe(false)
  })

  it('detects minimum length', () => {
    expect(checkPasswordRequirements('abcdefg').minLength).toBe(false)
    expect(checkPasswordRequirements('abcdefgh').minLength).toBe(true)
  })

  it('detects uppercase', () => {
    expect(checkPasswordRequirements('abcdefgh').hasUppercase).toBe(false)
    expect(checkPasswordRequirements('Abcdefgh').hasUppercase).toBe(true)
  })

  it('detects lowercase', () => {
    expect(checkPasswordRequirements('ABCDEFGH').hasLowercase).toBe(false)
    expect(checkPasswordRequirements('ABCDEFGh').hasLowercase).toBe(true)
  })

  it('detects number', () => {
    expect(checkPasswordRequirements('abcdefgh').hasNumber).toBe(false)
    expect(checkPasswordRequirements('abcdefg1').hasNumber).toBe(true)
  })

  it('detects special character', () => {
    expect(checkPasswordRequirements('abcdefgh').hasSpecial).toBe(false)
    expect(checkPasswordRequirements('abcdefg!').hasSpecial).toBe(true)
  })
})

describe('getPasswordStrength', () => {
  it('returns too-short for passwords under 8 chars', () => {
    expect(getPasswordStrength('Abc1!')).toBe('too-short')
    expect(getPasswordStrength('')).toBe('too-short')
  })

  it('returns weak for password with only 1 requirement met', () => {
    expect(getPasswordStrength('abcdefgh')).toBe('weak') // only lowercase
  })

  it('returns fair for password with 2 requirements met', () => {
    expect(getPasswordStrength('Abcdefgh')).toBe('fair') // upper + lower
  })

  it('returns strong for password with 3 requirements met', () => {
    expect(getPasswordStrength('Abcdefg1')).toBe('strong') // upper + lower + number
  })

  it('returns strong for password with 4 requirements but short', () => {
    expect(getPasswordStrength('Abc1!xyz')).toBe('strong') // all 4 but < 12 chars
  })

  it('returns very-strong for 12+ char password with all requirements', () => {
    expect(getPasswordStrength('Abcdefgh1!xy')).toBe('very-strong')
  })
})

describe('isPasswordValid', () => {
  it('returns false for too-short password', () => {
    expect(isPasswordValid('Abc1!')).toBe(false)
  })

  it('returns false for password missing uppercase', () => {
    expect(isPasswordValid('abcdefgh1!')).toBe(false)
  })

  it('returns false for password missing number', () => {
    expect(isPasswordValid('Abcdefgh!')).toBe(false)
  })

  it('returns false for password missing special character', () => {
    expect(isPasswordValid('Abcdefgh1')).toBe(false)
  })

  it('returns true for a valid strong password', () => {
    expect(isPasswordValid('Password1!')).toBe(true)
  })
})

describe('strengthLabels', () => {
  it('has labels for all strength levels', () => {
    expect(strengthLabels['too-short']).toBeDefined()
    expect(strengthLabels['weak']).toBeDefined()
    expect(strengthLabels['fair']).toBeDefined()
    expect(strengthLabels['strong']).toBeDefined()
    expect(strengthLabels['very-strong']).toBeDefined()
  })
})
