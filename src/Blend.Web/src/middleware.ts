import { NextResponse } from 'next/server'
import type { NextRequest } from 'next/server'

/**
 * Routes that require an authenticated user.
 * Guest users are redirected to /login with a `from` parameter.
 */
const PROTECTED_ROUTES = ['/preferences', '/profile', '/settings', '/cook-mode']

/**
 * Routes only accessible to unauthenticated users (e.g. login, register).
 * Authenticated users are redirected to /home.
 */
const AUTH_ONLY_ROUTES = ['/login', '/register', '/forgot-password', '/reset-password']

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl

  const isProtectedRoute = PROTECTED_ROUTES.some((route) => pathname.startsWith(route))
  const isAuthOnlyRoute = AUTH_ONLY_ROUTES.some((route) => pathname.startsWith(route))

  // The refresh token is stored in an HTTP-only cookie by the backend.
  // Its presence indicates the user has (or recently had) an active session.
  const hasSession =
    request.cookies.has('refresh_token') || request.cookies.has('blend_refresh_token')

  if (isProtectedRoute && !hasSession) {
    const loginUrl = new URL('/login', request.url)
    loginUrl.searchParams.set('from', pathname)
    return NextResponse.redirect(loginUrl)
  }

  if (isAuthOnlyRoute && hasSession) {
    return NextResponse.redirect(new URL('/home', request.url))
  }

  return NextResponse.next()
}

export const config = {
  matcher: [
    /*
     * Match all request paths except for:
     * - _next/static (static files)
     * - _next/image (image optimisation)
     * - favicon.ico
     * - api routes (handled separately)
     */
    '/((?!_next/static|_next/image|favicon.ico|api/).*)',
  ],
}
