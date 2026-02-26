'use client';

import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/authStore';
import { authApi } from '@/lib/apiClient';

export function useLogout() {
  const router = useRouter();
  const { clearAuth } = useAuthStore();

  const logout = async () => {
    try {
      await authApi.logout();
    } catch {
      // Continue with client-side logout even if server call fails
    } finally {
      clearAuth();
      router.push('/login');
    }
  };

  return { logout };
}
