import client from './client';
import type { CreateTaskRequest, Task, UpdateTaskRequest } from '../types';

export const tasksApi = {
  getAll: async () => {
    const response = await client.get<Task[]>('/api/tasks');
    return response.data;
  },
  create: async (data: CreateTaskRequest) => {
    const response = await client.post<Task>('/api/tasks', data);
    return response.data;
  },
  update: async (id: string, data: UpdateTaskRequest) => {
    const response = await client.put<Task>(`/api/tasks/${id}`, data);
    return response.data;
  },
  delete: async (id: string) => {
    await client.delete(`/api/tasks/${id}`);
  },
};
