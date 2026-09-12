import { useCallback, useEffect, useMemo, useState } from "react";
import { AlertTriangle, CheckCircle2, Info, XCircle } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import type { Screen } from "../components/Layout";
import { getCategories, type Category } from "../lib/categoriesApi";
import { getMilestones, type ProjectMilestone } from "../lib/milestonesApi";
import { getNotifications, type Notification, type NotificationType } from "../lib/notificationsApi";
import { getPendingUsers, getUsers, type PendingUser, type UserSummary } from "../lib/usersApi";
import { paletteFor } from "../lib/colorPalette";
import {
  getProjects,
  projectStatusLabel,
  PROJECT_STATUSES,
  PROJECT_STATUS_BADGE,
  type Project,
  type ProjectStatus,
} from "../lib/projectsApi";

const ACTIVE_STATUS: ProjectStatus = 4;

const NOTIFICATION_ICON: Record<NotificationType, { icon: typeof Info; bg: string; text: string }> = {
  1: { icon: Info, bg: "bg-sky-50", text: "text-sky-600" },
  2: { icon: AlertTriangle, bg: "bg-amber-50", text: "text-amber-700" },
  3: { icon: CheckCircle2, bg: "bg-emerald-50", text: "text-emerald-700" },
  4: { icon: XCircle, bg: "bg-red-50", text: "text-red-700" },
};

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });
}

interface StatCardProps {
  value: string | number;
  label: string;
  textClass?: string;
}

function StatCard({ value, label, textClass = "text-slate-900" }: StatCardProps) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-5 text-center">
      <p className={`font-serif text-3xl font-bold ${textClass}`}>{value}</p>
      <p className="mt-1 text-sm text-slate-500">{label}</p>
    </div>
  );
}

interface SectionHeaderProps {
  title: string;
  onViewAll?: () => void;
}

function SectionHeader({ title, onViewAll }: SectionHeaderProps) {
  return (
    <div className="flex items-center justify-between">
      <h2 className="font-serif text-lg font-bold text-slate-900">{title}</h2>
      {onViewAll && (
        <button type="button" onClick={onViewAll} className="text-sm font-medium text-red-800 hover:text-red-900">
          View all →
        </button>
      )}
    </div>
  );
}

interface DashboardScreenProps {
  onNavigate: (screen: Screen) => void;
}

