import { useState, useEffect } from 'react';
import API from '../api/axiosInstance';
import Loader from '../components/Loader';
import { useAuth } from '../context/AuthContext';
import { Users, UserPlus, Search, Edit3, Trash2, ShieldCheck, User as UserIcon, X, Check } from 'lucide-react';

export default function UserManagement() {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState([]);
  const [filteredUsers, setFilteredUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  // Modal / Form state
  const [showModal, setShowModal] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    password: '',
    role: 'User'
  });
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    fetchUsers();
  }, []);

  useEffect(() => {
    if (searchTerm) {
      setFilteredUsers(
        users.filter(u =>
          u.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
          u.email.toLowerCase().includes(searchTerm.toLowerCase())
        )
      );
    } else {
      setFilteredUsers(users);
    }
  }, [users, searchTerm]);

  const fetchUsers = async () => {
    try {
      const response = await API.get('/user/all');
      setUsers(response.data);
    } catch (err) {
      setError('Failed to fetch user list.');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenAddModal = () => {
    setEditingUser(null);
    setFormData({ name: '', email: '', password: '', role: 'User' });
    setError('');
    setShowModal(true);
  };

  const handleOpenEditModal = (userToEdit) => {
    setEditingUser(userToEdit);
    setFormData({
      name: userToEdit.name,
      email: userToEdit.email,
      password: '', // Blank unless updating password
      role: userToEdit.role
    });
    setError('');
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingUser(null);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);

    try {
      if (editingUser) {
        await API.put(`/user/${editingUser.id}`, formData);
        setSuccessMsg(`User '${formData.name}' updated successfully!`);
      } else {
        await API.post('/user', formData);
        setSuccessMsg(`User '${formData.name}' created successfully!`);
      }

      setTimeout(() => setSuccessMsg(''), 3000);
      handleCloseModal();
      fetchUsers();
    } catch (err) {
      setError(err.response?.data?.message || err.response?.data || 'Operation failed.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteUser = async (userId, userName) => {
    if (userId === currentUser?.id) {
      alert('You cannot delete your own admin account.');
      return;
    }

    if (!window.confirm(`Are you sure you want to delete user '${userName}'?`)) return;

    try {
      await API.delete(`/user/${userId}`);
      setSuccessMsg(`User '${userName}' deleted successfully.`);
      setTimeout(() => setSuccessMsg(''), 3000);
      fetchUsers();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete user.');
    }
  };

  if (loading) {
    return (
      <div className="page">
        <Loader text="Loading user directory..." />
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>User Management</h1>
          <p style={{ color: 'var(--text-secondary)', fontSize: '14px', marginTop: '4px' }}>
            Manage application users, roles, and administrative credentials.
          </p>
        </div>
        <button onClick={handleOpenAddModal} className="btn btn-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
          <UserPlus size={16} /> Add New User
        </button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {successMsg && <div className="alert alert-success">{successMsg}</div>}

      <div className="filters" style={{ marginBottom: '24px' }}>
        <div style={{ position: 'relative', flex: 1 }}>
          <Search size={16} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
          <input
            type="text"
            className="search-input"
            style={{ paddingLeft: '36px', width: '100%' }}
            placeholder="Search users by name or email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <div className="form-card" style={{ padding: 0, overflow: 'hidden' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
          <thead>
            <tr style={{ background: 'var(--bg-input)', borderBottom: '1px solid var(--border)' }}>
              <th style={{ padding: '16px 20px', fontSize: '13px', textTransform: 'uppercase', color: 'var(--text-secondary)' }}>User</th>
              <th style={{ padding: '16px 20px', fontSize: '13px', textTransform: 'uppercase', color: 'var(--text-secondary)' }}>Email</th>
              <th style={{ padding: '16px 20px', fontSize: '13px', textTransform: 'uppercase', color: 'var(--text-secondary)' }}>Role</th>
              <th style={{ padding: '16px 20px', fontSize: '13px', textTransform: 'uppercase', color: 'var(--text-secondary)', textAlign: 'right' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredUsers.length === 0 ? (
              <tr>
                <td colSpan="4" style={{ padding: '32px', textAlign: 'center', color: 'var(--text-muted)' }}>
                  No users found.
                </td>
              </tr>
            ) : (
              filteredUsers.map((u) => (
                <tr key={u.id} style={{ borderBottom: '1px solid var(--border)' }}>
                  <td style={{ padding: '16px 20px', fontWeight: '600' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                      <div className="user-avatar" style={{ width: '32px', height: '32px', fontSize: '13px', background: 'var(--accent)' }}>
                        {u.name?.charAt(0)?.toUpperCase()}
                      </div>
                      <span>{u.name}</span>
                    </div>
                  </td>
                  <td style={{ padding: '16px 20px', color: 'var(--text-secondary)' }}>{u.email}</td>
                  <td style={{ padding: '16px 20px' }}>
                    <span className={`badge ${u.role === 'Admin' ? 'badge-danger' : 'badge-info'}`} style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                      {u.role === 'Admin' && <ShieldCheck size={12} />}
                      {u.role}
                    </span>
                  </td>
                  <td style={{ padding: '16px 20px', textAlign: 'right' }}>
                    <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px' }}>
                      <button onClick={() => handleOpenEditModal(u)} className="btn btn-sm btn-secondary" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                        <Edit3 size={12} /> Edit
                      </button>
                      <button
                        onClick={() => handleDeleteUser(u.id, u.name)}
                        className="btn btn-sm btn-danger"
                        disabled={u.id === currentUser?.id}
                        style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}
                        title={u.id === currentUser?.id ? 'Cannot delete your active account' : 'Delete user'}
                      >
                        <Trash2 size={12} /> Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Modal Popup */}
      {showModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(0, 0, 0, 0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }}>
          <div className="auth-card" style={{ width: '100%', maxWidth: '480px', position: 'relative' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
              <h2 style={{ fontSize: '20px', fontWeight: '700' }}>
                {editingUser ? `Edit User: ${editingUser.name}` : 'Add New User'}
              </h2>
              <button onClick={handleCloseModal} style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)' }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="auth-form">
              <div className="inputBox">
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder=" "
                  required
                />
                <span><UserIcon size={12} /> Full Name</span>
              </div>

              <div className="inputBox">
                <input
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  placeholder=" "
                  required
                />
                <span>Email</span>
              </div>

              <div className="inputBox">
                <input
                  type="password"
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                  placeholder=" "
                  required={!editingUser}
                />
                <span>{editingUser ? 'New Password (Optional)' : 'Password'}</span>
              </div>

              <div className="form-group" style={{ marginTop: '10px' }}>
                <label htmlFor="role" style={{ fontSize: '12px', fontWeight: '600', textTransform: 'uppercase', letterSpacing: '1px' }}>Role</label>
                <select
                  id="role"
                  value={formData.role}
                  onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                  style={{ padding: '12px', borderRadius: '8px', background: 'var(--bg-input)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
                >
                  <option value="User">Regular User</option>
                  <option value="Admin">Administrator</option>
                </select>
              </div>

              <div style={{ display: 'flex', gap: '10px', marginTop: '16px' }}>
                <button type="submit" className="btn btn-primary btn-full" disabled={submitting}>
                  <Check size={16} /> {submitting ? 'Saving...' : (editingUser ? 'Update User' : 'Create User')}
                </button>
                <button type="button" onClick={handleCloseModal} className="btn btn-secondary btn-full">
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
