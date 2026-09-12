import { useState, type FormEvent } from "react";
import { X } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import type { Category } from "../lib/categoriesApi";
import {
  createProject,
  deleteProject,
  updateProject,
  PROJECT_STATUSES,
  type Project,
  type ProjectStatus,
} from "../lib/projectsApi";

function projectErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return (
      err.errors?.Title?.[0] ??
      err.errors?.Description?.[0] ??
      err.errors?.CategoryId?.[0] ??
      err.errors?.Status?.[0] ??
      err.message
    );
  }
  return "Something went wrong. Please try again.";
}

interface ProjectModalProps {
  initial?: Project;
  categories: Category[];
  canDelete: boolean;
  onClose: () => void;
  onSaved: () => void;
}

export function ProjectModal({ initial, categories, canDelete, onClose, onSaved }: ProjectModalProps) {
  const { token } = useAuth();
  const [title, setTitle] = useState(initial?.title ?? "");
  const [description, setDescription] = useState(initial?.description ?? "");
  const [status, setStatus] = useState<ProjectStatus>(initial?.status ?? 1);
  const [categoryId, setCategoryId] = useState(initial?.categoryId ?? categories[0]?.id ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!token) {
      return;
    }

    setError(null);
    setIsSaving(true);

    try {
      const input = { title, description: description || undefined, status, categoryId };
      if (initial) {
        await updateProject(initial.id, input, token);
      } else {
        await createProject(input, token);
      }
      onSaved();
    } catch (err) {
      setError(projectErrorMessage(err));
    } finally {
      setIsSaving(false);
    }
  }

  async function handleDelete() {
    if (!initial || !token) {
      return;
    }

    if (!window.confirm(`Delete "${initial.title}"? This cannot be undone.`)) {
      return;
    }

    setError(null);
    setIsDeleting(true);

    try {
      await deleteProject(initial.id, token);
      onSaved();
    } catch (err) {
      setError(projectErrorMessage(err));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h2 className="font-serif text-xl font-bold text-slate-900">
            {initial ? "Edit project" : "New project"}
          </h2>
          <button type="button" onClick={onClose} className="text-slate-400 hover:text-slate-700">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="mt-4 space-y-4">
          <div>
            <label htmlFor="projectTitle" className="block text-sm font-medium text-slate-800">
              Title
            </label>
            <input
              id="projectTitle"
              required
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
            />
          </div>

          <div>
            <label htmlFor="projectDescription" className="block text-sm font-medium text-slate-800">
              Description
            </label>
            <textarea
              id="projectDescription"
              rows={3}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="projectCategory" className="block text-sm font-medium text-slate-800">
                Category
              </label>
              <select
                id="projectCategory"
                required
                value={categoryId}
                onChange={(event) => setCategoryId(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
              >
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="projectStatus" className="block text-sm font-medium text-slate-800">
                Status
              </label>
              <select
                id="projectStatus"
                value={status}
                onChange={(event) => setStatus(Number(event.target.value) as ProjectStatus)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
              >
                {PROJECT_STATUSES.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <div className="flex items-center justify-between pt-2">
            {initial && canDelete ? (
              <button
                type="button"
                onClick={handleDelete}
                disabled={isDeleting}
                className="text-sm font-medium text-red-600 hover:text-red-800 disabled:opacity-50"
              >
                {isDeleting ? "Deleting..." : "Delete project"}
              </button>
            ) : (
              <span />
            )}

            <button
              type="submit"
              disabled={isSaving}
              className="rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {isSaving ? "Saving..." : "Save"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
