import { apiRequest } from "./apiClient";

export interface UserSummary {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface PendingUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  requestedRole: string;
  dateOfBirth: string;
  personalIdNumber: string;
  studentId: string | null;
  requestedAt: string;
}

export function getUsers(token: string): Promise<UserSummary[]> {
  return apiRequest<UserSummary[]>("/api/auth/users", { token });
}

export function getPendingUsers(token: string): Promise<PendingUser[]> {
  return apiRequest<PendingUser[]>("/api/auth/pending-users", { token });
}

export function changeUserRole(userId: string, role: string, token: string): Promise<UserSummary> {
  return apiRequest<UserSummary>(`/api/auth/users/${userId}/role`, {
    method: "PUT",
    body: { role },
    token,
  });
}

export function approveUser(userId: string, token: string): Promise<UserSummary> {
  return apiRequest<UserSummary>(`/api/auth/users/${userId}/approve`, {
    method: "PUT",
    token,
  });
}

export function rejectUser(userId: string, token: string): Promise<void> {
  return apiRequest<void>(`/api/auth/users/${userId}/reject`, {
    method: "DELETE",
    token,
  });
}
