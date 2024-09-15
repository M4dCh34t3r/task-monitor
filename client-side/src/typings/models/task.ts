import type { Project } from './project';
import type { TimeTracker } from './timeTracker';

export interface Task {
  id: string;
  projectId: string;
  name: string;
  description: string;
  createdAt: string | number;
  updatedAt?: string | number;
  deletedAt?: string | number;

  timeTrackers: TimeTracker[];

  project?: Project;
}
