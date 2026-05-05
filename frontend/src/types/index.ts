export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled';

export interface User {
  email: string;
  name: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  name: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
}

export interface Task {
  id: string;
  userId: string;
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  dueDate: string;
}

export interface UpdateTaskRequest {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
}

export interface ProblemDetails {
  title: string;
  status: number;
  detail?: string;
}
