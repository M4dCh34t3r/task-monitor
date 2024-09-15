import type { Collaborator } from './collaborator';
import type { Task } from './task';

export interface TimeTracker {
  id: string;
  taskId: string;
  collaboratorId: string;
  startDate?: string | number;
  endDate?: string | number;
  timeZoneId: string;

  task?: Task;
  collaborator?: Collaborator;
}
