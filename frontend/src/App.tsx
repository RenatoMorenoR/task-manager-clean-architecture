import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import TasksPage from './pages/TasksPage';
import { LogOut, CheckCircle2 } from 'lucide-react';

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const isAuthenticated = useAuthStore(state => state.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" />;
};

const App: React.FC = () => {
  const { isAuthenticated, logout, user } = useAuthStore();

  return (
    <BrowserRouter>
      <div style={{ minHeight: '100vh' }}>
        {isAuthenticated && (
          <nav className="glass" style={{ margin: '1rem', padding: '0.75rem 2rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between', position: 'sticky', top: '1rem', zIndex: 100 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', fontWeight: 'bold', fontSize: '1.25rem' }}>
              <div style={{ color: 'var(--primary)' }}>
                <CheckCircle2 size={24} />
              </div>
              TaskManager
            </div>
            
            <div style={{ display: 'flex', alignItems: 'center', gap: '2rem' }}>
              <div style={{ fontSize: '0.875rem' }}>
                <span style={{ color: 'var(--text-muted)' }}>Logged in as: </span>
                <span style={{ fontWeight: 600 }}>{user?.name}</span>
              </div>
              <button onClick={logout} className="btn" style={{ background: 'rgba(239, 68, 68, 0.1)', color: 'var(--danger)', padding: '0.5rem 1rem' }}>
                <LogOut size={18} />
                Sign Out
              </button>
            </div>
          </nav>
        )}

        <Routes>
          <Route path="/login" element={!isAuthenticated ? <LoginPage /> : <Navigate to="/" />} />
          <Route path="/register" element={!isAuthenticated ? <RegisterPage /> : <Navigate to="/" />} />
          <Route 
            path="/" 
            element={
              <ProtectedRoute>
                <TasksPage />
              </ProtectedRoute>
            } 
          />
        </Routes>
      </div>
    </BrowserRouter>
  );
};

export default App;
