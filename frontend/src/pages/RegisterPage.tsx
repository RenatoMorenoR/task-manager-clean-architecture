import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { useApi } from '../context/ApiContext';
import { UserPlus } from 'lucide-react';

const RegisterPage: React.FC = () => {
  const { auth: authApi } = useApi();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  
  const setAuth = useAuthStore(state => state.setAuth);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    if (password.length < 8) {
      setError('Password must be at least 8 characters');
      return;
    }

    setLoading(true);

    try {
      const response = await authApi.register({ name, email, password });
      setAuth(response);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.title || 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container" style={{ minHeight: '80vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <div className="glass animate-fade-in" style={{ padding: '3rem', width: '100%', maxWidth: '450px' }}>
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <div className="glass" style={{ width: '64px', height: '64px', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 1rem', color: 'var(--primary)' }}>
            <UserPlus size={32} />
          </div>
          <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold' }}>Create Account</h1>
          <p style={{ color: 'var(--text-muted)', marginTop: '0.5rem' }}>Start organizing your daily work</p>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.875rem' }}>Full Name</label>
            <input 
              type="text" 
              className="input" 
              value={name} 
              onChange={e => setName(e.target.value)}
              placeholder="John Doe"
              required 
            />
          </div>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.875rem' }}>Email Address</label>
            <input 
              type="email" 
              className="input" 
              value={email} 
              onChange={e => setEmail(e.target.value)}
              placeholder="john@example.com"
              required 
            />
          </div>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.875rem' }}>Password</label>
            <input 
              type="password" 
              className="input" 
              value={password} 
              onChange={e => setPassword(e.target.value)}
              placeholder="Min. 8 characters"
              required 
            />
          </div>

          {error && <div style={{ color: 'var(--danger)', fontSize: '0.875rem', textAlign: 'center' }}>{error}</div>}

          <button type="submit" className="btn btn-primary" style={{ justifyContent: 'center', marginTop: '0.5rem' }} disabled={loading}>
            {loading ? 'Creating Account...' : 'Get Started'}
          </button>
        </form>

        <p style={{ textAlign: 'center', marginTop: '2rem', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
          Already have an account? <Link to="/login" style={{ color: 'var(--primary)', textDecoration: 'none' }}>Log in</Link>
        </p>
      </div>
    </div>
  );
};

export default RegisterPage;
