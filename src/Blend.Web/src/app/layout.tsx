import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Blend — Your Cooking Companion',
  description: 'Discover, cook, and share recipes tailored to your taste',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="antialiased">{children}</body>
    </html>
  );
}
