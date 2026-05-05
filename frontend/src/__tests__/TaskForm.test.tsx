import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import TaskForm from '../components/tasks/TaskForm';
import type { Task } from '../types';

describe('TaskForm', () => {
  const mockOnClose = vi.fn();
  const mockOnSubmit = vi.fn();

  it('should render for creation', () => {
    render(<TaskForm onClose={mockOnClose} onSubmit={mockOnSubmit} loading={false} />);
    
    expect(screen.getByText('New Task')).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/What needs to be done?/i)).toBeInTheDocument();
    expect(screen.queryByText('Status')).not.toBeInTheDocument();
  });

  it('should render for editing with initial data', () => {
    const task: Task = {
      id: '1',
      userId: 'u1',
      title: 'Existing Task',
      description: 'Desc',
      status: 'InProgress',
      dueDate: '2026-10-10T00:00:00Z',
      createdAt: '',
      updatedAt: ''
    };

    render(<TaskForm task={task} onClose={mockOnClose} onSubmit={mockOnSubmit} loading={false} />);

    expect(screen.getByText('Edit Task')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Existing Task')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
  });

  it('should call onSubmit with form data', () => {
    render(<TaskForm onClose={mockOnClose} onSubmit={mockOnSubmit} loading={false} />);

    fireEvent.change(screen.getByPlaceholderText(/What needs to be done?/i), { target: { value: 'My New Task' } });
    fireEvent.change(screen.getByPlaceholderText(/Add more details.../i), { target: { value: 'Some description' } });
    
    // Set date (using raw value for input type="date" which is YYYY-MM-DD)
    fireEvent.change(screen.getByLabelText(/Due Date/i), { target: { value: '2026-12-31' } });

    fireEvent.click(screen.getByRole('button', { name: /Create Task/i }));

    expect(mockOnSubmit).toHaveBeenCalledWith({
      title: 'My New Task',
      description: 'Some description',
      dueDate: '2026-12-31'
    });
  });
});
