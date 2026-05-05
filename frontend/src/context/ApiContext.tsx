import React, { createContext, useContext } from 'react';
import { tasksApi } from '../api/tasks';
import { authApi } from '../api/auth';

export interface ApiContextType {
  tasks: typeof tasksApi;
  auth: typeof authApi;
}

export const ApiContext = createContext<ApiContextType | null>(null);

export const ApiProvider: React.FC<{ children: React.ReactNode, value?: ApiContextType }> = ({ children, value }) => {
  const defaultApi = {
    tasks: tasksApi,
    auth: authApi
  };

  return (
    <ApiContext.Provider value={value || defaultApi}>
      {children}
    </ApiContext.Provider>
  );
};

export const useApi = () => {
  const context = useContext(ApiContext);
  if (!context) {
    throw new Error('useApi must be used within an ApiProvider');
  }
  return context;
};
