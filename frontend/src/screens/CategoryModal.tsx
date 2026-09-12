import { useState, type FormEvent } from "react";
import { X } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { createCategory, deleteCategory, updateCategory, type Category } from "../lib/categoriesApi";

function categoryErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return err.errors?.Name?.[0] ?? err.errors?.Description?.[0] ?? err.message;
  }
  return "Something went wrong. Please try again.";
}

interface CategoryModalProps {
  initial?: Category;
  onClose: () => void;
  onSaved: () => void;
}

export function CategoryModal({ initial, onClose, onSaved }: CategoryModalProps) {
  const { token } = useAuth();
  const [name, setName] = useState(initial?.name ?? "");
  const [description, setDescription] = useState(initial?.description ?? "");
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
      const input = { name, description: description || undefined };
      if (initial) {
        await updateCategory(initial.id, input, token);
      } else {
        await createCategory(input, token);
      }
      onSaved();
    } catch (err) {
      setError(categoryErrorMessage(err));
    } finally {
      setIsSaving(false);
    }
  }

  async function handleDelete() {
    if (!initial || !token) {
      return;
    }

    if (!window.confirm(`Delete the "${initial.name}" category? This cannot be undone.`)) {
      return;
    }

    setError(null);
    setIsDeleting(true);

    try {
      await deleteCategory(initial.id, token);
      onSaved();
    } catch (err) {
      setError(categoryErrorMessage(err));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-sm rounded-xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h2 className="font-serif text-xl font-bold text-slate-900">
            {initial ? "Edit category" : "New category"}
          </h2>
          <button type="button" onClick={onClose} className="text-slate-400 hover:text-slate-700">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="mt-4 space-y-4">
          <div>
            <label htmlFor="categoryName" className="block text-sm font-medium text-slate-800">
              Name
            </label>
            <input
              id="categoryName"
              required
              value={name}
              onChange={(event) => setName(event.target.value)}
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
            />
          </div>

          <div>
            <label htmlFor="categoryDescription" className="block text-sm font-medium text-slate-800">
              Description
            </label>
            <textarea
              id="categoryDescription"
              rows={3}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
            />
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
                {isDeleting ? "Deleting..." : "Delete category"}
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
