import { apiRequest } from "./apiClient";
import type { ProjectStatus } from "./projectsApi";

export interface ProjectStatusHistoryEntry {
  id: string;
  projectId: string;
  previousStatus: ProjectStatus;
  newStatus: ProjectStatus;
  comment: string | null;
  createdAt: string;
}

export function getStatusHistories(token: string): Promise<ProjectStatusHistoryEntry[]> {
  return apiRequest<ProjectStatusHistoryEntry[]>("/api/project-status-histories", { token });
}
