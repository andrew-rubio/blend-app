import { describe, it, expect } from 'vitest'
import { NextRequest } from 'next/server'
import { middleware } from '../../middleware'

// We need to test middleware logic; since Next.js internals are complex,
// we mock NextResponse and NextRequest minimally.

function makeRequest(pathname: string, cookies: Record<string, string> = {}): NextRequest {
  const url = `http://localhost${pathname}`
  const req = new NextRequest(url)
  for (const [name, value] of Object.entries(cookies)) {
    req.cookies.set(name, value)
  }
  return req
}

describe('middleware route protection', () => {
  it('allows unauthenticated access to public routes', () => {
    const req = makeRequest('/home')
    const res = middleware(req)
    expect(res.status).toBe(200)
  })

  it('redirects unauthenticated users from protected routes to login', () => {
    const req = makeRequest('/preferences')
    const res = middleware(req)
    expect(res.status).toBe(307)
    expect(res.headers.get('location')).toContain('/login')
  })

  it('passes the `from` param when redirecting to login', () => {
    const req = makeRequest('/preferences')
    const res = middleware(req)
    expect(res.headers.get('location')).toContain('from=%2Fpreferences')
  })

  it('allows authenticated users to access protected routes', () => {
    const req = makeRequest('/preferences', { refresh_token: 'sometoken' })
    const res = middleware(req)
    expect(res.status).toBe(200)
  })

  it('allows authenticated users to access protected routes with blend_refresh_token cookie', () => {
    const req = makeRequest('/profile', { blend_refresh_token: 'sometoken' })
    const res = middleware(req)
    expect(res.status).toBe(200)
  })

  it('redirects authenticated users away from auth-only routes', () => {
    const req = makeRequest('/login', { refresh_token: 'sometoken' })
    const res = middleware(req)
    expect(res.status).toBe(307)
    expect(res.headers.get('location')).toContain('/home')
  })

  it('does not redirect unauthenticated users from auth routes', () => {
    const req = makeRequest('/login')
    const res = middleware(req)
    expect(res.status).toBe(200)
  })

  it('allows access to register page without session', () => {
    const req = makeRequest('/register')
    const res = middleware(req)
    expect(res.status).toBe(200)
  })

  it('protects /profile route', () => {
    const req = makeRequest('/profile')
    const res = middleware(req)
    expect(res.status).toBe(307)
  })

  it('protects /settings route', () => {
    const req = makeRequest('/settings')
    const res = middleware(req)
    expect(res.status).toBe(307)
  })

  it('protects /cook-mode route', () => {
    const req = makeRequest('/cook-mode')
    const res = middleware(req)
    expect(res.status).toBe(307)
  })
})
