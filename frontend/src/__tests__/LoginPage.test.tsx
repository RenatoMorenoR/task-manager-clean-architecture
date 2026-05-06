import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import LoginPage from '../pages/LoginPage';
import { authApi } from '../api/auth';
import { ApiProvider } from '../context/ApiContext';

// Mock authApi
vi.mock('../api/auth', () => ({
  authApi: {
    login: vi.fn(),
  },
}));

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render login form', () => {
    render(
      <BrowserRouter>
        <ApiProvider>
          <LoginPage />
        </ApiProvider>
      </BrowserRouter>
    );

    expect(screen.getByPlaceholderText(/demo@taskmanager.com/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/••••••••/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Login Now/i })).toBeInTheDocument();
  });

  it('should call login api on submit and navigate on success', async () => {
    const mockResponse = { token: 'token', email: 'test@test.com', name: 'Test', expiresAt: '' };
    (authApi.login as any).mockResolvedValue(mockResponse);

    render(
      <BrowserRouter>
        <ApiProvider>
          <LoginPage />
        </ApiProvider>
      </BrowserRouter>
    );

    fireEvent.change(screen.getByLabelText(/Email Address/i), { target: { value: 'test@test.com' } });
    fireEvent.change(screen.getByLabelText(/Password/i), { target: { value: 'password' } });
    fireEvent.click(screen.getByRole('button', { name: /Login Now/i }));

    await waitFor(() => {
      expect(authApi.login).toHaveBeenCalledWith({ email: 'test@test.com', password: 'password' });
      expect(mockNavigate).toHaveBeenCalledWith('/');
    });
  });

  it('should show error message on login failure', async () => {
    (authApi.login as any).mockRejectedValue({
      response: { data: { title: 'Invalid credentials' } }
    });

    render(
      <BrowserRouter>
        <ApiProvider>
          <LoginPage />
        </ApiProvider>
      </BrowserRouter>
    );

    fireEvent.change(screen.getByLabelText(/Email Address/i), { target: { value: 'test@test.com' } });
    fireEvent.change(screen.getByLabelText(/Password/i), { target: { value: 'wrong' } });
    fireEvent.click(screen.getByRole('button', { name: /Login Now/i }));

    await waitFor(() => {
      expect(screen.getByText(/Invalid credentials/i)).toBeInTheDocument();
    });
  });
});
