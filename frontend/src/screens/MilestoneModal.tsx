import { useState, type FormEvent } from "react";
import { X } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import {
  createMilestone,
  deleteMilestone,
  updateMilestone,
  MILESTONE_STATUS_META,
  type MilestoneStatus,
  type ProjectMilestone,
} from "../lib/milestonesApi";

const EDITABLE_STATUSES: MilestoneStatus[] = [1, 2, 3];

function milestoneErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return err.errors?.Title?.[0] ?? err.errors?.Description?.[0] ?? err.errors?.DueDate?.[0] ?? err.message;
  }
  return "Something went wrong. Please try again.";
}

function toDateInputValue(value: string): string {
  return value.slice(0, 10);
}

interface MilestoneModalProps {
  projectId: string;
  initial?: ProjectMilestone;
  onClose: () => void;
  onSaved: () => void;
}

export function MilestoneModal({ projectId, initial, onClose, onSaved }: MilestoneModalProps) {
  const { token } = useAuth();
  const [title, setTitle] = useState(initial?.title ?? "");
  const [description, setDescription] = useState(initial?.description ?? "");
  const [dueDate, setDueDate] = useState(initial ? toDateInputValue(initial.dueDate) : "");
  const [status, setStatus] = useState<MilestoneStatus>(initial?.status ?? 1);
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
      if (initial) {
        await updateMilestone(
          initial.id,
          { title, description: description || undefined, dueDate, status, projectId },
          token,
        );
      } else {
        await createMilestone({ title, description: description || undefined, dueDate, projectId }, token);
      }
      onSaved();
    } catch (err) {
      setError(milestoneErrorMessage(err));
    } finally {
      setIsSaving(false);
    }
  }

  async function handleDelete() {
    if (!initial || !token) {
      return;
    }

    if (!window.confirm(`Delete milestone "${initial.title}"? This cannot be undone.`)) {
      return;
    }

    setError(null);
    setIsDeleting(true);

    try {
      await deleteMilestone(initial.id, token);
      onSaved();
    } catch (err) {
      setError(milestoneErrorMessage(err));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h2 className="font-serif text-xl font-bold text-slate-900">
            {initial ? "Edit milestone" : "New milestone"}
          </h2>
          <button type="button" onClick={onClose} className="text-slate-400 hover:text-slate-700">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="mt-4 space-y-4">
          <div>
            <label htmlFor="milestoneTitle" className="block text-sm font-medium text-slate-800">
              Title
            </label>
            <input
              id="milestoneTitle"
              required
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
            />
          </div>

          <div>
            <label htmlFor="milestoneDescription" className="block text-sm font-medium text-slate-800">
              Description
            </label>
            <textarea
              id="milestoneDescription"
              rows={3}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
            />
          </div>

          <div className={`grid gap-4 ${initial ? "grid-cols-2" : "grid-cols-1"}`}>
            <div>
              <label htmlFor="milestoneDueDate" className="block text-sm font-medium text-slate-800">
                Due date
              </label>
              <input
                id="milestoneDueDate"
                type="date"
                required
                value={dueDate}
                onChange={(event) => setDueDate(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
              />
            </div>

            {initial && (
              <div>
                <label htmlFor="milestoneStatus" className="block text-sm font-medium text-slate-800">
                  Status
                </label>
                <select
                  id="milestoneStatus"
                  value={status}
                  onChange={(event) => setStatus(Number(event.target.value) as MilestoneStatus)}
                  className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
                >
                  {EDITABLE_STATUSES.map((value) => (
                    <option key={value} value={value}>
                      {MILESTONE_STATUS_META[value].label}
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <div className="flex items-center justify-between pt-2">
            {initial ? (
              <button
                type="button"
                onClick={handleDelete}
                disabled={isDeleting}
                className="text-sm font-medium text-red-600 hover:text-red-800 disabled:opacity-50"
              >
                {isDeleting ? "Deleting..." : "Delete milestone"}
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
