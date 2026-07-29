import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import API from '../api/axiosInstance';
import { useAuth } from '../context/AuthContext';
import { CheckSquare, Mail, Lock, LogIn } from 'lucide-react';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await API.post('/auth/login', { email, password });
      const { token, name, role } = response.data;
      login(token, { name, email, role });
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.message || err.response?.data || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-header">
          <div className="auth-brand">
            <div className="brand-icon">
              <CheckSquare size={22} strokeWidth={2.5} />
            </div>
            <h1>TaskManager</h1>
          </div>
          <p>Sign in to your account</p>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="inputBox">
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder=" "
              required
            />
            <span><Mail size={12} /> Email</span>
          </div>

          <div className="inputBox">
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder=" "
              required
            />
            <span><Lock size={12} /> Password</span>
          </div>

          <button type="submit" className="btn btn-primary btn-full" disabled={loading} style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', marginTop: '10px' }}>
            <LogIn size={16} /> {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>

        <p className="auth-footer">
          Don't have an account? <Link to="/signup">Create one</Link>
        </p>
      </div>
    </div>
  );
}
