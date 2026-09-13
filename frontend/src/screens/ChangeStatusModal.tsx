import { useState, type FormEvent } from "react";
import { X } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { updateProject, PROJECT_STATUSES, type Project, type ProjectStatus } from "../lib/projectsApi";

function statusErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return err.errors?.Status?.[0] ?? err.message;
  }
  return "Something went wrong. Please try again.";
}

interface ChangeStatusModalProps {
  project: Project;
  onClose: () => void;
  onSaved: () => void;
}

export function ChangeStatusModal({ project, onClose, onSaved }: ChangeStatusModalProps) {
  const { token } = useAuth();
  const [status, setStatus] = useState<ProjectStatus>(project.status);
  const [comment, setComment] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!token) {
      return;
    }

    setError(null);
    setIsSaving(true);

    try {
      await updateProject(
        project.id,
        {
          title: project.title,
          description: project.description ?? undefined,
          status,
          categoryId: project.categoryId,
          statusChangeComment: comment || undefined,
        },
        token,
      );
      onSaved();
    } catch (err) {
      setError(statusErrorMessage(err));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h2 className="font-serif text-xl font-bold text-slate-900">Change project status</h2>
          <button type="button" onClick={onClose} className="text-slate-400 hover:text-slate-700">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="mt-4 space-y-4">
          <div>
            <label htmlFor="newStatus" className="block text-sm font-medium text-slate-800">
              New status
            </label>
            <select
              id="newStatus"
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

          <div>
            <label htmlFor="statusComment" className="block text-sm font-medium text-slate-800">
              Comment (optional)
            </label>
            <textarea
              id="statusComment"
              rows={3}
              value={comment}
              onChange={(event) => setComment(event.target.value)}
              placeholder="Why is the status changing?"
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-slate-400 focus:outline-none"
            />
          </div>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <div className="flex justify-end pt-2">
            <button
              type="submit"
              disabled={isSaving || status === project.status}
              className="rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {isSaving ? "Saving..." : "Update status"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
