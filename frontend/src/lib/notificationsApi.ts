import { apiRequest } from "./apiClient";

export type NotificationType = 1 | 2 | 3 | 4;

export interface Notification {
  id: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  userId: string;
  createdAt: string;
  updatedAt: string | null;
}

export function getNotifications(token: string): Promise<Notification[]> {
  return apiRequest<Notification[]>("/api/notifications", { token });
}

export function markNotificationAsRead(id: string, token: string): Promise<Notification> {
  return apiRequest<Notification>(`/api/notifications/${id}/read`, {
    method: "PUT",
    token,
  });
}

export function markAllNotificationsAsRead(token: string): Promise<void> {
  return apiRequest<void>("/api/notifications/mark-all-read", {
    method: "PUT",
    token,
  });
}
