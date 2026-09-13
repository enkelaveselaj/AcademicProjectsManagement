import { apiRequest } from "./apiClient";

export type MilestoneStatus = 1 | 2 | 3 | 4;

export const MILESTONE_STATUS_META: Record<MilestoneStatus, { label: string; badge: string }> = {
  1: { label: "pending", badge: "bg-slate-100 text-slate-600" },
  2: { label: "in progress", badge: "bg-sky-50 text-sky-700" },
  3: { label: "completed", badge: "bg-emerald-50 text-emerald-700" },
  4: { label: "overdue", badge: "bg-red-50 text-red-700" },
};

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
