import { useCallback, useEffect, useMemo, useState } from "react";
import { AlertTriangle, Check, Folder } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { getMilestones, type ProjectMilestone, type MilestoneStatus } from "../lib/milestonesApi";
import { getProjects, type Project } from "../lib/projectsApi";

const STATUS_META: Record<MilestoneStatus, { label: string; badge: string }> = {
  1: { label: "pending", badge: "bg-slate-100 text-slate-600" },
  2: { label: "in progress", badge: "bg-sky-50 text-sky-700" },
  3: { label: "completed", badge: "bg-emerald-50 text-emerald-700" },
  4: { label: "overdue", badge: "bg-red-50 text-red-700" },
};

const STAT_CARDS: { status: MilestoneStatus; label: string }[] = [
  { status: 2, label: "In Progress" },
  { status: 1, label: "Pending" },
  { status: 4, label: "Overdue" },
  { status: 3, label: "Completed" },
];

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });
}

function StatusIcon({ status }: { status: MilestoneStatus }) {
  if (status === 3) {
    return (
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-emerald-700 text-white">
        <Check className="h-4 w-4" />
      </div>
    );
  }
  if (status === 4) {
    return (
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-red-700 text-white">
        <AlertTriangle className="h-4 w-4" />
      </div>
    );
  }
  if (status === 2) {
    return <div className="h-8 w-8 shrink-0 rounded-full border-2 border-slate-900" />;
  }
  return <div className="h-8 w-8 shrink-0 rounded-full border-2 border-slate-300" />;
}

