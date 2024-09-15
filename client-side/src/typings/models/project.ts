import type { Task } from './task';

export interface Project {
  id: string;
  name: string;
  createdAt: string | number;
  updatedAt?: string | number;
  deletedAt?: string | number;

  tasks: Task[];
}
