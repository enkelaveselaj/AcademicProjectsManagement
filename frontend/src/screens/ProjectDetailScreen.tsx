import { useCallback, useEffect, useMemo, useState } from "react";
import { Check, Download, Plus, Trash2, UserPlus, X } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { paletteFor } from "../lib/colorPalette";
import { createComment, deleteComment, getComments, type Comment } from "../lib/commentsApi";
import { fetchDocumentBlob, getDocuments, type DocumentFile } from "../lib/documentsApi";
import { UploadDocumentModal } from "./UploadDocumentModal";
import { FILE_CATEGORY_META, formatFileSize, getFileCategory } from "../lib/fileCategory";
import { cancelInvitation, getInvitations, type ProjectInvitation } from "../lib/invitationsApi";
import { InviteMemberModal } from "./InviteMemberModal";
import { MILESTONE_STATUS_META, type ProjectMilestone } from "../lib/milestonesApi";
import { MilestoneModal } from "./MilestoneModal";
import { MilestoneStatusIcon } from "../components/MilestoneStatusIcon";
import { ChangeStatusModal } from "./ChangeStatusModal";
import { getStatusHistories, type ProjectStatusHistoryEntry } from "../lib/statusHistoriesApi";
import type { ProjectAssignment } from "../lib/assignmentsApi";
import type { UserDirectoryEntry } from "../lib/usersApi";
import { projectStatusLabel, PROJECT_STATUS_BADGE, type Project } from "../lib/projectsApi";

const SUBMITTED_STATUS = 2;

type TabKey = "overview" | "milestones" | "documents" | "comments" | "status";

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });
}

function formatDateTime(value: string): string {
  return new Date(value).toLocaleString(undefined, {
    day: "numeric",
    month: "short",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });
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
  onRefresh: () => void;
}

