import { useCallback, useEffect, useMemo, useState } from "react";
import { Download, Folder, Search, Trash2 } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { deleteDocument, fetchDocumentBlob, getDocuments, type DocumentFile } from "../lib/documentsApi";
import { getProjects, type Project } from "../lib/projectsApi";
import { getAssignments, type ProjectAssignment } from "../lib/assignmentsApi";
import { getUserDirectory, type UserDirectoryEntry } from "../lib/usersApi";
import { FILE_CATEGORY_META, formatFileSize, getFileCategory, type FileCategory } from "../lib/fileCategory";
import { UploadDocumentModal } from "./UploadDocumentModal";

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });
}

function openBlobInNewTab(blob: Blob) {
  const url = URL.createObjectURL(blob);
  window.open(url, "_blank");
  setTimeout(() => URL.revokeObjectURL(url), 60_000);
}

function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

export function DocumentsScreen() {
  const { token, user } = useAuth();
  const [documents, setDocuments] = useState<DocumentFile[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [assignments, setAssignments] = useState<ProjectAssignment[]>([]);
  const [directory, setDirectory] = useState<UserDirectoryEntry[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [isUploadOpen, setIsUploadOpen] = useState(false);
  const [busyIds, setBusyIds] = useState<Set<string>>(new Set());

  const loadAll = useCallback(async () => {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setLoadError(null);

    try {
      const [documentsData, projectsData, assignmentsData, directoryData] = await Promise.all([
        getDocuments(token),
        getProjects(token),
        getAssignments(token),
        getUserDirectory(token),
      ]);
      setDocuments(documentsData);
      setProjects(projectsData);
      setAssignments(assignmentsData);
      setDirectory(directoryData);
    } catch {
      setLoadError("Could not load documents. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void loadAll();
  }, [loadAll]);

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

  const projectTitleById = useMemo(() => {
    const map = new Map<string, string>();
    for (const project of projects) {
      map.set(project.id, project.title);
    }
    return map;
  }, [projects]);

  const directoryById = useMemo(() => {
    const map = new Map<string, UserDirectoryEntry>();
    for (const entry of directory) {
      map.set(entry.id, entry);
    }
    return map;
  }, [directory]);

  const myProjectIds = useMemo(
    () => new Set(assignments.filter((assignment) => assignment.userId === user?.id).map((a) => a.projectId)),
    [assignments, user],
  );

  function canManage(doc: DocumentFile): boolean {
    return user?.role === "Administrator" || doc.uploadedById === user?.id || myProjectIds.has(doc.projectId);
  }

  const categoryCounts = useMemo(() => {
    const counts = new Map<FileCategory, number>();
    for (const doc of documents) {
      const category = getFileCategory(doc.fileName);
      counts.set(category, (counts.get(category) ?? 0) + 1);
    }
    return counts;
  }, [documents]);

  const totalBytes = documents.reduce((sum, doc) => sum + doc.fileSizeBytes, 0);

  const filteredDocuments = useMemo(() => {
    const term = search.trim().toLowerCase();
    const list = term ? documents.filter((doc) => doc.fileName.toLowerCase().includes(term)) : documents;
    return [...list].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }, [documents, search]);

  async function handleView(doc: DocumentFile) {
    if (!token) {
      return;
    }
    setActionError(null);
    setBusy(doc.id, true);
    try {
      const { blob } = await fetchDocumentBlob(doc.id, token);
      openBlobInNewTab(blob);
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : "Could not open the file.");
    } finally {
      setBusy(doc.id, false);
    }
  }

  async function handleDownload(doc: DocumentFile) {
    if (!token) {
      return;
    }
    setActionError(null);
    setBusy(doc.id, true);
    try {
      const { blob, fileName } = await fetchDocumentBlob(doc.id, token);
      downloadBlob(blob, fileName || doc.fileName);
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : "Could not download the file.");
    } finally {
      setBusy(doc.id, false);
    }
  }

  async function handleDelete(doc: DocumentFile) {
    if (!token) {
      return;
    }
    if (!window.confirm(`Delete "${doc.fileName}"? This cannot be undone.`)) {
      return;
    }
    setActionError(null);
    setBusy(doc.id, true);
    try {
      await deleteDocument(doc.id, token);
      await loadAll();
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : "Could not delete the file.");
    } finally {
      setBusy(doc.id, false);
    }
  }

  function handleUploaded() {
    setIsUploadOpen(false);
    void loadAll();
  }

  return (
    <div>
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="font-serif text-3xl font-bold text-slate-900">Documents</h1>
          <p className="mt-1 text-sm text-slate-500">
            {documents.length} files · {formatFileSize(totalBytes)} total
          </p>
        </div>

        <button
          type="button"
          onClick={() => setIsUploadOpen(true)}
          disabled={projects.length === 0}
          title={projects.length === 0 ? "Create a project first" : undefined}
          className="shrink-0 rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
        >
          + Upload File
        </button>
      </div>

      <div className="relative mt-6">
        <Search className="pointer-events-none absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-slate-400" />
        <input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search documents..."
          className="w-full rounded-lg border border-slate-200 bg-white py-2.5 pr-3 pl-9 text-sm text-slate-900 placeholder:text-slate-400 focus:border-slate-400 focus:outline-none"
        />
      </div>

      {loadError && <p className="mt-4 text-sm text-red-600">{loadError}</p>}
      {actionError && <p className="mt-4 text-sm text-red-600">{actionError}</p>}

      {isLoading ? (
        <p className="mt-6 text-sm text-slate-500">Loading...</p>
      ) : (
        <>
          {categoryCounts.size > 0 && (
            <div className="mt-6 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
              {[...categoryCounts.entries()].map(([category, count]) => {
                const meta = FILE_CATEGORY_META[category];
                const Icon = meta.icon;

                return (
                  <div key={category} className="rounded-xl border border-slate-200 bg-white p-4 text-center">
                    <div className={`mx-auto flex h-10 w-10 items-center justify-center rounded-lg ${meta.bg}`}>
                      <Icon className={`h-5 w-5 ${meta.text}`} />
                    </div>
                    <p className="mt-2 font-serif text-2xl font-bold text-slate-900">{count}</p>
                    <p className="text-xs text-slate-500">{meta.label}</p>
                  </div>
                );
              })}
            </div>
          )}

          <div className="mt-6 space-y-3">
            {documents.length === 0 ? (
              <p className="text-sm text-slate-500">No documents yet. Upload the first one above.</p>
            ) : filteredDocuments.length === 0 ? (
              <p className="text-sm text-slate-500">No documents match your search.</p>
            ) : (
              filteredDocuments.map((doc) => {
                const category = getFileCategory(doc.fileName);
                const meta = FILE_CATEGORY_META[category];
                const Icon = meta.icon;
                const uploader = directoryById.get(doc.uploadedById);
                const isBusy = busyIds.has(doc.id);

                return (
                  <div
                    key={doc.id}
                    className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4"
                  >
                    <div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-lg ${meta.bg}`}>
                      <Icon className={`h-5 w-5 ${meta.text}`} />
                    </div>

                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-semibold text-slate-900">{doc.fileName}</p>
                      <div className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-500">
                        <span className="flex items-center gap-1">
                          <Folder className="h-3.5 w-3.5 text-amber-600" />
                          {projectTitleById.get(doc.projectId) ?? "Unknown project"}
                        </span>
                        <span>{formatFileSize(doc.fileSizeBytes)}</span>
                        <span>by {uploader ? `${uploader.firstName} ${uploader.lastName}` : "Unknown"}</span>
                        <span>{formatDate(doc.createdAt)}</span>
                      </div>
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
                      {canManage(doc) && (
                        <button
                          type="button"
                          disabled={isBusy}
                          title="Delete"
                          onClick={() => handleDelete(doc)}
                          className="rounded-lg border border-slate-200 p-1.5 text-slate-500 hover:bg-red-50 hover:text-red-700 disabled:opacity-50"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      )}
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </>
      )}

      {isUploadOpen && (
        <UploadDocumentModal projects={projects} onClose={() => setIsUploadOpen(false)} onUploaded={handleUploaded} />
      )}
    </div>
  );
}
