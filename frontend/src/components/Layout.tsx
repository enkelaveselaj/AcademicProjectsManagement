import type { ReactNode } from "react";
import { useAuth } from "../lib/useAuth";

interface LayoutProps {
  children: ReactNode;
}

export function Layout({ children }: LayoutProps) {
  const { user, signOut } = useAuth();

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="flex items-center justify-between border-b border-slate-200 bg-white px-6 py-3">
        <span className="text-sm font-semibold text-slate-900">Academic Projects</span>

        {user && (
          <div className="flex items-center gap-3">
            <span className="text-sm text-slate-600">
              {user.firstName} {user.lastName}
            </span>
            <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700">
              {user.role}
            </span>
            <button
              type="button"
              onClick={signOut}
              className="text-sm font-medium text-slate-500 hover:text-slate-800"
            >
              Sign out
            </button>
          </div>
        )}
      </header>

      <main className="p-6">{children}</main>
    </div>
  );
}