export function ProjectDetailScreen({
  project,
  milestones,
  assignments,
  directory,
  onBack,
  onRefresh,
}: ProjectDetailScreenProps) {
  const { token, user } = useAuth();
  const [activeTab, setActiveTab] = useState<TabKey>("overview");

  const [documents, setDocuments] = useState<DocumentFile[]>([]);
  const [isLoadingDocuments, setIsLoadingDocuments] = useState(true);
  const [documentError, setDocumentError] = useState<string | null>(null);
  const [busyDocumentId, setBusyDocumentId] = useState<string | null>(null);

  const [invitations, setInvitations] = useState<ProjectInvitation[]>([]);
  const [invitationError, setInvitationError] = useState<string | null>(null);
  const [inviteModal, setInviteModal] = useState<"Student" | "Mentor" | null>(null);
  const [busyInvitationId, setBusyInvitationId] = useState<string | null>(null);

  const [comments, setComments] = useState<Comment[]>([]);
  const [commentError, setCommentError] = useState<string | null>(null);
  const [newComment, setNewComment] = useState("");
  const [isPostingComment, setIsPostingComment] = useState(false);
  const [busyCommentId, setBusyCommentId] = useState<string | null>(null);

  const [statusHistories, setStatusHistories] = useState<ProjectStatusHistoryEntry[]>([]);

  const [milestoneModal, setMilestoneModal] = useState<"create" | ProjectMilestone | null>(null);
  const [isStatusModalOpen, setIsStatusModalOpen] = useState(false);
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);

  const loadInvitations = useCallback(() => {
    if (!token) {
      return;
    }

    getInvitations(token)
      .then((allInvitations) => {
        setInvitations(allInvitations.filter((invitation) => invitation.projectId === project.id));
      })
      .catch(() => {
        setInvitationError("Could not load invitations.");
      });
  }, [token, project.id]);

  const loadComments = useCallback(() => {
    if (!token) {
      return;
    }

    getComments(token)
      .then((allComments) => {
        setComments(allComments.filter((comment) => comment.projectId === project.id));
      })
      .catch(() => {
        setCommentError("Could not load comments.");
      });
  }, [token, project.id]);

  const loadStatusHistories = useCallback(() => {
    if (!token) {
      return;
    }

    getStatusHistories(token)
      .then((allHistories) => {
        setStatusHistories(allHistories.filter((history) => history.projectId === project.id));
      })
      .catch(() => {
        // Non-critical: the status history tab simply stays empty.
      });
  }, [token, project.id]);

  useEffect(() => {
    loadInvitations();
  }, [loadInvitations]);

  useEffect(() => {
    loadComments();
  }, [loadComments]);

  useEffect(() => {
    loadStatusHistories();
  }, [loadStatusHistories]);

  const loadDocuments = useCallback(() => {
    if (!token) {
      return;
    }

    setIsLoadingDocuments(true);

    getDocuments(token)
      .then((allDocuments) => {
        setDocuments(allDocuments.filter((doc) => doc.projectId === project.id));
      })
      .catch(() => {
        setDocumentError("Could not load documents.");
      })
      .finally(() => {
        setIsLoadingDocuments(false);
      });
  }, [token, project.id]);

  useEffect(() => {
    loadDocuments();
  }, [loadDocuments]);

  const directoryById = useMemo(() => {
    const map = new Map<string, UserDirectoryEntry>();
    for (const entry of directory) {
      map.set(entry.id, entry);
    }
    return map;
  }, [directory]);

  const projectMilestones = useMemo(
    () =>
      [...milestones]
        .filter((m) => m.projectId === project.id)
        .sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime()),
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

  const isAdmin = user?.role === "Administrator";
  const isMember = projectAssignments.some((assignment) => assignment.userId === user?.id);
  const canManageProject = isAdmin || isMember;
  const canManageMilestones = isAdmin || mentor?.userId === user?.id;

  const pendingInvitations = useMemo(
    () => invitations.filter((invitation) => invitation.status === 1),
    [invitations],
  );

  const excludedUserIds = useMemo(() => {
    const ids = new Set(projectAssignments.map((assignment) => assignment.userId));
    for (const invitation of pendingInvitations) {
      ids.add(invitation.invitedUserId);
    }
    if (user) {
      ids.add(user.id);
    }
    return ids;
  }, [projectAssignments, pendingInvitations, user]);

  const hasPendingMentorRequest = pendingInvitations.some((invitation) => invitation.role === "Mentor");

  const submittedAt = useMemo(
    () => statusHistories.find((history) => history.newStatus === SUBMITTED_STATUS)?.createdAt,
    [statusHistories],
  );

  async function handleCancelInvitation(invitationId: string) {
    if (!token) {
      return;
    }
    setInvitationError(null);
    setBusyInvitationId(invitationId);
    try {
      await cancelInvitation(invitationId, token);
      loadInvitations();
    } catch (err) {
      setInvitationError(err instanceof ApiError ? err.message : "Could not cancel the invitation.");
    } finally {
      setBusyInvitationId(null);
    }
  }

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

  async function handlePostComment() {
    if (!token || !newComment.trim()) {
      return;
    }
    setCommentError(null);
    setIsPostingComment(true);
    try {
      await createComment({ projectId: project.id, content: newComment.trim() }, token);
      setNewComment("");
      loadComments();
    } catch (err) {
      setCommentError(err instanceof ApiError ? err.message : "Could not post the comment.");
    } finally {
      setIsPostingComment(false);
    }
  }

  async function handleDeleteComment(commentId: string) {
    if (!token) {
      return;
    }
    setCommentError(null);
    setBusyCommentId(commentId);
    try {
      await deleteComment(commentId, token);
      loadComments();
    } catch (err) {
      setCommentError(err instanceof ApiError ? err.message : "Could not delete the comment.");
    } finally {
      setBusyCommentId(null);
    }
  }

  const categoryPalette = paletteFor(project.categoryId);

  const tabs: { key: TabKey; label: string; count?: number }[] = [
    { key: "overview", label: "Overview" },
    { key: "milestones", label: "Milestones", count: totalMilestones },
    { key: "documents", label: "Documents", count: documents.length },
    { key: "comments", label: "Comments", count: comments.length },
    { key: "status", label: "Status History" },
  ];

  return (
    <div>
      <div className="flex items-center gap-1.5 text-sm">
        <button type="button" onClick={onBack} className="font-medium text-slate-500 hover:text-slate-900">
          Projects
        </button>
        <span className="text-slate-300">/</span>
        <span className="truncate text-slate-500">{project.title}</span>
      </div>

      <div className="mt-4 flex flex-wrap items-start justify-between gap-4">
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

        {canManageProject && (
          <div className="flex shrink-0 flex-wrap gap-2">
            <button
              type="button"
              onClick={() => setIsStatusModalOpen(true)}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50"
            >
              Change Status
            </button>
            <button
              type="button"
              onClick={() => setInviteModal("Student")}
              className="flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50"
            >
              <UserPlus className="h-4 w-4" />
              Invite Student
            </button>
            {!mentor && !hasPendingMentorRequest && (
              <button
                type="button"
                onClick={() => setInviteModal("Mentor")}
                className="flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50"
              >
                <UserPlus className="h-4 w-4" />
                Request Mentor
              </button>
            )}
          </div>
        )}
      </div>

      <div className="mt-6 rounded-xl border border-slate-200 bg-white p-5">
        <div className="flex items-center justify-between text-sm text-slate-500">
          <span>Overall Progress</span>
          <span>{totalMilestones === 0 ? "No milestones yet" : `${completedMilestones}/${totalMilestones} milestones complete`}</span>
        </div>
        <div className="mt-2 flex items-center gap-3">
          <div className="h-2 flex-1 rounded-full bg-slate-100">
            <div className="h-2 rounded-full bg-red-800" style={{ width: `${progress}%` }} />
          </div>
          <span className="text-sm text-slate-500">{progress}%</span>
        </div>

        <div className="mt-5 grid grid-cols-2 gap-4 border-t border-slate-100 pt-4 sm:grid-cols-4">
          <div>
            <p className="text-xs text-slate-400">Created</p>
            <p className="mt-1 text-sm font-medium text-slate-900">{formatDate(project.createdAt)}</p>
          </div>
          <div>
            <p className="text-xs text-slate-400">Last Updated</p>
            <p className="mt-1 text-sm font-medium text-slate-900">{formatDate(project.updatedAt ?? project.createdAt)}</p>
          </div>
          <div>
            <p className="text-xs text-slate-400">Submitted</p>
            <p className="mt-1 text-sm font-medium text-slate-900">{submittedAt ? formatDate(submittedAt) : "–"}</p>
          </div>
          <div>
            <p className="text-xs text-slate-400">Status Changes</p>
            <p className="mt-1 text-sm font-medium text-slate-900">{statusHistories.length}</p>
          </div>
        </div>
      </div>

      <div className="mt-6 flex gap-6 overflow-x-auto border-b border-slate-200">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            type="button"
            onClick={() => setActiveTab(tab.key)}
            className={`flex shrink-0 items-center gap-1.5 border-b-2 px-1 pb-3 text-sm font-medium ${
              activeTab === tab.key
                ? "border-slate-900 text-slate-900"
                : "border-transparent text-slate-500 hover:text-slate-700"
            }`}
          >
            {tab.label}
            {tab.count !== undefined && (
              <span className="rounded-full bg-slate-100 px-1.5 py-0.5 text-xs text-slate-500">{tab.count}</span>
            )}
          </button>
        ))}
      </div>

      <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          {activeTab === "overview" && (
            <>
              <div className="rounded-xl border border-slate-200 bg-white p-5">
                <h2 className="font-serif text-lg font-bold text-slate-900">Project Description</h2>
                <p className="mt-3 text-sm text-slate-600">{project.description || "No description provided."}</p>
              </div>

              {canManageProject && pendingInvitations.length > 0 && (
                <div className="rounded-xl border border-slate-200 bg-white p-5">
                  <h2 className="font-serif text-lg font-bold text-slate-900">Pending Invitations</h2>
                  {invitationError && <p className="mt-2 text-sm text-red-600">{invitationError}</p>}
                  <div className="mt-3 space-y-2">
                    {pendingInvitations.map((invitation) => {
                      const invitedUser = directoryById.get(invitation.invitedUserId);
                      const isBusy = busyInvitationId === invitation.id;

                      return (
                        <div
                          key={invitation.id}
                          className="flex items-center justify-between gap-4 rounded-lg border border-slate-100 p-3"
                        >
                          <div>
                            <p className="text-sm font-medium text-slate-900">{displayName(invitedUser)}</p>
                            <p className="text-xs text-slate-500">Invited as {invitation.role} · Pending</p>
                          </div>
                          <button
                            type="button"
                            disabled={isBusy}
                            onClick={() => handleCancelInvitation(invitation.id)}
                            className="flex items-center gap-1 rounded-lg border border-slate-200 px-3 py-1.5 text-xs font-medium text-slate-600 hover:bg-slate-50 disabled:opacity-50"
                          >
                            <X className="h-3.5 w-3.5" />
                            Cancel
                          </button>
                        </div>
                      );
                    })}
                  </div>
                </div>
              )}

              <div className="rounded-xl border border-slate-200 bg-white p-5">
                <h2 className="font-serif text-lg font-bold text-slate-900">Milestone Progress</h2>
                {projectMilestones.length === 0 ? (
                  <p className="mt-3 text-sm text-slate-500">No milestones yet.</p>
                ) : (
                  <div className="mt-3 space-y-3">
                    {projectMilestones.map((milestone) => {
                      const meta = MILESTONE_STATUS_META[milestone.status];

                      return (
                        <div key={milestone.id} className="flex items-center gap-3">
                          <MilestoneStatusIcon status={milestone.status} />
                          <div className="min-w-0 flex-1">
                            <p className="text-sm font-medium text-slate-900">{milestone.title}</p>
                            <p className="text-xs text-slate-500">Due {formatDate(milestone.dueDate)}</p>
                          </div>
                          <span className={`shrink-0 rounded-full px-2.5 py-1 text-xs font-medium ${meta.badge}`}>
                            {meta.label}
                          </span>
                          {canManageMilestones && (
                            <button
                              type="button"
                              onClick={() => setMilestoneModal(milestone)}
                              className="shrink-0 text-xs font-medium text-slate-500 hover:text-slate-900"
                            >
                              Edit
                            </button>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            </>
          )}

          {activeTab === "milestones" && (
            <div className="rounded-xl border border-slate-200 bg-white p-5">
              <div className="flex items-center justify-between">
                <h2 className="font-serif text-lg font-bold text-slate-900">Milestones</h2>
                {canManageMilestones && (
                  <button
                    type="button"
                    onClick={() => setMilestoneModal("create")}
                    className="flex items-center gap-1.5 rounded-lg bg-slate-900 px-3 py-1.5 text-xs font-semibold text-white hover:bg-slate-800"
                  >
                    <Plus className="h-3.5 w-3.5" />
                    Add Milestone
                  </button>
                )}
              </div>

              {projectMilestones.length === 0 ? (
                <p className="mt-3 text-sm text-slate-500">No milestones yet.</p>
              ) : (
                <div className="mt-3 space-y-3">
                  {projectMilestones.map((milestone) => {
                    const meta = MILESTONE_STATUS_META[milestone.status];

                    return (
                      <div
                        key={milestone.id}
                        className="flex items-start gap-4 rounded-xl border border-slate-200 p-4"
                      >
                        <MilestoneStatusIcon status={milestone.status} />
                        <div className="min-w-0 flex-1">
                          <div className="flex items-start justify-between gap-4">
                            <h3 className="text-sm font-semibold text-slate-900">{milestone.title}</h3>
                            <div className="flex shrink-0 items-center gap-2">
                              <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${meta.badge}`}>
                                {meta.label}
                              </span>
                              {canManageMilestones && (
                                <button
                                  type="button"
                                  onClick={() => setMilestoneModal(milestone)}
                                  className="text-xs font-medium text-slate-500 hover:text-slate-900"
                                >
                                  Edit
                                </button>
                              )}
                            </div>
                          </div>
                          {milestone.description && (
                            <p className="mt-1 text-sm text-slate-500">{milestone.description}</p>
                          )}
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
          )}

          {activeTab === "documents" && (
            <div className="rounded-xl border border-slate-200 bg-white p-5">
              <div className="flex items-center justify-between">
                <h2 className="font-serif text-lg font-bold text-slate-900">Documents</h2>
                {canManageProject && (
                  <button
                    type="button"
                    onClick={() => setIsUploadModalOpen(true)}
                    className="flex items-center gap-1.5 rounded-lg bg-slate-900 px-3 py-1.5 text-xs font-semibold text-white hover:bg-slate-800"
                  >
                    <Plus className="h-3.5 w-3.5" />
                    Add Document
                  </button>
                )}
              </div>
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
                      <div key={doc.id} className="flex items-center gap-4 rounded-xl border border-slate-200 p-4">
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
          )}

          {activeTab === "comments" && (
            <div className="rounded-xl border border-slate-200 bg-white p-5">
              <h2 className="font-serif text-lg font-bold text-slate-900">Comments</h2>

              {canManageProject && (
                <div className="mt-3 flex gap-2">
                  <input
                    value={newComment}
                    onChange={(event) => setNewComment(event.target.value)}
                    onKeyDown={(event) => {
                      if (event.key === "Enter") {
                        void handlePostComment();
                      }
                    }}
                    placeholder="Write a comment..."
                    className="flex-1 rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-slate-400 focus:outline-none"
                  />
                  <button
                    type="button"
                    disabled={isPostingComment || !newComment.trim()}
                    onClick={handlePostComment}
                    className="rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
                  >
                    Post
                  </button>
                </div>
              )}

              {commentError && <p className="mt-2 text-sm text-red-600">{commentError}</p>}

              {comments.length === 0 ? (
                <p className="mt-4 text-sm text-slate-500">No comments yet.</p>
              ) : (
                <div className="mt-4 space-y-3">
                  {[...comments]
                    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
                    .map((comment) => {
                      const author = directoryById.get(comment.authorId);
                      const canDelete = isAdmin || comment.authorId === user?.id;
                      const isBusy = busyCommentId === comment.id;

                      return (
                        <div key={comment.id} className="flex items-start gap-3 rounded-lg border border-slate-100 p-3">
                          <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-slate-900 text-xs font-semibold text-white">
                            {initials(author?.firstName ?? "", author?.lastName ?? "")}
                          </div>
                          <div className="min-w-0 flex-1">
                            <div className="flex items-center justify-between gap-2">
                              <p className="text-sm font-medium text-slate-900">{displayName(author)}</p>
                              <span className="shrink-0 text-xs text-slate-400">{formatDateTime(comment.createdAt)}</span>
                            </div>
                            <p className="mt-1 text-sm text-slate-600">{comment.content}</p>
                          </div>
                          {canDelete && (
                            <button
                              type="button"
                              disabled={isBusy}
                              title="Delete comment"
                              onClick={() => handleDeleteComment(comment.id)}
                              className="shrink-0 text-slate-400 hover:text-red-600 disabled:opacity-50"
                            >
                              <Trash2 className="h-4 w-4" />
                            </button>
                          )}
                        </div>
                      );
                    })}
                </div>
              )}
            </div>
          )}

          {activeTab === "status" && (
            <div className="rounded-xl border border-slate-200 bg-white p-5">
              <h2 className="font-serif text-lg font-bold text-slate-900">Status History</h2>
              {statusHistories.length === 0 ? (
                <p className="mt-3 text-sm text-slate-500">No status changes yet.</p>
              ) : (
                <div className="mt-3 space-y-3">
                  {[...statusHistories]
                    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
                    .map((history) => (
                      <div key={history.id} className="rounded-lg border border-slate-100 p-3">
                        <div className="flex flex-wrap items-center justify-between gap-2">
                          <p className="text-sm font-medium text-slate-900">
                            {projectStatusLabel(history.previousStatus)} → {projectStatusLabel(history.newStatus)}
                          </p>
                          <span className="text-xs text-slate-400">{formatDateTime(history.createdAt)}</span>
                        </div>
                        {history.comment && <p className="mt-1 text-sm text-slate-500">{history.comment}</p>}
                      </div>
                    ))}
                </div>
              )}
            </div>
          )}
        </div>

        <div className="space-y-6">
          <div className="rounded-xl border border-slate-200 bg-white p-5">
            <h3 className="text-xs font-semibold tracking-wide text-slate-400 uppercase">Mentor</h3>
            {mentor ? (
              <div className="mt-3 flex items-center gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-slate-900 text-sm font-semibold text-white">
                  {initials(
                    directoryById.get(mentor.userId)?.firstName ?? "",
                    directoryById.get(mentor.userId)?.lastName ?? "",
                  )}
                </div>
                <p className="text-sm font-medium text-slate-900">{displayName(directoryById.get(mentor.userId))}</p>
              </div>
            ) : (
              <p className="mt-3 text-sm text-slate-400">No mentor assigned</p>
            )}
          </div>

          <div className="rounded-xl border border-slate-200 bg-white p-5">
            <h3 className="text-xs font-semibold tracking-wide text-slate-400 uppercase">
              Students ({teammates.length})
            </h3>
            {teammates.length === 0 ? (
              <p className="mt-3 text-sm text-slate-400">No students yet</p>
            ) : (
              <div className="mt-3 space-y-3">
                {teammates.map((teammate) => (
                  <div key={teammate.id} className="flex items-center gap-3">
                    <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-stone-200 text-xs font-semibold text-stone-700">
                      {initials(
                        directoryById.get(teammate.userId)?.firstName ?? "",
                        directoryById.get(teammate.userId)?.lastName ?? "",
                      )}
                    </div>
                    <p className="text-sm font-medium text-slate-900">{displayName(directoryById.get(teammate.userId))}</p>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="rounded-xl border border-slate-200 bg-white p-5">
            <h3 className="text-xs font-semibold tracking-wide text-slate-400 uppercase">Quick Stats</h3>
            <div className="mt-3 space-y-2 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-slate-500">Comments</span>
                <span className="font-semibold text-slate-900">{comments.length}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-slate-500">Documents</span>
                <span className="font-semibold text-slate-900">{documents.length}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-slate-500">Milestones Done</span>
                <span className="font-semibold text-slate-900">
                  {completedMilestones}/{totalMilestones}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {inviteModal && (
        <InviteMemberModal
          projectId={project.id}
          role={inviteModal}
          directory={directory}
          excludedUserIds={excludedUserIds}
          onClose={() => setInviteModal(null)}
          onInvited={() => {
            setInviteModal(null);
            loadInvitations();
          }}
        />
      )}

      {milestoneModal && (
        <MilestoneModal
          projectId={project.id}
          initial={milestoneModal === "create" ? undefined : milestoneModal}
          onClose={() => setMilestoneModal(null)}
          onSaved={() => {
            setMilestoneModal(null);
            onRefresh();
          }}
        />
      )}

      {isStatusModalOpen && (
        <ChangeStatusModal
          project={project}
          onClose={() => setIsStatusModalOpen(false)}
          onSaved={() => {
            setIsStatusModalOpen(false);
            onRefresh();
            loadStatusHistories();
          }}
        />
      )}

      {isUploadModalOpen && (
        <UploadDocumentModal
          projects={[project]}
          fixedProject={project}
          onClose={() => setIsUploadModalOpen(false)}
          onUploaded={() => {
            setIsUploadModalOpen(false);
            loadDocuments();
          }}
        />
      )}
    </div>
  );
}
