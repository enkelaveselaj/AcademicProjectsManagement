import { useEffect, useMemo, useState } from "react";
import { ArrowLeft, Check, Download } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { paletteFor } from "../lib/colorPalette";
import { fetchDocumentBlob, getDocuments, type DocumentFile } from "../lib/documentsApi";
import { FILE_CATEGORY_META, formatFileSize, getFileCategory } from "../lib/fileCategory";
import { MILESTONE_STATUS_META, type ProjectMilestone } from "../lib/milestonesApi";
import { MilestoneStatusIcon } from "../components/MilestoneStatusIcon";
import type { ProjectAssignment } from "../lib/assignmentsApi";
import type { UserDirectoryEntry } from "../lib/usersApi";
import { projectStatusLabel, PROJECT_STATUS_BADGE, type Project } from "../lib/projectsApi";

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });
}

function initials(firstName: string, lastName: string): string {
  return `${firstName[0] ?? ""}${lastName[0] ?? ""}`.toUpperCase();
}

function displayName(entry: UserDirectoryEntry | undefined): string {
  return entry ? `${entry.firstName} ${entry.lastName}` : "Unknown";
}

interface ProjectDetailScreenProps {
  project: Project;
  milestones: ProjectMilestone[];
  assignments: ProjectAssignment[];
  directory: UserDirectoryEntry[];
  onBack: () => void;
}

