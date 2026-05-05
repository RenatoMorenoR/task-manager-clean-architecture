import React, { useState } from 'react';
import type { CreateTaskRequest, Task, TaskStatus } from '../../types';
import { X, Calendar, AlignLeft, Type, Clock } from 'lucide-react';

interface TaskFormProps {
  task?: Task;
  onSubmit: (data: CreateTaskRequest | Task) => void;
  onClose: () => void;
  loading: boolean;
}

const TaskForm: React.FC<TaskFormProps> = ({ task, onSubmit, onClose, loading }) => {
  const [title, setTitle] = useState(task?.title || '');
  const [description, setDescription] = useState(task?.description || '');
  const [dueDate, setDueDate] = useState(task?.dueDate ? new Date(task.dueDate).toISOString().split('T')[0] : '');
  const [status, setStatus] = useState<TaskStatus>(task?.status || 'Pending');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (task) {
      onSubmit({ ...task, title, description, dueDate, status });
    } else {
      onSubmit({ title, description, dueDate });
    }
  };

  return (
    <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.6)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000, padding: '1rem' }}>
      <div className="glass animate-fade-in" style={{ width: '100%', maxWidth: '500px', padding: '2rem', position: 'relative' }}>
        <button onClick={onClose} style={{ position: 'absolute', top: '1rem', right: '1rem', background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
          <X size={24} />
        </button>

        <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', marginBottom: '1.5rem' }}>
          {task ? 'Edit Task' : 'New Task'}
        </h2>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div>
            <label htmlFor="title" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
              <Type size={16} /> Title
            </label>
            <input 
              id="title"
              className="input" 
              value={title} 
              onChange={e => setTitle(e.target.value)} 
              placeholder="What needs to be done?" 
              required 
            />
          </div>

          <div>
            <label htmlFor="description" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
              <AlignLeft size={16} /> Description
            </label>
            <textarea 
              id="description"
              className="input" 
              style={{ minHeight: '100px', resize: 'vertical' }} 
              value={description} 
              onChange={e => setDescription(e.target.value)} 
              placeholder="Add more details..." 
            />
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <div>
              <label htmlFor="dueDate" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
                <Calendar size={16} /> Due Date
              </label>
              <input 
                id="dueDate"
                type="date" 
                className="input" 
                value={dueDate} 
                onChange={e => setDueDate(e.target.value)} 
                required 
              />
            </div>
            {task && (
              <div>
                <label htmlFor="status" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
                  <Clock size={16} /> Status
                </label>
                <select id="status" className="input" value={status} onChange={e => setStatus(e.target.value as TaskStatus)}>
                  <option value="Pending">Pending</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Completed">Completed</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
              </div>
            )}
          </div>

          <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
            <button type="button" onClick={onClose} className="btn" style={{ flex: 1, background: 'rgba(255,255,255,0.05)', color: 'var(--text-main)' }}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" style={{ flex: 1, justifyContent: 'center' }} disabled={loading}>
              {loading ? 'Saving...' : (task ? 'Update Task' : 'Create Task')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TaskForm;
