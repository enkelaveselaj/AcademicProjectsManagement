import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import {
  approveUser,
  changeUserRole,
  getPendingUsers,
  getUsers,
  rejectUser,
  type PendingUser,
  type UserSummary,
} from "../lib/usersApi";

const ROLE_OPTIONS = ["Student", "Mentor", "Administrator"] as const;

function initials(firstName: string, lastName: string): string {
  return `${firstName[0] ?? ""}${lastName[0] ?? ""}`.toUpperCase();
}

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

export function UserManagementScreen() {
  const { token } = useAuth();
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [pendingUsers, setPendingUsers] = useState<PendingUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [actionErrors, setActionErrors] = useState<Record<string, string>>({});
  const [busyIds, setBusyIds] = useState<Set<string>>(new Set());

  const loadData = useCallback(async () => {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setLoadError(null);

    try {
      const [allUsers, pending] = await Promise.all([getUsers(token), getPendingUsers(token)]);
      setUsers(allUsers);
      setPendingUsers(pending);
    } catch {
      setLoadError("Could not load users. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  function setBusy(id: string, busy: boolean) {
    setBusyIds((previous) => {
      const next = new Set(previous);
      if (busy) {
        next.add(id);
      } else {
        next.delete(id);
      }
      return next;
    });
  }

  function setActionError(id: string, message: string) {
    setActionErrors((previous) => ({ ...previous, [id]: message }));
  }

  async function handleApprove(id: string) {
    if (!token) {
      return;
    }

    setBusy(id, true);
    setActionError(id, "");

    try {
      await approveUser(id, token);
      await loadData();
    } catch (err) {
      setActionError(id, err instanceof ApiError ? err.message : "Something went wrong.");
    } finally {
      setBusy(id, false);
    }
  }

  async function handleReject(id: string) {
    if (!token) {
      return;
    }

    setBusy(id, true);
    setActionError(id, "");

    try {
      await rejectUser(id, token);
      await loadData();
    } catch (err) {
      setActionError(id, err instanceof ApiError ? err.message : "Something went wrong.");
    } finally {
      setBusy(id, false);
    }
  }

  async function handleRoleChange(id: string, role: string) {
    if (!token) {
      return;
    }

    setBusy(id, true);
    setActionError(id, "");

    try {
      await changeUserRole(id, role, token);
      await loadData();
    } catch (err) {
      const message = err instanceof ApiError ? (err.errors?.role?.[0] ?? err.message) : "Something went wrong.";
      setActionError(id, message);
    } finally {
      setBusy(id, false);
    }
  }

  return (
    <div>
      <h1 className="font-serif text-3xl font-bold text-slate-900">User Management</h1>
      <p className="mt-1 text-sm text-slate-500">Review pending requests and manage existing accounts.</p>

      {loadError && <p className="mt-4 text-sm text-red-600">{loadError}</p>}

      {isLoading ? (
        <p className="mt-6 text-sm text-slate-500">Loading...</p>
      ) : (
        <>
          <section className="mt-8">
            <h2 className="text-lg font-semibold text-slate-900">
              Pending Approvals {pendingUsers.length > 0 && `(${pendingUsers.length})`}
            </h2>

            {pendingUsers.length === 0 ? (
              <p className="mt-3 text-sm text-slate-500">No pending requests.</p>
            ) : (
              <div className="mt-3 space-y-3">
                {pendingUsers.map((pending) => (
                  <div key={pending.id} className="rounded-lg border border-slate-200 bg-white p-4">
                    <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-start">
                      <div className="flex items-start gap-3">
                        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-red-800 text-sm font-semibold text-white">
                          {initials(pending.firstName, pending.lastName)}
                        </div>
                        <div>
                          <p className="text-sm font-semibold text-slate-900">
                            {pending.firstName} {pending.lastName}
                            <span className="ml-2 rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700">
                              {pending.requestedRole}
                            </span>
                          </p>
                          <p className="font-mono text-xs text-slate-500">{pending.email}</p>
                          <dl className="mt-2 grid grid-cols-2 gap-x-6 gap-y-1 text-xs text-slate-500 sm:grid-cols-4">
                            <div>
                              <dt className="text-slate-400">Date of birth</dt>
                              <dd>{formatDate(pending.dateOfBirth)}</dd>
                            </div>
                            <div>
                              <dt className="text-slate-400">Personal ID</dt>
                              <dd>{pending.personalIdNumber}</dd>
                            </div>
                            {pending.studentId && (
                              <div>
                                <dt className="text-slate-400">Student ID</dt>
                                <dd>{pending.studentId}</dd>
                              </div>
                            )}
                            <div>
                              <dt className="text-slate-400">Requested</dt>
                              <dd>{formatDate(pending.requestedAt)}</dd>
                            </div>
                          </dl>
                        </div>
                      </div>

                      <div className="flex shrink-0 gap-2">
                        <button
                          type="button"
                          disabled={busyIds.has(pending.id)}
                          onClick={() => handleApprove(pending.id)}
                          className="rounded-lg bg-slate-900 px-3 py-1.5 text-xs font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
                        >
                          Approve
                        </button>
                        <button
                          type="button"
                          disabled={busyIds.has(pending.id)}
                          onClick={() => handleReject(pending.id)}
                          className="rounded-lg border border-red-200 px-3 py-1.5 text-xs font-semibold text-red-700 hover:bg-red-50 disabled:opacity-50"
                        >
                          Reject
                        </button>
                      </div>
                    </div>

                    {actionErrors[pending.id] && (
                      <p className="mt-2 text-xs text-red-600">{actionErrors[pending.id]}</p>
                    )}
                  </div>
                ))}
              </div>
            )}
          </section>

          <section className="mt-10">
            <h2 className="text-lg font-semibold text-slate-900">All Users ({users.length})</h2>

            <div className="mt-3 divide-y divide-slate-100 overflow-hidden rounded-lg border border-slate-200 bg-white">
              {users.map((user) => (
                <div key={user.id} className="px-4 py-3">
                  <div className="flex items-center justify-between gap-4">
                    <div className="flex items-center gap-3">
                      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-slate-800 text-xs font-semibold text-white">
                        {initials(user.firstName, user.lastName)}
                      </div>
                      <div>
                        <p className="text-sm font-medium text-slate-900">
                          {user.firstName} {user.lastName}
                        </p>
                        <p className="text-xs text-slate-500">{user.email}</p>
                      </div>
                    </div>

                    <select
                      value={user.role}
                      disabled={busyIds.has(user.id)}
                      onChange={(event) => handleRoleChange(user.id, event.target.value)}
                      className="rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-xs font-medium text-slate-700 focus:border-slate-400 focus:outline-none disabled:opacity-50"
                    >
                      {ROLE_OPTIONS.map((role) => (
                        <option key={role} value={role}>
                          {role}
                        </option>
                      ))}
                    </select>
                  </div>

                  {actionErrors[user.id] && <p className="mt-2 text-xs text-red-600">{actionErrors[user.id]}</p>}
                </div>
              ))}
            </div>
          </section>
        </>
      )}
    </div>
  );
}
