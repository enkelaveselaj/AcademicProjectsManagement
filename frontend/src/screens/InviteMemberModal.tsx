import { useMemo, useState } from "react";
import { Search, X } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { createInvitation } from "../lib/invitationsApi";
import type { UserDirectoryEntry } from "../lib/usersApi";

function invitationErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return err.message;
  }
  return "Something went wrong. Please try again.";
}

interface InviteMemberModalProps {
  projectId: string;
  role: "Student" | "Mentor";
  directory: UserDirectoryEntry[];
  excludedUserIds: Set<string>;
  onClose: () => void;
  onInvited: () => void;
}

export function InviteMemberModal({
  projectId,
  role,
  directory,
  excludedUserIds,
  onClose,
  onInvited,
}: InviteMemberModalProps) {
  const { token } = useAuth();
  const [search, setSearch] = useState("");
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSending, setIsSending] = useState(false);

  const candidates = useMemo(() => {
    const term = search.trim().toLowerCase();

    return directory.filter((entry) => {
      if (entry.role !== role || excludedUserIds.has(entry.id)) {
        return false;
      }
      if (!term) {
        return true;
      }
      return `${entry.firstName} ${entry.lastName}`.toLowerCase().includes(term);
    });
  }, [directory, role, excludedUserIds, search]);

  async function handleSend() {
    if (!token || !selectedUserId) {
      return;
    }

    setError(null);
    setIsSending(true);

    try {
      await createInvitation({ projectId, invitedUserId: selectedUserId, role }, token);
      onInvited();
    } catch (err) {
      setError(invitationErrorMessage(err));
    } finally {
      setIsSending(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h2 className="font-serif text-xl font-bold text-slate-900">
            {role === "Mentor" ? "Request a mentor" : "Invite a student"}
          </h2>
          <button type="button" onClick={onClose} className="text-slate-400 hover:text-slate-700">
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="relative mt-4">
          <Search className="pointer-events-none absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            autoFocus
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder={role === "Mentor" ? "Search mentors..." : "Search students..."}
            className="w-full rounded-lg border border-slate-200 bg-white py-2.5 pr-3 pl-9 text-sm text-slate-900 placeholder:text-slate-400 focus:border-slate-400 focus:outline-none"
          />
        </div>

        <div className="mt-3 max-h-64 space-y-1 overflow-y-auto">
          {candidates.length === 0 ? (
            <p className="py-6 text-center text-sm text-slate-500">No matching {role.toLowerCase()}s found.</p>
          ) : (
            candidates.map((entry) => (
              <button
                key={entry.id}
                type="button"
                onClick={() => setSelectedUserId(entry.id)}
                className={`flex w-full items-center gap-3 rounded-lg border px-3 py-2 text-left text-sm ${
                  selectedUserId === entry.id
                    ? "border-slate-900 bg-slate-50"
                    : "border-transparent hover:bg-slate-50"
                }`}
              >
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-slate-900 text-xs font-semibold text-white">
                  {`${entry.firstName[0] ?? ""}${entry.lastName[0] ?? ""}`.toUpperCase()}
                </div>
                <span className="font-medium text-slate-900">
                  {entry.firstName} {entry.lastName}
                </span>
              </button>
            ))
          )}
        </div>

        {error && <p className="mt-3 text-sm text-red-600">{error}</p>}

        <div className="mt-4 flex justify-end">
          <button
            type="button"
            disabled={!selectedUserId || isSending}
            onClick={handleSend}
            className="rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
          >
            {isSending ? "Sending..." : role === "Mentor" ? "Send request" : "Send invite"}
          </button>
        </div>
      </div>
    </div>
  );
}