export function ProjectDetailScreen({ project, milestones, assignments, directory, onBack }: ProjectDetailScreenProps) {
  const { token } = useAuth();
  const [documents, setDocuments] = useState<DocumentFile[]>([]);
  const [isLoadingDocuments, setIsLoadingDocuments] = useState(true);
  const [documentError, setDocumentError] = useState<string | null>(null);
  const [busyDocumentId, setBusyDocumentId] = useState<string | null>(null);

  useEffect(() => {
    if (!token) {
      return;
    }

    let cancelled = false;
    setIsLoadingDocuments(true);

    getDocuments(token)
      .then((allDocuments) => {
        if (!cancelled) {
          setDocuments(allDocuments.filter((doc) => doc.projectId === project.id));
        }
      })
      .catch(() => {
        if (!cancelled) {
          setDocumentError("Could not load documents.");
        }
      })
      .finally(() => {
        if (!cancelled) {
          setIsLoadingDocuments(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [token, project.id]);

  const directoryById = useMemo(() => {
    const map = new Map<string, UserDirectoryEntry>();
    for (const entry of directory) {
      map.set(entry.id, entry);
    }
    return map;
  }, [directory]);

  const projectMilestones = useMemo(
    () => [...milestones].filter((m) => m.projectId === project.id).sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime()),
    [milestones, project.id],
  );

  const totalMilestones = projectMilestones.length;
  const completedMilestones = projectMilestones.filter((m) => m.status === 3).length;
  const progress = totalMilestones > 0 ? Math.round((completedMilestones / totalMilestones) * 100) : 0;

  const projectAssignments = useMemo(
    () => assignments.filter((assignment) => assignment.projectId === project.id),
    [assignments, project.id],
  );
  const mentor = projectAssignments.find((assignment) => assignment.role === "Mentor");
  const teammates = projectAssignments.filter((assignment) => assignment.id !== mentor?.id);

  async function handleView(doc: DocumentFile) {
    if (!token) {
      return;
    }
    setDocumentError(null);
    setBusyDocumentId(doc.id);
    try {
      const { blob } = await fetchDocumentBlob(doc.id, token);
      const url = URL.createObjectURL(blob);
      window.open(url, "_blank");
      setTimeout(() => URL.revokeObjectURL(url), 60_000);
    } catch {
      setDocumentError("Could not open the file.");
    } finally {
      setBusyDocumentId(null);
    }
  }

  async function handleDownload(doc: DocumentFile) {
    if (!token) {
      return;
    }
    setDocumentError(null);
    setBusyDocumentId(doc.id);
    try {
      const { blob, fileName } = await fetchDocumentBlob(doc.id, token);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName || doc.fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch {
      setDocumentError("Could not download the file.");
    } finally {
      setBusyDocumentId(null);
    }
  }

  const categoryPalette = paletteFor(project.categoryId);

  return (
    <div>
      <button
        type="button"
        onClick={onBack}
        className="flex items-center gap-1.5 text-sm font-medium text-slate-500 hover:text-slate-900"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Projects
      </button>

      <div className="mt-4 flex items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span
              className={`rounded-full px-2.5 py-1 text-xs font-medium ${categoryPalette.soft} ${categoryPalette.softText}`}
            >
              {project.categoryName}
            </span>
            <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${PROJECT_STATUS_BADGE[project.status]}`}>
              {projectStatusLabel(project.status)}
            </span>
          </div>
          <h1 className="mt-3 font-serif text-3xl font-bold text-slate-900">{project.title}</h1>
          {project.description && <p className="mt-2 max-w-2xl text-sm text-slate-600">{project.description}</p>}
        </div>
      </div>

      <div className="mt-6 rounded-xl border border-slate-200 bg-white p-5">
        <div className="flex items-center gap-3">
          <div className="h-2 flex-1 rounded-full bg-slate-100">
            <div className="h-2 rounded-full bg-red-800" style={{ width: `${progress}%` }} />
          </div>
          <span className="text-sm text-slate-500">{progress}%</span>
        </div>
        <div className="mt-2 flex items-center justify-between text-xs text-slate-500">
          <span>{totalMilestones === 0 ? "No milestones yet" : `${completedMilestones}/${totalMilestones} milestones`}</span>
          <span>Updated {formatDate(project.updatedAt ?? project.createdAt)}</span>
        </div>

        <div className="mt-5 flex items-center justify-between border-t border-slate-100 pt-4">
          {mentor ? (
            <div className="flex items-center gap-2">
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-slate-900 text-xs font-semibold text-white">
                {initials(
                  directoryById.get(mentor.userId)?.firstName ?? "",
                  directoryById.get(mentor.userId)?.lastName ?? "",
                )}
              </div>
              <div>
                <p className="text-sm font-medium text-slate-900">{displayName(directoryById.get(mentor.userId))}</p>
                <p className="text-xs text-slate-500">Mentor</p>
              </div>
            </div>
          ) : (
            <span className="text-xs text-slate-400">No mentor assigned</span>
          )}

          {teammates.length > 0 && (
            <div className="flex -space-x-2">
              {teammates.map((teammate) => (
                <div
                  key={teammate.id}
                  title={displayName(directoryById.get(teammate.userId))}
                  className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-white bg-stone-200 text-xs font-semibold text-stone-700"
                >
                  {initials(
                    directoryById.get(teammate.userId)?.firstName ?? "",
                    directoryById.get(teammate.userId)?.lastName ?? "",
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <div className="mt-8">
        <h2 className="font-serif text-lg font-bold text-slate-900">Milestones</h2>
        {projectMilestones.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No milestones yet.</p>
        ) : (
          <div className="mt-3 space-y-3">
            {projectMilestones.map((milestone) => {
              const meta = MILESTONE_STATUS_META[milestone.status];

              return (
                <div key={milestone.id} className="flex items-start gap-4 rounded-xl border border-slate-200 bg-white p-4">
                  <MilestoneStatusIcon status={milestone.status} />
                  <div className="min-w-0 flex-1">
                    <div className="flex items-start justify-between gap-4">
                      <h3 className="text-sm font-semibold text-slate-900">{milestone.title}</h3>
                      <span className={`shrink-0 rounded-full px-2.5 py-1 text-xs font-medium ${meta.badge}`}>
                        {meta.label}
                      </span>
                    </div>
                    {milestone.description && <p className="mt-1 text-sm text-slate-500">{milestone.description}</p>}
                    <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-500">
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
            })}
          </div>
        )}
      </div>

      <div className="mt-8">
        <h2 className="font-serif text-lg font-bold text-slate-900">Documents</h2>
        {documentError && <p className="mt-2 text-sm text-red-600">{documentError}</p>}
        {isLoadingDocuments ? (
          <p className="mt-3 text-sm text-slate-500">Loading...</p>
        ) : documents.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No documents yet.</p>
        ) : (
          <div className="mt-3 space-y-3">
            {documents.map((doc) => {
              const category = getFileCategory(doc.fileName);
              const meta = FILE_CATEGORY_META[category];
              const Icon = meta.icon;
              const isBusy = busyDocumentId === doc.id;

              return (
                <div key={doc.id} className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4">
                  <div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-lg ${meta.bg}`}>
                    <Icon className={`h-5 w-5 ${meta.text}`} />
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-semibold text-slate-900">{doc.fileName}</p>
                    <p className="text-xs text-slate-500">{formatFileSize(doc.fileSizeBytes)}</p>
                  </div>
                  <div className="flex shrink-0 items-center gap-2">
                    <button
                      type="button"
                      disabled={isBusy}
                      onClick={() => handleView(doc)}
                      className="rounded-lg border border-slate-200 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
                    >
                      View
                    </button>
                    <button
                      type="button"
                      disabled={isBusy}
                      title="Download"
                      onClick={() => handleDownload(doc)}
                      className="rounded-lg border border-slate-200 p-1.5 text-slate-600 hover:bg-slate-50 disabled:opacity-50"
                    >
                      <Download className="h-4 w-4" />
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
