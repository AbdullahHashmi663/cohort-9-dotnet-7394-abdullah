import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import API from '../api/axiosInstance';
import { useAuth } from '../context/AuthContext';
import Loader from '../components/Loader';

export default function Profile() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const { logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      const response = await API.get('/user/profile');
      setProfile(response.data);
    } catch (err) {
      setError('Failed to load profile.');
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (loading) {
    return (
      <div className="page">
        <Loader text="Loading profile..." />
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1>Profile</h1>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {profile && (
        <div className="profile-card">
          <div className="profile-avatar">
            {profile.name?.charAt(0)?.toUpperCase() || 'U'}
          </div>

          <div className="profile-info">
            <div className="profile-row">
              <strong>Name</strong>
              <span>{profile.name}</span>
            </div>
            <div className="profile-row">
              <strong>Email</strong>
              <span>{profile.email}</span>
            </div>
            <div className="profile-row">
              <strong>Role</strong>
              <span className="badge badge-info">{profile.role}</span>
            </div>
            <div className="profile-row">
              <strong>Total Tasks</strong>
              <span>{profile.totalTasks}</span>
            </div>
          </div>

          <div className="profile-actions">
            <button onClick={handleLogout} className="btn btn-danger btn-full">
              Logout
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
