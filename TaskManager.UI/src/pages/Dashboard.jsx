import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import API from '../api/axiosInstance';
import { useAuth } from '../context/AuthContext';
import Loader from '../components/Loader';
import { Clock, RefreshCw, CheckCircle2, FolderKanban, Plus, ListTodo } from 'lucide-react';

export default function Dashboard() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const { user } = useAuth();

  useEffect(() => {
    fetchDashboard();
  }, []);

  const fetchDashboard = async () => {
    try {
      const response = await API.get('/tasks/dashboard');
      setStats(response.data);
    } catch (err) {
      setError('Failed to load dashboard data.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="page">
        <Loader text="Loading dashboard..." />
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1>Dashboard</h1>
        <p>Welcome back, <strong>{user?.name}</strong>! {user?.role === 'Admin' ? '(Viewing all tasks)' : ''}</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {stats && (
        <div className="stats-grid">
          <div className="stat-card stat-pending">
            <div className="stat-icon" style={{ color: '#e07a5f' }}>
              <Clock size={32} />
            </div>
            <div className="stat-info">
              <h3>{stats.pendingCount}</h3>
              <p>Pending</p>
            </div>
          </div>

          <div className="stat-card stat-inprogress">
            <div className="stat-icon" style={{ color: '#38bdf8' }}>
              <RefreshCw size={32} />
            </div>
            <div className="stat-info">
              <h3>{stats.inProgressCount}</h3>
              <p>In Progress</p>
            </div>
          </div>

          <div className="stat-card stat-completed">
            <div className="stat-icon" style={{ color: '#10b981' }}>
              <CheckCircle2 size={32} />
            </div>
            <div className="stat-info">
              <h3>{stats.completedCount}</h3>
              <p>Completed</p>
            </div>
          </div>

          <div className="stat-card stat-total">
            <div className="stat-icon" style={{ color: '#3c6e71' }}>
              <FolderKanban size={32} />
            </div>
            <div className="stat-info">
              <h3>{stats.totalCount}</h3>
              <p>Total Tasks</p>
            </div>
          </div>
        </div>
      )}

      <div className="quick-actions">
        <h2>Quick Actions</h2>
        <div className="action-buttons">
          <Link to="/tasks/new" className="btn btn-primary">
            <Plus size={18} /> Create New Task
          </Link>
          <Link to="/tasks" className="btn btn-secondary">
            <ListTodo size={18} /> View All Tasks
          </Link>
        </div>
      </div>
    </div>
  );
}