export function MilestonesScreen() {
  const { token } = useAuth();
  const [milestones, setMilestones] = useState<ProjectMilestone[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<MilestoneStatus | "all">("all");

  const loadAll = useCallback(async () => {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setLoadError(null);

    try {
      const [milestonesData, projectsData] = await Promise.all([getMilestones(token), getProjects(token)]);
      setMilestones(milestonesData);
      setProjects(projectsData);
    } catch {
      setLoadError("Could not load milestones. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void loadAll();
  }, [loadAll]);

  const projectTitleById = useMemo(() => {
    const map = new Map<string, string>();
    for (const project of projects) {
      map.set(project.id, project.title);
    }
    return map;
  }, [projects]);

  const statusCounts = useMemo(() => {
    const counts = new Map<MilestoneStatus, number>();
    for (const milestone of milestones) {
      counts.set(milestone.status, (counts.get(milestone.status) ?? 0) + 1);
    }
    return counts;
  }, [milestones]);

  const filteredMilestones = useMemo(() => {
    const list = statusFilter === "all" ? milestones : milestones.filter((m) => m.status === statusFilter);
    return [...list].sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime());
  }, [milestones, statusFilter]);

  const projectStats = useMemo(() => {
    const map = new Map<string, { total: number; completed: number; overdue: number }>();
    for (const milestone of milestones) {
      const entry = map.get(milestone.projectId) ?? { total: 0, completed: 0, overdue: 0 };
      entry.total += 1;
      if (milestone.status === 3) {
        entry.completed += 1;
      }
      if (milestone.status === 4) {
        entry.overdue += 1;
      }
      map.set(milestone.projectId, entry);
    }
    return map;
  }, [milestones]);

  const projectsWithMilestones = useMemo(
    () => projects.filter((project) => (projectStats.get(project.id)?.total ?? 0) > 0),
    [projects, projectStats],
  );

  const successfulProjects = projectsWithMilestones.filter((project) => {
    const stats = projectStats.get(project.id)!;
    return stats.completed === stats.total;
  }).length;

  const projectsAtRisk = projectsWithMilestones.filter(
    (project) => (projectStats.get(project.id)?.overdue ?? 0) > 0,
  ).length;

  const completedCount = statusCounts.get(3) ?? 0;
  const completionRate = milestones.length > 0 ? Math.round((completedCount / milestones.length) * 100) : 0;
  const avgPerProject =
    projectsWithMilestones.length > 0 ? (milestones.length / projectsWithMilestones.length).toFixed(1) : "0";

  return (
    <div>
      <h1 className="font-serif text-3xl font-bold text-slate-900">Milestones</h1>
      <p className="mt-1 text-sm text-slate-500">Track progress across every project's milestones.</p>

      {loadError && <p className="mt-4 text-sm text-red-600">{loadError}</p>}

      {isLoading ? (
        <p className="mt-6 text-sm text-slate-500">Loading...</p>
      ) : (
        <>
          <div className="mt-6">
            <h2 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">Project Outcomes</h2>
            <div className="mt-3 grid grid-cols-2 gap-4 sm:grid-cols-4">
              <div className="rounded-xl border border-slate-200 bg-white p-5 text-center">
                <p className="font-serif text-3xl font-bold text-emerald-700">{successfulProjects}</p>
                <p className="mt-1 text-sm text-slate-500">Successful projects</p>
              </div>
              <div className="rounded-xl border border-slate-200 bg-white p-5 text-center">
                <p className="font-serif text-3xl font-bold text-red-800">{projectsAtRisk}</p>
                <p className="mt-1 text-sm text-slate-500">Projects at risk</p>
              </div>
              <div className="rounded-xl border border-slate-200 bg-white p-5 text-center">
                <p className="font-serif text-3xl font-bold text-slate-900">{completionRate}%</p>
                <p className="mt-1 text-sm text-slate-500">Completion rate</p>
              </div>
              <div className="rounded-xl border border-slate-200 bg-white p-5 text-center">
                <p className="font-serif text-3xl font-bold text-slate-900">{avgPerProject}</p>
                <p className="mt-1 text-sm text-slate-500">Avg. milestones / project</p>
              </div>
            </div>
          </div>

          <div className="mt-6 flex flex-wrap gap-2">
            <button
              type="button"
              onClick={() => setStatusFilter("all")}
              className={`rounded-full border px-3 py-1.5 text-xs font-medium ${
                statusFilter === "all"
                  ? "border-slate-900 bg-slate-900 text-white"
                  : "border-slate-200 bg-white text-slate-600 hover:border-slate-300"
              }`}
            >
              All {milestones.length}
            </button>
            {STAT_CARDS.map((card) => (
              <button
                key={card.status}
                type="button"
                onClick={() => setStatusFilter(card.status)}
                className={`rounded-full border px-3 py-1.5 text-xs font-medium ${
                  statusFilter === card.status
                    ? "border-slate-900 bg-slate-900 text-white"
                    : "border-slate-200 bg-white text-slate-600 hover:border-slate-300"
                }`}
              >
                {card.label} {statusCounts.get(card.status) ?? 0}
              </button>
            ))}
          </div>

          <div className="mt-6 space-y-3">
            {milestones.length === 0 ? (
              <p className="text-sm text-slate-500">No milestones yet.</p>
            ) : filteredMilestones.length === 0 ? (
              <p className="text-sm text-slate-500">No milestones match this filter.</p>
            ) : (
              filteredMilestones.map((milestone) => {
                const meta = STATUS_META[milestone.status];

                return (
                  <div
                    key={milestone.id}
                    className="flex items-start gap-4 rounded-xl border border-slate-200 bg-white p-4"
                  >
                    <StatusIcon status={milestone.status} />

                    <div className="min-w-0 flex-1">
                      <div className="flex items-start justify-between gap-4">
                        <h3 className="text-sm font-semibold text-slate-900">{milestone.title}</h3>
                        <span className={`shrink-0 rounded-full px-2.5 py-1 text-xs font-medium ${meta.badge}`}>
                          {meta.label}
                        </span>
                      </div>

                      {milestone.description && (
                        <p className="mt-1 text-sm text-slate-500">{milestone.description}</p>
                      )}

                      <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-500">
                        <span className="flex items-center gap-1">
                          <Folder className="h-3.5 w-3.5 text-amber-600" />
                          {projectTitleById.get(milestone.projectId) ?? "Unknown project"}
                        </span>
                        <span>Due: {formatDate(milestone.dueDate)}</span>
                        {milestone.completedAt && (
                          <span className="flex items-center gap-1 text-emerald-700">
                            <Check className="h-3.5 w-3.5" />
                            {formatDate(milestone.completedAt)}
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </>
      )}
    </div>
  );
}
