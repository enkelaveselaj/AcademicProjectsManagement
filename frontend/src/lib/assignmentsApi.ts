import { apiRequest } from "./apiClient";

export interface ProjectAssignment {
  id: string;
  projectId: string;
  userId: string;
  role: string;
  createdAt: string;
  updatedAt: string | null;
}

export function getAssignments(token: string): Promise<ProjectAssignment[]> {
  return apiRequest<ProjectAssignment[]>("/api/project-assignments", { token });
}
