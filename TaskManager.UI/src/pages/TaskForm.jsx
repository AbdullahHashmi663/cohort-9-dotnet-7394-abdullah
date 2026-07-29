import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import API from '../api/axiosInstance';

export default function TaskForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    dueDate: '',
    priority: 'Medium',
    status: 'Pending',
    category: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);

  useEffect(() => {
    if (isEdit) {
      fetchTask();
    }
  }, [id]);

  const fetchTask = async () => {
    try {
      const response = await API.get(`/tasks/${id}`);
      const task = response.data;
      setFormData({
        title: task.title,
        description: task.description || '',
        dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
        priority: task.priority,
        status: task.status,
        category: task.category || '',
      });
    } catch (err) {
      setError('Failed to load task for editing.');
    } finally {
      setFetching(false);
    }
  };

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    const payload = {
      ...formData,
      dueDate: formData.dueDate || null,
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
        <div className="loading-screen">
          <div className="spinner"></div>
          <p>Loading task...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1>{isEdit ? 'Edit Task' : 'Create New Task'}</h1>
        <Link to="/tasks" className="btn btn-secondary">← Back to Tasks</Link>
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

          <div className="form-actions">
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving...' : (isEdit ? 'Update Task' : 'Create Task')}
            </button>
            <Link to="/tasks" className="btn btn-secondary">Cancel</Link>
          </div>
        </form>
      </div>
    </div>
  );
}
