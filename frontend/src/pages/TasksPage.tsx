import React, { useEffect, useState } from 'react';
import type { Task, CreateTaskRequest, UpdateTaskRequest } from '../types';
import { useApi } from '../context/ApiContext';
import TaskForm from '../components/tasks/TaskForm';
import { Plus, Trash2, Calendar, Clock, CheckCircle, Circle, AlertCircle, XCircle, Edit2 } from 'lucide-react';

const TasksPage: React.FC = () => {
  const { tasks: tasksApi } = useApi();
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | undefined>();
  const [formLoading, setFormLoading] = useState(false);

  useEffect(() => {
    loadTasks();
  }, []);

  const loadTasks = async () => {
    try {
      const data = await tasksApi.getAll();
      setTasks(data);
    } catch (err) {
      console.error('Failed to load tasks', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (data: any) => {
    setFormLoading(true);
    try {
      await tasksApi.create(data as CreateTaskRequest);
      await loadTasks();
      setIsFormOpen(false);
    } catch (err) {
      console.error('Failed to create task', err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleUpdate = async (data: any) => {
    setFormLoading(true);
    try {
      await tasksApi.update(data.id, data as UpdateTaskRequest);
      await loadTasks();
      setIsFormOpen(false);
      setEditingTask(undefined);
    } catch (err) {
      console.error('Failed to update task', err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this task?')) return;
    try {
      await tasksApi.delete(id);
      setTasks(tasks.filter(t => t.id !== id));
    } catch (err) {
      console.error('Failed to delete task', err);
    }
  };

  const openCreate = () => {
    setEditingTask(undefined);
    setIsFormOpen(true);
  };

  const openEdit = (task: Task) => {
    setEditingTask(task);
    setIsFormOpen(true);
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Completed': return <CheckCircle size={18} style={{ color: 'var(--success)' }} />;
      case 'InProgress': return <Clock size={18} style={{ color: 'var(--warning)' }} />;
      case 'Cancelled': return <XCircle size={18} style={{ color: 'var(--danger)' }} />;
      default: return <Circle size={18} style={{ color: 'var(--text-muted)' }} />;
    }
  };

  return (
    <div className="container animate-fade-in">
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '3rem' }}>
        <div>
          <h1 style={{ fontSize: '2rem', fontWeight: 'bold' }}>My Tasks</h1>
          <p style={{ color: 'var(--text-muted)' }}>You have {tasks.length} tasks in total</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>
          <Plus size={20} />
          New Task
        </button>
      </header>

      {loading ? (
        <div style={{ textAlign: 'center', padding: '4rem' }}>Loading tasks...</div>
      ) : tasks.length === 0 ? (
        <div className="glass" style={{ textAlign: 'center', padding: '5rem', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '1rem' }}>
          <AlertCircle size={48} style={{ color: 'var(--text-muted)' }} />
          <h2 style={{ fontSize: '1.5rem' }}>No tasks found</h2>
          <p style={{ color: 'var(--text-muted)' }}>Get started by creating your first task.</p>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(350px, 1fr))', gap: '1.5rem' }}>
          {tasks.map(task => (
            <div key={task.id} className="glass" style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1rem', transition: 'transform 0.2s' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  {getStatusIcon(task.status)}
                  <span style={{ fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.05em' }}>{task.status}</span>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <button className="btn" style={{ padding: '0.25rem', color: 'var(--text-muted)' }} onClick={() => openEdit(task)}>
                    <Edit2 size={18} />
                  </button>
                  <button className="btn" style={{ padding: '0.25rem', color: 'var(--danger)', background: 'transparent' }} onClick={() => handleDelete(task.id)}>
                    <Trash2 size={18} />
                  </button>
                </div>
              </div>

              <div>
                <h3 style={{ fontSize: '1.25rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>{task.title}</h3>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.925rem', lineHeight: 1.5 }}>{task.description}</p>
              </div>

              <div style={{ marginTop: 'auto', paddingTop: '1rem', borderTop: '1px solid var(--glass-border)', display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
                <Calendar size={16} />
                Due {new Date(task.dueDate).toLocaleDateString()}
              </div>
            </div>
          ))}
        </div>
      )}

      {isFormOpen && (
        <TaskForm 
          task={editingTask} 
          onClose={() => setIsFormOpen(false)} 
          onSubmit={editingTask ? handleUpdate : handleCreate}
          loading={formLoading}
        />
      )}
    </div>
  );
};

export default TasksPage;
