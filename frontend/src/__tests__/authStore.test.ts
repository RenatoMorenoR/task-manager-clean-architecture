import { describe, it, expect, beforeEach } from 'vitest';
import { useAuthStore } from '../store/authStore';

describe('authStore', () => {
  beforeEach(() => {
    useAuthStore.getState().logout();
  });

  it('should start with unauthenticated state', () => {
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
    expect(state.token).toBeNull();
  });

  it('should set auth data correctly', () => {
    const authData = {
      token: 'test-token',
      email: 'test@example.com',
      name: 'Test User',
      expiresAt: '2026-01-01'
    };

    useAuthStore.getState().setAuth(authData);

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(true);
    expect(state.user?.email).toBe(authData.email);
    expect(state.token).toBe(authData.token);
  });

  it('should clear state on logout', () => {
    useAuthStore.getState().setAuth({
      token: 'token',
      email: 'email',
      name: 'name',
      expiresAt: 'date'
    });

    useAuthStore.getState().logout();

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
  });
});
