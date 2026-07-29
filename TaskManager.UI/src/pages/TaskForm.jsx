import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import API from '../api/axiosInstance';
import Loader from '../components/Loader';
import { useAuth } from '../context/AuthContext';
import { ArrowLeft, Save, Plus, Trash2, UserCheck, CheckSquare } from 'lucide-react';

export default function TaskForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user: currentUser } = useAuth();
  const isEdit = Boolean(id);
  const isAdmin = currentUser?.role === 'Admin';

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    dueDate: '',
    priority: 'Medium',
    status: 'Pending',
    category: '',
    assignedUserId: '',
  });
  const [usersList, setUsersList] = useState([]);
  const [subtasks, setSubtasks] = useState([]);
  const [newSubtaskTitle, setNewSubtaskTitle] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);

  useEffect(() => {
    const initData = async () => {
      try {
        if (isAdmin) {
          const usersRes = await API.get('/user/all');
          setUsersList(usersRes.data);
        }

        if (isEdit) {
          const taskRes = await API.get(`/tasks/${id}`);
          const task = taskRes.data;
          setFormData({
            title: task.title,
            description: task.description || '',
            dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
            priority: task.priority,
            status: task.status,
            category: task.category || '',
            assignedUserId: task.assignedUserId || '',
          });
          setSubtasks(task.subTasks || []);
        }
      } catch (err) {
        setError('Failed to initialize task form.');
      } finally {
        setFetching(false);
      }
    };

    initData();
  }, [id, isEdit, isAdmin]);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleAddSubtask = () => {
    if (!newSubtaskTitle.trim()) return;
    setSubtasks([...subtasks, { title: newSubtaskTitle.trim(), isCompleted: false }]);
    setNewSubtaskTitle('');
  };

  const handleRemoveSubtask = (index) => {
    setSubtasks(subtasks.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    const payload = {
      ...formData,
      dueDate: formData.dueDate || null,
      assignedUserId: formData.assignedUserId ? parseInt(formData.assignedUserId, 10) : null,
      subTasks: subtasks,
    };

    try {
      if (isEdit) {
        await API.put(`/tasks/${id}`, payload);
      } else {
        await API.post('/tasks', payload);
      }
      navigate('/tasks');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save task.');
    } finally {
      setLoading(false);
    }
  };

  if (fetching) {
    return (
      <div className="page">
        <Loader text="Loading task form..." />
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1>{isEdit ? 'Edit Task' : 'Create New Task'}</h1>
        <Link to="/tasks" className="btn btn-secondary" style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
          <ArrowLeft size={16} /> Back to Tasks
        </Link>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      <div className="form-card">
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="title">Title *</label>
            <input
              id="title"
              name="title"
              type="text"
              value={formData.title}
              onChange={handleChange}
              placeholder="Enter task title"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              placeholder="Enter task description"
              rows="4"
            ></textarea>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="priority">Priority *</label>
              <select id="priority" name="priority" value={formData.priority} onChange={handleChange}>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="status">Status *</label>
              <select id="status" name="status" value={formData.status} onChange={handleChange}>
                <option value="Pending">Pending</option>
                <option value="InProgress">In Progress</option>
                <option value="Completed">Completed</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="category">Category</label>
              <input
                id="category"
                name="category"
                type="text"
                value={formData.category}
                onChange={handleChange}
                placeholder="e.g., Work, Personal"
              />
            </div>

            <div className="form-group">
              <label htmlFor="dueDate">Due Date</label>
              <input
                id="dueDate"
                name="dueDate"
                type="date"
                value={formData.dueDate}
                onChange={handleChange}
              />
            </div>
          </div>

          {/* Admin Task Assignment Field */}
          {isAdmin && (
            <div className="form-group">
              <label htmlFor="assignedUserId" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                <UserCheck size={16} /> Assign To User (Admin Only)
              </label>
              <select
                id="assignedUserId"
                name="assignedUserId"
                value={formData.assignedUserId}
                onChange={handleChange}
              >
                <option value="">-- Myself ({currentUser?.name}) --</option>
                {usersList.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.name} ({u.email}) [{u.role}]
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Subtasks Section */}
          <div className="form-group" style={{ marginTop: '20px' }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <CheckSquare size={16} /> Subtasks / Checklist
            </label>
            <div style={{ display: 'flex', gap: '8px', marginBottom: '12px' }}>
              <input
                type="text"
                value={newSubtaskTitle}
                onChange={(e) => setNewSubtaskTitle(e.target.value)}
                placeholder="Add a subtask item..."
                onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); handleAddSubtask(); } }}
              />
              <button type="button" onClick={handleAddSubtask} className="btn btn-secondary" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                <Plus size={16} /> Add
              </button>
            </div>

            {subtasks.length > 0 && (
              <ul style={{ listStyle: 'none', padding: 0, display: 'flex', flexDirection: 'column', gap: '8px' }}>
                {subtasks.map((st, index) => (
                  <li key={index} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', background: 'var(--bg-input)', padding: '8px 12px', borderRadius: 'var(--radius-sm)' }}>
                    <span>{st.title}</span>
                    <button type="button" onClick={() => handleRemoveSubtask(index)} className="btn btn-sm btn-danger" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                      <Trash2 size={12} /> Remove
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="form-actions">
            <button type="submit" className="btn btn-primary" disabled={loading} style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
              <Save size={16} /> {loading ? 'Saving...' : (isEdit ? 'Update Task' : 'Create Task')}
            </button>
            <Link to="/tasks" className="btn btn-secondary">Cancel</Link>
          </div>
        </form>
      </div>
    </div>
  );
}
