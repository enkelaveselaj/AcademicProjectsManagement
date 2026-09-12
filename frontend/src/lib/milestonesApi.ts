import { apiRequest } from "./apiClient";

export type MilestoneStatus = 1 | 2 | 3 | 4;

export interface ProjectMilestone {
  id: string;
  title: string;
  description: string | null;
  dueDate: string;
  completedAt: string | null;
  status: MilestoneStatus;
  projectId: string;
  createdAt: string;
  updatedAt: string | null;
}

export function getMilestones(token: string): Promise<ProjectMilestone[]> {
  return apiRequest<ProjectMilestone[]>("/api/project-milestones", { token });
}
