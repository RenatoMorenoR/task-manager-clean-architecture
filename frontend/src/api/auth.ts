import client from './client';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types';

export const authApi = {
  login: async (data: LoginRequest) => {
    const response = await client.post<AuthResponse>('/api/auth/login', data);
    return response.data;
  },
  register: async (data: RegisterRequest) => {
    const response = await client.post<AuthResponse>('/api/auth/register', data);
    return response.data;
  },
};
