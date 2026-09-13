import { apiRequest } from "./apiClient";

export type InvitationStatus = 1 | 2 | 3;

export const INVITATION_STATUS_LABEL: Record<InvitationStatus, string> = {
  1: "Pending",
  2: "Accepted",
  3: "Declined",
};

export interface ProjectInvitation {
  id: string;
  projectId: string;
  invitedUserId: string;
  invitedById: string;
  role: string;
  status: InvitationStatus;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateInvitationInput {
  projectId: string;
  invitedUserId: string;
  role: "Student" | "Mentor";
}

export function getInvitations(token: string): Promise<ProjectInvitation[]> {
  return apiRequest<ProjectInvitation[]>("/api/project-invitations", { token });
}

export function createInvitation(input: CreateInvitationInput, token: string): Promise<ProjectInvitation> {
  return apiRequest<ProjectInvitation>("/api/project-invitations", {
    method: "POST",
    body: input,
    token,
  });
}

export function acceptInvitation(id: string, token: string): Promise<ProjectInvitation> {
  return apiRequest<ProjectInvitation>(`/api/project-invitations/${id}/accept`, {
    method: "PUT",
    token,
  });
}

export function declineInvitation(id: string, token: string): Promise<ProjectInvitation> {
  return apiRequest<ProjectInvitation>(`/api/project-invitations/${id}/decline`, {
    method: "PUT",
    token,
  });
}

export function cancelInvitation(id: string, token: string): Promise<void> {
  return apiRequest<void>(`/api/project-invitations/${id}`, {
    method: "DELETE",
    token,
  });
}
