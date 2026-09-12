import { useCallback, useEffect, useMemo, useState } from "react";
import { Search } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { getCategories, type Category } from "../lib/categoriesApi";
import { getMilestones, type ProjectMilestone } from "../lib/milestonesApi";
import { getAssignments, type ProjectAssignment } from "../lib/assignmentsApi";
import { getUserDirectory, type UserDirectoryEntry } from "../lib/usersApi";
import { paletteFor } from "../lib/colorPalette";
import {
  getProjects,
  projectStatusLabel,
  PROJECT_STATUSES,
  type Project,
  type ProjectStatus,
} from "../lib/projectsApi";
import { ProjectModal } from "./ProjectModal";

const ACTIVE_STATUS: ProjectStatus = 4; // "In Progress" is treated as the platform's "active" state.

const STATUS_BADGE: Record<ProjectStatus, string> = {
  1: "bg-slate-100 text-slate-700",
  2: "bg-amber-50 text-amber-700",
  3: "bg-sky-50 text-sky-700",
  4: "bg-emerald-50 text-emerald-700",
  5: "bg-purple-50 text-purple-700",
  6: "bg-blue-50 text-blue-700",
  7: "bg-red-50 text-red-700",
};

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

function initials(firstName: string, lastName: string): string {
  return `${firstName[0] ?? ""}${lastName[0] ?? ""}`.toUpperCase();
}

function displayName(entry: UserDirectoryEntry | undefined): string {
  return entry ? `${entry.firstName} ${entry.lastName}` : "Unknown";
}

function pillClass(active: boolean): string {
  return `rounded-full border px-3 py-1.5 text-xs font-medium ${
    active ? "border-slate-900 bg-slate-900 text-white" : "border-slate-200 bg-white text-slate-600 hover:border-slate-300"
  }`;
}

type ModalState = "closed" | "create" | Project;

