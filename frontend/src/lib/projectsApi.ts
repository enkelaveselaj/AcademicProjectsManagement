import { apiRequest } from "./apiClient";

export type ProjectStatus = 1 | 2 | 3 | 4 | 5 | 6 | 7;

export const PROJECT_STATUSES: { value: ProjectStatus; label: string }[] = [
  { value: 1, label: "Draft" },
  { value: 2, label: "Submitted" },
  { value: 3, label: "Approved" },
  { value: 4, label: "In Progress" },
  { value: 5, label: "Under Review" },
  { value: 6, label: "Completed" },
  { value: 7, label: "Cancelled" },
];

export function projectStatusLabel(status: ProjectStatus): string {
  return PROJECT_STATUSES.find((option) => option.value === status)?.label ?? "Unknown";
}

export interface Project {
  id: string;
  title: string;
  description: string | null;
  status: ProjectStatus;
  categoryId: string;
  categoryName: string;
  createdById: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface ProjectInput {
  title: string;
  description?: string;
  status: ProjectStatus;
  categoryId: string;
}

export function getProjects(token: string): Promise<Project[]> {
  return apiRequest<Project[]>("/api/projects", { token });
}

export function createProject(input: ProjectInput, token: string): Promise<Project> {
  return apiRequest<Project>("/api/projects", {
    method: "POST",
    body: input,
    token,
  });
}

export function updateProject(id: string, input: ProjectInput, token: string): Promise<Project> {
  return apiRequest<Project>(`/api/projects/${id}`, {
    method: "PUT",
    body: input,
    token,
  });
}

export function deleteProject(id: string, token: string): Promise<void> {
  return apiRequest<void>(`/api/projects/${id}`, {
    method: "DELETE",
    token,
  });
}
