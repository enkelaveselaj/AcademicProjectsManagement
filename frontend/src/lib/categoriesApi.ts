import { apiRequest } from "./apiClient";

export interface Category {
  id: string;
  name: string;
  description: string | null;
  projectCount: number;
}

export interface CategoryInput {
  name: string;
  description?: string;
}

export function getCategories(token: string): Promise<Category[]> {
  return apiRequest<Category[]>("/api/categories", { token });
}

export function createCategory(input: CategoryInput, token: string): Promise<Category> {
  return apiRequest<Category>("/api/categories", {
    method: "POST",
    body: input,
    token,
  });
}

export function updateCategory(id: string, input: CategoryInput, token: string): Promise<Category> {
  return apiRequest<Category>(`/api/categories/${id}`, {
    method: "PUT",
    body: input,
    token,
  });
}

export function deleteCategory(id: string, token: string): Promise<void> {
  return apiRequest<void>(`/api/categories/${id}`, {
    method: "DELETE",
    token,
  });
}
