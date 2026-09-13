import { useCallback, useEffect, useMemo, useState } from "react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { acceptInvitation, declineInvitation, getInvitations, type ProjectInvitation } from "../lib/invitationsApi";
import { getProjects, type Project } from "../lib/projectsApi";

export function MyInvitationsPanel() {
  const { token, user } = useAuth();
  const [invitations, setInvitations] = useState<ProjectInvitation[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const load = useCallback(() => {
    if (!token) {
      return;
    }

    Promise.all([getInvitations(token), getProjects(token)])
      .then(([invitationsData, projectsData]) => {
        setInvitations(invitationsData);
        setProjects(projectsData);
      })
      .catch(() => {
        // This panel is a convenience surface, not the source of truth - fail silently.
      });
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  const projectsById = useMemo(() => {
    const map = new Map<string, Project>();
    for (const project of projects) {
      map.set(project.id, project);
    }
    return map;
  }, [projects]);

  const myInvitations = useMemo(
    () => invitations.filter((invitation) => invitation.status === 1 && invitation.invitedUserId === user?.id),
    [invitations, user],
  );

  async function respond(invitationId: string, accept: boolean) {
    if (!token) {
      return;
    }
    setError(null);
    setBusyId(invitationId);
    try {
      if (accept) {
        await acceptInvitation(invitationId, token);
      } else {
        await declineInvitation(invitationId, token);
      }
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not respond to the invitation.");
    } finally {
      setBusyId(null);
    }
  }

  if (myInvitations.length === 0) {
    return null;
  }

  return (
    <div>
      <h2 className="font-serif text-lg font-bold text-slate-900">My Invitations</h2>
      {error && <p className="mt-2 text-sm text-red-600">{error}</p>}
      <div className="mt-3 space-y-3">
        {myInvitations.map((invitation) => {
          const project = projectsById.get(invitation.projectId);
          const isBusy = busyId === invitation.id;

          return (
            <div
              key={invitation.id}
              className="flex items-center justify-between gap-4 rounded-xl border border-slate-200 bg-white p-4"
            >
              <div>
                <p className="text-sm font-medium text-slate-900">
                  {project?.title ?? "A project"} invited you as {invitation.role}
                </p>
                <p className="text-xs text-slate-500">Awaiting your response</p>
              </div>
              <div className="flex shrink-0 gap-2">
                <button
                  type="button"
                  disabled={isBusy}
                  onClick={() => respond(invitation.id, true)}
                  className="rounded-lg bg-slate-900 px-3 py-1.5 text-xs font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
                >
                  Accept
                </button>
                <button
                  type="button"
                  disabled={isBusy}
                  onClick={() => respond(invitation.id, false)}
                  className="rounded-lg border border-slate-200 px-3 py-1.5 text-xs font-medium text-slate-600 hover:bg-slate-50 disabled:opacity-50"
                >
                  Decline
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
