import { useState, type FormEvent } from "react";
import { X } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { uploadDocument } from "../lib/documentsApi";
import type { Project } from "../lib/projectsApi";

interface UploadDocumentModalProps {
  projects: Project[];
  onClose: () => void;
  onUploaded: () => void;
}

function uploadErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return err.errors?.FileName?.[0] ?? err.errors?.FileSizeBytes?.[0] ?? err.errors?.ProjectId?.[0] ?? err.message;
  }
  return "Something went wrong. Please try again.";
}

export function UploadDocumentModal({ projects, onClose, onUploaded }: UploadDocumentModalProps) {
  const { token } = useAuth();
  const [file, setFile] = useState<File | null>(null);
  const [projectId, setProjectId] = useState(projects[0]?.id ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!token || !file) {
      return;
    }

    setError(null);
    setIsUploading(true);

    try {
      await uploadDocument(file, projectId, token);
      onUploaded();
    } catch (err) {
      setError(uploadErrorMessage(err));
    } finally {
      setIsUploading(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between">
          <h2 className="font-serif text-xl font-bold text-slate-900">Upload file</h2>
          <button type="button" onClick={onClose} className="text-slate-400 hover:text-slate-700">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="mt-4 space-y-4">
          <div>
            <label htmlFor="uploadProject" className="block text-sm font-medium text-slate-800">
              Project
            </label>
            <select
              id="uploadProject"
              required
              value={projectId}
              onChange={(event) => setProjectId(event.target.value)}
              className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
            >
              {projects.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.title}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label htmlFor="uploadFile" className="block text-sm font-medium text-slate-800">
              File
            </label>
            <input
              id="uploadFile"
              required
              type="file"
              onChange={(event) => setFile(event.target.files?.[0] ?? null)}
              className="mt-1.5 w-full text-sm text-slate-700 file:mr-3 file:rounded-lg file:border-0 file:bg-slate-900 file:px-3 file:py-2 file:text-sm file:font-medium file:text-white hover:file:bg-slate-800"
            />
          </div>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={isUploading || !file || !projectId}
            className="w-full rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
          >
            {isUploading ? "Uploading..." : "Upload"}
          </button>
        </form>
      </div>
    </div>
  );
}
