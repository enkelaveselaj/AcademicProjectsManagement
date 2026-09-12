import { apiRequest, apiFetchBlob } from "./apiClient";

export interface DocumentFile {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedById: string;
  projectId: string;
  createdAt: string;
  updatedAt: string | null;
}

export function getDocuments(token: string): Promise<DocumentFile[]> {
  return apiRequest<DocumentFile[]>("/api/documents", { token });
}

export function uploadDocument(file: File, projectId: string, token: string): Promise<DocumentFile> {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("projectId", projectId);

  return apiRequest<DocumentFile>("/api/documents", {
    method: "POST",
    body: formData,
    token,
  });
}

export function deleteDocument(id: string, token: string): Promise<void> {
  return apiRequest<void>(`/api/documents/${id}`, {
    method: "DELETE",
    token,
  });
}

export function fetchDocumentBlob(id: string, token: string) {
  return apiFetchBlob(`/api/documents/${id}/download`, token);
}
