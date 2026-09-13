import { useState, type FormEvent } from "react";
import { X } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { resetUserPassword, type UserSummary } from "../lib/usersApi";

function resetErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return err.errors?.password?.[0] ?? err.message;
  }
  return "Something went wrong. Please try again.";
}

interface ResetUserPasswordModalProps {
  user: UserSummary;
  onClose: () => void;
}

export function ResetUserPasswordModal({ user, onClose }: ResetUserPasswordModalProps) {
  const { token } = useAuth();
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [isDone, setIsDone] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!token) {
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("The passwords don't match.");
      return;
    }

    setError(null);
    setIsSaving(true);

    try {
      await resetUserPassword(user.id, newPassword, token);
      setIsDone(true);
    } catch (err) {
      setError(resetErrorMessage(err));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h2 className="font-serif text-xl font-bold text-slate-900">Reset password</h2>
          <button type="button" onClick={onClose} className="text-slate-400 hover:text-slate-700">
            <X className="h-5 w-5" />
          </button>
        </div>
        <p className="mt-1 text-sm text-slate-500">
          for {user.firstName} {user.lastName} ({user.email})
        </p>

        {isDone ? (
          <div className="mt-4">
            <p className="text-sm text-emerald-700">
              Password reset. Make sure to relay the new password to {user.firstName} securely.
            </p>
            <button
              type="button"
              onClick={onClose}
              className="mt-4 w-full rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800"
            >
              Done
            </button>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="mt-4 space-y-4">
            <div>
              <label htmlFor="newUserPassword" className="block text-sm font-medium text-slate-800">
                New password
              </label>
              <input
                id="newUserPassword"
                type="password"
                required
                autoComplete="new-password"
                value={newPassword}
                onChange={(event) => setNewPassword(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
              />
            </div>

            <div>
              <label htmlFor="confirmUserPassword" className="block text-sm font-medium text-slate-800">
                Confirm new password
              </label>
              <input
                id="confirmUserPassword"
                type="password"
                required
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
              />
            </div>

            {error && <p className="text-sm text-red-600">{error}</p>}

            <button
              type="submit"
              disabled={isSaving}
              className="w-full rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {isSaving ? "Saving..." : "Reset password"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