export function DashboardScreen({ onNavigate }: DashboardScreenProps) {
  const { token, user } = useAuth();
  const isAdmin = user?.role === "Administrator";

  const [projects, setProjects] = useState<Project[]>([]);
  const [milestones, setMilestones] = useState<ProjectMilestone[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [pendingUsers, setPendingUsers] = useState<PendingUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const loadAll = useCallback(async () => {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setLoadError(null);

    try {
      const [projectsData, milestonesData, categoriesData, notificationsData] = await Promise.all([
        getProjects(token),
        getMilestones(token),
        getCategories(token),
        getNotifications(token),
      ]);
      setProjects(projectsData);
      setMilestones(milestonesData);
      setCategories(categoriesData);
      setNotifications(notificationsData);

      if (isAdmin) {
        const [usersData, pendingData] = await Promise.all([getUsers(token), getPendingUsers(token)]);
        setUsers(usersData);
        setPendingUsers(pendingData);
      }
    } catch {
      setLoadError("Could not load dashboard data. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [token, isAdmin]);

  useEffect(() => {
    void loadAll();
  }, [loadAll]);

  const statusCounts = useMemo(() => {
    const counts = new Map<ProjectStatus, number>();
    for (const project of projects) {
      counts.set(project.status, (counts.get(project.status) ?? 0) + 1);
    }
    return counts;
  }, [projects]);

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

  const completedMilestones = milestones.filter((milestone) => milestone.status === 3).length;
  const completionRate = milestones.length > 0 ? Math.round((completedMilestones / milestones.length) * 100) : 0;

  const unreadNotifications = notifications.filter((notification) => !notification.isRead).length;

  const recentNotifications = useMemo(
    () =>
      [...notifications]
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 4),
    [notifications],
  );

  const roleCounts = useMemo(() => {
    const counts = new Map<string, number>();
    for (const summary of users) {
      counts.set(summary.role, (counts.get(summary.role) ?? 0) + 1);
    }
    return counts;
  }, [users]);

  const maxCategoryCount = Math.max(1, ...categories.map((category) => category.projectCount));

  if (isLoading) {
    return (
      <div>
        <h1 className="font-serif text-3xl font-bold text-slate-900">Dashboard</h1>
        <p className="mt-6 text-sm text-slate-500">Loading...</p>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="font-serif text-3xl font-bold text-slate-900">
          Welcome back, {user?.firstName}.
        </h1>
        <p className="mt-1 text-sm text-slate-500">Here's what's happening across the platform.</p>
      </div>

      {loadError && <p className="text-sm text-red-600">{loadError}</p>}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <StatCard value={projects.length} label="Total Projects" />
        <StatCard value={statusCounts.get(ACTIVE_STATUS) ?? 0} label="Active Projects" textClass="text-emerald-700" />
        <StatCard value={milestones.length} label="Total Milestones" />
        <StatCard
          value={unreadNotifications}
          label="Unread Notifications"
          textClass={unreadNotifications > 0 ? "text-red-800" : "text-slate-900"}
        />
      </div>

      <div>
        <SectionHeader title="Projects by Status" onViewAll={() => onNavigate("projects")} />
        {projects.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No projects yet.</p>
        ) : (
          <div className="mt-3 flex flex-wrap gap-2">
            {PROJECT_STATUSES.filter((option) => (statusCounts.get(option.value) ?? 0) > 0).map((option) => (
              <span
                key={option.value}
                className={`rounded-full px-3 py-1.5 text-sm font-medium ${PROJECT_STATUS_BADGE[option.value]}`}
              >
                {projectStatusLabel(option.value)} · {statusCounts.get(option.value)}
              </span>
            ))}
          </div>
        )}
      </div>

      {isAdmin && (
        <div>
          <SectionHeader title="User Management" onViewAll={() => onNavigate("userManagement")} />
          <div className="mt-3 grid grid-cols-2 gap-4 sm:grid-cols-4">
            <StatCard value={users.length} label="Total Users" />
            <StatCard value={roleCounts.get("Mentor") ?? 0} label="Mentors" />
            <StatCard value={roleCounts.get("Student") ?? 0} label="Students" />
            <StatCard
              value={pendingUsers.length}
              label="Pending Approvals"
              textClass={pendingUsers.length > 0 ? "text-amber-700" : "text-slate-900"}
            />
          </div>
        </div>
      )}

      <div>
        <SectionHeader title="Milestone Outcomes" onViewAll={() => onNavigate("milestones")} />
        {milestones.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No milestones yet.</p>
        ) : (
          <div className="mt-3 grid grid-cols-2 gap-4 sm:grid-cols-4">
            <StatCard value={successfulProjects} label="Successful projects" textClass="text-emerald-700" />
            <StatCard value={projectsAtRisk} label="Projects at risk" textClass="text-red-800" />
            <StatCard value={`${completionRate}%`} label="Completion rate" />
            <StatCard value={completedMilestones} label="Completed milestones" />
          </div>
        )}
      </div>

      <div>
        <SectionHeader title="Categories" onViewAll={isAdmin ? () => onNavigate("categories") : undefined} />
        {categories.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No categories yet.</p>
        ) : (
          <div className="mt-3 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {categories.map((category) => {
              const palette = paletteFor(category.id);
              const barWidth = Math.max(4, Math.round((category.projectCount / maxCategoryCount) * 100));

              return (
                <div key={category.id} className="rounded-xl border border-slate-200 bg-white p-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <div
                        className={`flex h-7 w-7 items-center justify-center rounded-lg font-serif text-xs font-bold text-white ${palette.bg}`}
                      >
                        {category.name[0]?.toUpperCase()}
                      </div>
                      <p className="text-sm font-medium text-slate-900">{category.name}</p>
                    </div>
                    <span className={`font-serif text-lg font-bold ${palette.text}`}>{category.projectCount}</span>
                  </div>
                  <div className="mt-3 h-1.5 w-full rounded-full bg-slate-100">
                    <div className={`h-1.5 rounded-full ${palette.bar}`} style={{ width: `${barWidth}%` }} />
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      <div>
        <SectionHeader title="Recent Notifications" onViewAll={() => onNavigate("notifications")} />
        {recentNotifications.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No notifications yet.</p>
        ) : (
          <div className="mt-3 space-y-3">
            {recentNotifications.map((notification) => {
              const meta = NOTIFICATION_ICON[notification.type];
              const Icon = meta.icon;

              return (
                <div
                  key={notification.id}
                  className={`flex items-center gap-4 rounded-xl border bg-white p-4 ${
                    notification.isRead ? "border-slate-200" : "border-l-4 border-l-red-800 border-slate-200"
                  }`}
                >
                  <div className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg ${meta.bg}`}>
                    <Icon className={`h-4 w-4 ${meta.text}`} />
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium text-slate-900">{notification.message}</p>
                    <p className="text-xs text-slate-400">{formatDate(notification.createdAt)}</p>
                  </div>
                  {!notification.isRead && <span className="h-2 w-2 shrink-0 rounded-full bg-red-800" />}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
