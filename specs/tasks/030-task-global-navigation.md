# Task 030: Global Navigation and Responsive Layout

> **GitHub Issue:** [#41](https://github.com/andrew-rubio/blend-app/issues/41)

## Description

Implement the persistent global navigation bar and responsive layout shell per the Platform FRD (PLAT-42 through PLAT-46, REQ-62). This is the application-wide layout component that wraps all pages.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project with the App Router layout system
- **007-task-auth-frontend** — requires auth state for login/logout navigation
- **025-task-friends-notifications-frontend** — requires the notification bell component

## Technical Requirements

### Navigation bar (PLAT-42 through PLAT-46)

- Persistent bottom navigation on mobile (5 items):
  1. **Home** — `/home` (house icon)
  2. **Explore** — `/explore` (search icon)
  3. **Cook** — `/cook` (flame icon, central action button)
  4. **Friends** — `/friends` (people icon)
  5. **Profile** — `/profile` (person icon)
- Persistent top navigation on desktop (horizontal header):
  - App logo (left) linking to home
  - Navigation links: Home, Explore, Cook, Friends, Profile
  - Notification bell with unread badge (right side)
  - User avatar dropdown with: View Profile, Settings, Logout

### Active state indicators (PLAT-43)

- The current section's icon/link is visually highlighted
- Active state determined by the current route prefix
- Smooth transition between active states

### Notification bell integration

- Desktop: notification bell in the top navigation header
- Mobile: notification bell accessible from a top app bar or badge overlay on the Friends tab
- Uses the notification bell component from task 025

### Layout shell

- Next.js root layout wraps all authenticated pages with the navigation
- Auth pages (login, register) use a separate layout without the navigation bar
- Layout components:
  - `AuthLayout` — splash/login/register pages (no nav bar)
  - `MainLayout` — all authenticated pages with navigation
  - `AdminLayout` — admin pages with admin-specific navigation

### Guest access (PLAT-09 through PLAT-11)

- Guest users see the navigation bar but certain actions show login prompt modals
- Cook Mode, Friends, and Profile tabs require authentication
- Tapping a restricted tab shows a modal: "Sign in to access this feature" with login and register buttons

### Responsive behaviour

- Mobile: bottom navigation bar, content area fills remaining viewport height
- Tablet: bottom or side navigation depending on orientation
- Desktop: top navigation header, content area centred with max-width container
- Navigation transitions use CSS animations for tab switching
- Mobile safe area insets for notch/home indicator clearance

### Scroll behaviour

- Mobile: bottom nav bar hides on scroll down, shows on scroll up (optional, configurable)
- Desktop: top nav remains sticky at the top
- Scroll restoration between page navigations

## Acceptance Criteria

- [ ] Bottom navigation appears on mobile with 5 tabs
- [ ] Top navigation appears on desktop with logo, links, bell, and avatar dropdown
- [ ] Current section is correctly highlighted in the navigation
- [ ] Notification bell shows unread count badge
- [ ] Auth pages display without navigation
- [ ] Guest users see login prompt modals for restricted tabs
- [ ] Layout is responsive across mobile, tablet, and desktop breakpoints
- [ ] Navigation transitions are smooth and performant
- [ ] Scroll restoration works between page navigations
- [ ] Admin layout has separate navigation for admin users

## Testing Requirements

- Component tests for bottom navigation (render, active state, tap navigation)
- Component tests for top navigation (render, active state, dropdown)
- Component test for notification bell integration and badge display
- Component test for guest access modal trigger
- Component test for responsive behaviour (breakpoint switching)
- Integration test for navigation between all main sections
- Accessibility tests (keyboard tab navigation, ARIA roles, skip links)
- Visual regression tests for mobile and desktop layouts
- Minimum 85% code coverage
