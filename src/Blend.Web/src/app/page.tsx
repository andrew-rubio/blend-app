'use client';

import { useRouter } from 'next/navigation';
import { SplashIntro, useSplashIntro } from '@/components/features/SplashIntro';

export default function HomePage() {
  const router = useRouter();
  const { shouldShow, markSeen } = useSplashIntro();

  const handleSplashComplete = (action: 'register' | 'login' | 'guest') => {
    markSeen();
    if (action === 'register') {
      router.push('/register');
    } else if (action === 'login') {
      router.push('/login');
    }
    // guest: stay on home page
  };

  return (
    <>
      {shouldShow && <SplashIntro onComplete={handleSplashComplete} />}
      <main className="flex min-h-screen flex-col items-center justify-center p-8">
        <h1 className="text-4xl font-bold text-green-600">Blend</h1>
        <p className="mt-2 text-gray-500">Your personal cooking companion</p>
      </main>
    </>
  );
}
