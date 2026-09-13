import { apiRequest } from "./apiClient";

export interface Comment {
  id: string;
  content: string;
  authorId: string;
  projectId: string;
  createdAt: string;
  updatedAt: string | null;
}

export function getComments(token: string): Promise<Comment[]> {
  return apiRequest<Comment[]>("/api/comments", { token });
}

export function createComment(
  input: { projectId: string; content: string },
  token: string,
): Promise<Comment> {
  return apiRequest<Comment>("/api/comments", {
    method: "POST",
    body: input,
    token,
  });
}

export function deleteComment(id: string, token: string): Promise<void> {
  return apiRequest<void>(`/api/comments/${id}`, {
    method: "DELETE",
    token,
  });
}
