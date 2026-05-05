import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { useApi } from '../context/ApiContext';
import { LogIn } from 'lucide-react';

const LoginPage: React.FC = () => {
  const { auth: authApi } = useApi();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  
  const setAuth = useAuthStore(state => state.setAuth);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authApi.login({ email, password });
      setAuth(response);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.title || 'Invalid credentials');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container" style={{ minHeight: '80vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <div className="glass animate-fade-in" style={{ padding: '3rem', width: '100%', maxWidth: '450px' }}>
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <div className="glass" style={{ width: '64px', height: '64px', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 1rem', color: 'var(--primary)' }}>
            <LogIn size={32} />
          </div>
          <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold' }}>Welcome Back</h1>
          <p style={{ color: 'var(--text-muted)', marginTop: '0.5rem' }}>Login to manage your tasks</p>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          <div>
            <label htmlFor="email" style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.875rem' }}>Email Address</label>
            <input 
              id="email"
              type="email" 
              className="input" 
              value={email} 
              onChange={e => setEmail(e.target.value)}
              placeholder="demo@taskmanager.com"
              required 
            />
          </div>
          <div>
            <label htmlFor="password" style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.875rem' }}>Password</label>
            <input 
              id="password"
              type="password" 
              className="input" 
              value={password} 
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              required 
            />
          </div>

          {error && <div style={{ color: 'var(--danger)', fontSize: '0.875rem', textAlign: 'center' }}>{error}</div>}

          <button type="submit" className="btn btn-primary" style={{ justifyContent: 'center' }} disabled={loading}>
            {loading ? 'Logging in...' : 'Login Now'}
          </button>
        </form>

        <p style={{ textAlign: 'center', marginTop: '2rem', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
          Don't have an account? <Link to="/register" style={{ color: 'var(--primary)', textDecoration: 'none' }}>Sign up</Link>
        </p>
      </div>
    </div>
  );
};

export default LoginPage;
