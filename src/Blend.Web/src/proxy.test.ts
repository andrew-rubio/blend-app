import { describe, it, expect } from 'vitest';
import { proxy } from './proxy';
import { NextRequest } from 'next/server';

function makeRequest(path: string, hasCookie = false): NextRequest {
  const url = `http://localhost:3000${path}`;
  const req = new NextRequest(url);
  if (hasCookie) {
    req.cookies.set('refreshToken', 'test-refresh-token');
  }
  return req;
}

describe('proxy (route protection)', () => {
  it('allows unauthenticated access to public paths', () => {
    const response = proxy(makeRequest('/'));
    expect(response.status).toBe(200);
  });

  it('redirects unauthenticated user from protected route to login', () => {
    const response = proxy(makeRequest('/preferences', false));
    expect(response.status).toBe(307);
    expect(response.headers.get('location')).toContain('/login');
  });

  it('includes redirect param when protecting a route', () => {
    const response = proxy(makeRequest('/preferences', false));
    const location = response.headers.get('location') ?? '';
    expect(location).toContain('redirect=%2Fpreferences');
  });

  it('redirects authenticated user from login to home', () => {
    const response = proxy(makeRequest('/login', true));
    expect(response.status).toBe(307);
    expect(response.headers.get('location')).toContain('/');
  });

  it('allows authenticated user to access protected routes', () => {
    const response = proxy(makeRequest('/preferences', true));
    expect(response.status).toBe(200);
  });

  it('allows unauthenticated user to access login page', () => {
    const response = proxy(makeRequest('/login', false));
    expect(response.status).toBe(200);
  });

  it('protects /profile route', () => {
    const response = proxy(makeRequest('/profile', false));
    expect(response.status).toBe(307);
  });

  it('protects /settings route', () => {
    const response = proxy(makeRequest('/settings', false));
    expect(response.status).toBe(307);
  });
});