export function ProjectsScreen() {
  const { token, user } = useAuth();
  const [projects, setProjects] = useState<Project[]>([]);
  const [milestones, setMilestones] = useState<ProjectMilestone[]>([]);
  const [assignments, setAssignments] = useState<ProjectAssignment[]>([]);
  const [directory, setDirectory] = useState<UserDirectoryEntry[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [modalState, setModalState] = useState<ModalState>("closed");

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<ProjectStatus | "all">("all");
  const [categoryFilter, setCategoryFilter] = useState<string>("all");

  const loadAll = useCallback(async () => {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setLoadError(null);

    try {
      const [projectsData, milestonesData, assignmentsData, directoryData, categoriesData] = await Promise.all([
        getProjects(token),
        getMilestones(token),
        getAssignments(token),
        getUserDirectory(token),
        getCategories(token),
      ]);
      setProjects(projectsData);
      setMilestones(milestonesData);
      setAssignments(assignmentsData);
      setDirectory(directoryData);
      setCategories(categoriesData);
    } catch {
      setLoadError("Could not load projects. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void loadAll();
  }, [loadAll]);

  function handleSaved() {
    setModalState("closed");
    void loadAll();
  }

  const directoryById = useMemo(() => {
    const map = new Map<string, UserDirectoryEntry>();
    for (const entry of directory) {
      map.set(entry.id, entry);
    }
    return map;
  }, [directory]);

  const milestonesByProject = useMemo(() => {
    const map = new Map<string, ProjectMilestone[]>();
    for (const milestone of milestones) {
      const list = map.get(milestone.projectId) ?? [];
      list.push(milestone);
      map.set(milestone.projectId, list);
    }
    return map;
  }, [milestones]);

  const assignmentsByProject = useMemo(() => {
    const map = new Map<string, ProjectAssignment[]>();
    for (const assignment of assignments) {
      const list = map.get(assignment.projectId) ?? [];
      list.push(assignment);
      map.set(assignment.projectId, list);
    }
    return map;
  }, [assignments]);

  const statusCounts = useMemo(() => {
    const counts = new Map<ProjectStatus, number>();
    for (const project of projects) {
      counts.set(project.status, (counts.get(project.status) ?? 0) + 1);
    }
    return counts;
  }, [projects]);

  const filteredProjects = useMemo(() => {
    const term = search.trim().toLowerCase();

    return projects.filter((project) => {
      if (statusFilter !== "all" && project.status !== statusFilter) {
        return false;
      }
      if (categoryFilter !== "all" && project.categoryId !== categoryFilter) {
        return false;
      }
      if (
        term &&
        !project.title.toLowerCase().includes(term) &&
        !(project.description ?? "").toLowerCase().includes(term)
      ) {
        return false;
      }
      return true;
    });
  }, [projects, search, statusFilter, categoryFilter]);

  function canManage(project: Project): boolean {
    return user?.role === "Administrator" || project.createdById === user?.id;
  }

  return (
    <div>
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="font-serif text-3xl font-bold text-slate-900">Projects</h1>
          <p className="mt-1 text-sm text-slate-500">
            {projects.length} total · {statusCounts.get(ACTIVE_STATUS) ?? 0} active
          </p>
        </div>

        <button
          type="button"
          onClick={() => setModalState("create")}
          disabled={categories.length === 0}
          title={categories.length === 0 ? "Add a category first" : undefined}
          className="shrink-0 rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
        >
          + New Project
        </button>
      </div>

      <div className="mt-6 flex flex-col gap-3 sm:flex-row">
        <div className="relative flex-[3]">
          <Search className="pointer-events-none absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search projects..."
            className="w-full rounded-lg border border-slate-200 bg-white py-2.5 pr-3 pl-9 text-sm text-slate-900 placeholder:text-slate-400 focus:border-slate-400 focus:outline-none"
          />
        </div>

        <select
          value={categoryFilter}
          onChange={(event) => setCategoryFilter(event.target.value)}
          className="flex-1 rounded-lg border border-slate-200 bg-white px-3 py-2.5 text-sm text-slate-700 focus:border-slate-400 focus:outline-none"
        >
          <option value="all">All Categories</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
      </div>

      <div className="mt-4 flex flex-wrap gap-2">
        <button type="button" onClick={() => setStatusFilter("all")} className={pillClass(statusFilter === "all")}>
          All {projects.length}
        </button>
        {PROJECT_STATUSES.map((option) => (
          <button
            key={option.value}
            type="button"
            onClick={() => setStatusFilter(option.value)}
            className={pillClass(statusFilter === option.value)}
          >
            {option.label} {statusCounts.get(option.value) ?? 0}
          </button>
        ))}
      </div>

      {loadError && <p className="mt-4 text-sm text-red-600">{loadError}</p>}

      <div className="mt-6">
        {isLoading ? (
          <p className="text-sm text-slate-500">Loading...</p>
        ) : projects.length === 0 ? (
          <p className="text-sm text-slate-500">No projects yet. Create the first one above.</p>
        ) : filteredProjects.length === 0 ? (
          <p className="text-sm text-slate-500">No projects match your filters.</p>
        ) : (
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
            {filteredProjects.map((project) => {
              const categoryPalette = paletteFor(project.categoryId);
              const projectMilestones = milestonesByProject.get(project.id) ?? [];
              const totalMilestones = projectMilestones.length;
              const completedMilestones = projectMilestones.filter((milestone) => milestone.status === 3).length;
              const progress = totalMilestones > 0 ? Math.round((completedMilestones / totalMilestones) * 100) : 0;
              const projectAssignments = assignmentsByProject.get(project.id) ?? [];
              const mentor = projectAssignments.find((assignment) => assignment.role === "Mentor");
              const teammates = projectAssignments.filter((assignment) => assignment.id !== mentor?.id);
              const updatedLabel = formatDate(project.updatedAt ?? project.createdAt);

              return (
                <div key={project.id} className="rounded-xl border border-slate-200 bg-white p-5">
                  <div className="flex items-center justify-between gap-2">
                    <span
                      className={`rounded-full px-2.5 py-1 text-xs font-medium ${categoryPalette.soft} ${categoryPalette.softText}`}
                    >
                      {project.categoryName}
                    </span>
                    <div className="flex items-center gap-3">
                      <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${STATUS_BADGE[project.status]}`}>
                        {projectStatusLabel(project.status)}
                      </span>
                      {canManage(project) && (
                        <button
                          type="button"
                          onClick={() => setModalState(project)}
                          className="text-xs font-medium text-slate-500 hover:text-slate-900"
                        >
                          Edit
                        </button>
                      )}
                    </div>
                  </div>

                  <h3 className="mt-3 text-lg font-semibold text-slate-900">{project.title}</h3>
                  {project.description && (
                    <p className="mt-1 line-clamp-2 text-sm text-slate-500">{project.description}</p>
                  )}

                  <div className="mt-4">
                    <div className="flex items-center gap-3">
                      <div className="h-2 flex-1 rounded-full bg-slate-100">
                        <div className="h-2 rounded-full bg-red-800" style={{ width: `${progress}%` }} />
                      </div>
                      <span className="text-xs text-slate-500">{progress}%</span>
                    </div>
                    <div className="mt-2 flex items-center justify-between text-xs text-slate-500">
                      <span>
                        {totalMilestones === 0
                          ? "No milestones yet"
                          : `${completedMilestones}/${totalMilestones} milestones`}
                      </span>
                      <span>Updated {updatedLabel}</span>
                    </div>
                  </div>

                  <div className="mt-4 flex items-center justify-between border-t border-slate-100 pt-4">
                    {mentor ? (
                      <div className="flex items-center gap-2">
                        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-slate-900 text-xs font-semibold text-white">
                          {initials(
                            directoryById.get(mentor.userId)?.firstName ?? "",
                            directoryById.get(mentor.userId)?.lastName ?? "",
                          )}
                        </div>
                        <div>
                          <p className="text-sm font-medium text-slate-900">
                            {displayName(directoryById.get(mentor.userId))}
                          </p>
                          <p className="text-xs text-slate-500">Mentor</p>
                        </div>
                      </div>
                    ) : (
                      <span className="text-xs text-slate-400">No mentor assigned</span>
                    )}

                    {teammates.length > 0 && (
                      <div className="flex -space-x-2">
                        {teammates.slice(0, 3).map((teammate) => (
                          <div
                            key={teammate.id}
                            title={displayName(directoryById.get(teammate.userId))}
                            className="flex h-7 w-7 items-center justify-center rounded-full border-2 border-white bg-stone-200 text-[10px] font-semibold text-stone-700"
                          >
                            {initials(
                              directoryById.get(teammate.userId)?.firstName ?? "",
                              directoryById.get(teammate.userId)?.lastName ?? "",
                            )}
                          </div>
                        ))}
                        {teammates.length > 3 && (
                          <div className="flex h-7 w-7 items-center justify-center rounded-full border-2 border-white bg-stone-300 text-[10px] font-semibold text-stone-700">
                            +{teammates.length - 3}
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {modalState !== "closed" && (
        <ProjectModal
          initial={modalState === "create" ? undefined : modalState}
          categories={categories}
          canDelete={modalState !== "create" && canManage(modalState)}
          onClose={() => setModalState("closed")}
          onSaved={handleSaved}
        />
      )}
    </div>
  );
}
