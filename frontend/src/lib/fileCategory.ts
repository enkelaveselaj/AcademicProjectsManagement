import { Archive, File, FileCode, FileSpreadsheet, FileText, Film, Image, type LucideIcon } from "lucide-react";

export type FileCategory = "PDF" | "Notebook" | "Spreadsheet" | "Image" | "Archive" | "Video" | "Document" | "Other";

const EXTENSION_CATEGORIES: Record<string, FileCategory> = {
  ".pdf": "PDF",
  ".ipynb": "Notebook",
  ".xlsx": "Spreadsheet",
  ".xls": "Spreadsheet",
  ".csv": "Spreadsheet",
  ".png": "Image",
  ".jpg": "Image",
  ".jpeg": "Image",
  ".gif": "Image",
  ".svg": "Image",
  ".webp": "Image",
  ".zip": "Archive",
  ".rar": "Archive",
  ".7z": "Archive",
  ".tar": "Archive",
  ".gz": "Archive",
  ".mp4": "Video",
  ".mov": "Video",
  ".avi": "Video",
  ".webm": "Video",
  ".doc": "Document",
  ".docx": "Document",
  ".ppt": "Document",
  ".pptx": "Document",
  ".txt": "Document",
  ".md": "Document",
};

export function getFileCategory(fileName: string): FileCategory {
  const dotIndex = fileName.lastIndexOf(".");
  if (dotIndex === -1) {
    return "Other";
  }
  return EXTENSION_CATEGORIES[fileName.slice(dotIndex).toLowerCase()] ?? "Other";
}

export const FILE_CATEGORY_META: Record<FileCategory, { icon: LucideIcon; bg: string; text: string; label: string }> = {
  PDF: { icon: FileText, bg: "bg-red-50", text: "text-red-600", label: "PDFs" },
  Notebook: { icon: FileCode, bg: "bg-purple-50", text: "text-purple-600", label: "Notebooks" },
  Spreadsheet: { icon: FileSpreadsheet, bg: "bg-emerald-50", text: "text-emerald-600", label: "Spreadsheets" },
  Image: { icon: Image, bg: "bg-sky-50", text: "text-sky-600", label: "Images" },
  Archive: { icon: Archive, bg: "bg-amber-50", text: "text-amber-700", label: "Archives" },
  Video: { icon: Film, bg: "bg-fuchsia-50", text: "text-fuchsia-600", label: "Videos" },
  Document: { icon: FileText, bg: "bg-slate-100", text: "text-slate-600", label: "Documents" },
  Other: { icon: File, bg: "bg-slate-100", text: "text-slate-500", label: "Other files" },
};

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }
  if (bytes < 1024 * 1024) {
    return `${(bytes / 1024).toFixed(1)} KB`;
  }
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
