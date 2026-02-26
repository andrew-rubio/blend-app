'use client';

import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/Button';

const SPLASH_SEEN_KEY = 'blend_splash_seen';

const STEPS = [
  {
    title: 'Welcome to Blend',
    description:
      'Your personal cooking companion. Discover recipes tailored to your taste and dietary needs.',
    emoji: '🍳',
  },
  {
    title: 'Explore & Search',
    description:
      'Browse thousands of recipes, search by ingredient, cuisine, or dietary preference.',
    emoji: '🔍',
  },
  {
    title: 'Cook Mode',
    description:
      'Step-by-step cooking guidance with hands-free voice control — no more sticky screens.',
    emoji: '👨‍🍳',
  },
  {
    title: 'Connect & Share',
    description:
      'Follow friends, share your creations, and discover what your community is cooking.',
    emoji: '❤️',
  },
];

interface SplashIntroProps {
  onComplete: (action: 'register' | 'login' | 'guest') => void;
}

export function SplashIntro({ onComplete }: SplashIntroProps) {
  const [step, setStep] = useState(0);
  const [showActions, setShowActions] = useState(false);

  const isLast = step === STEPS.length - 1;

  const handleNext = () => {
    if (isLast) {
      setShowActions(true);
    } else {
      setStep((s) => s + 1);
    }
  };

  const handleDismiss = () => {
    setShowActions(true);
  };

  if (showActions) {
    return (
      <div
        className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4"
        role="dialog"
        aria-modal="true"
        aria-label="Get started with Blend"
      >
        <div className="w-full max-w-sm rounded-2xl bg-white p-8 text-center shadow-2xl">
          <div className="mb-6 text-5xl">🥗</div>
          <h2 className="mb-2 text-2xl font-bold text-gray-900">Get started</h2>
          <p className="mb-8 text-gray-500">Join Blend or continue as a guest</p>
          <div className="flex flex-col gap-3">
            <Button onClick={() => onComplete('register')} size="lg" className="w-full">
              Create an account
            </Button>
            <Button
              onClick={() => onComplete('login')}
              variant="outline"
              size="lg"
              className="w-full"
            >
              Log in
            </Button>
            <Button
              onClick={() => onComplete('guest')}
              variant="ghost"
              size="lg"
              className="w-full text-gray-500"
            >
              Continue as guest
            </Button>
          </div>
        </div>
      </div>
    );
  }

  const current = STEPS[step];

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4"
      role="dialog"
      aria-modal="true"
      aria-label={`Splash intro step ${step + 1} of ${STEPS.length}`}
    >
      <div className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-2xl">
        <div className="mb-6 text-center text-6xl">{current.emoji}</div>
        <h2 className="mb-3 text-center text-2xl font-bold text-gray-900">{current.title}</h2>
        <p className="mb-8 text-center text-gray-500">{current.description}</p>

        <div className="mb-6 flex justify-center gap-2">
          {STEPS.map((_, i) => (
            <div
              key={i}
              className={`h-2 rounded-full transition-all ${
                i === step ? 'w-6 bg-green-600' : 'w-2 bg-gray-200'
              }`}
              aria-hidden="true"
            />
          ))}
        </div>

        <div className="flex items-center justify-between">
          <button
            onClick={handleDismiss}
            className="text-sm text-gray-400 hover:text-gray-600"
            aria-label="Skip intro"
          >
            Skip
          </button>
          <Button onClick={handleNext}>{isLast ? 'Get started' : 'Next'}</Button>
        </div>
      </div>
    </div>
  );
}

export function useSplashIntro() {
  const [shouldShow, setShouldShow] = useState(false);

  useEffect(() => {
    const seen = localStorage.getItem(SPLASH_SEEN_KEY);
    setShouldShow(!seen);
  }, []);

  const markSeen = () => {
    localStorage.setItem(SPLASH_SEEN_KEY, '1');
    setShouldShow(false);
  };

  const showAgain = () => {
    setShouldShow(true);
  };

  return { shouldShow, markSeen, showAgain };
}
