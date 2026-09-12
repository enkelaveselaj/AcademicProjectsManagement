import type { ReactNode } from "react";
import {
  LayoutDashboard,
  Folder,
  Target,
  FileText,
  Bell,
  Users,
  Tag,
  LogOut,
} from "lucide-react";
import { useAuth } from "../lib/useAuth";

export type Screen =
  | "dashboard"
  | "projects"
  | "milestones"
  | "documents"
  | "notifications"
  | "userManagement"
  | "categories";

interface NavItem {
  screen: Screen;
  label: string;
  icon: typeof LayoutDashboard;
  adminOnly?: boolean;
}

const NAV_ITEMS: NavItem[] = [
  { screen: "dashboard", label: "Dashboard", icon: LayoutDashboard },
  { screen: "projects", label: "Projects", icon: Folder },
  { screen: "milestones", label: "Milestones", icon: Target },
  { screen: "documents", label: "Documents", icon: FileText },
  { screen: "notifications", label: "Notifications", icon: Bell },
  { screen: "userManagement", label: "User Management", icon: Users, adminOnly: true },
  { screen: "categories", label: "Categories", icon: Tag, adminOnly: true },
];

function initials(firstName: string, lastName: string): string {
  return `${firstName[0] ?? ""}${lastName[0] ?? ""}`.toUpperCase();
}

interface LayoutProps {
  activeScreen: Screen;
  onNavigate: (screen: Screen) => void;
  children: ReactNode;
}

export function Layout({ activeScreen, onNavigate, children }: LayoutProps) {
  const { user, signOut } = useAuth();

  const navItems = NAV_ITEMS.filter((item) => !item.adminOnly || user?.role === "Administrator");

  return (
    <div className="flex min-h-screen bg-cream">
      <aside className="flex w-64 shrink-0 flex-col bg-slate-900 text-white">
        <div className="flex items-center gap-3 px-5 py-6">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-red-800 font-serif text-lg font-bold">
            A
          </div>
          <div>
            <p className="font-serif text-base leading-tight font-bold">Academic Projects</p>
            <p className="text-xs text-slate-400">Platform</p>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto px-3 pb-4">
          <p className="px-2 pb-2 text-xs font-medium tracking-wide text-slate-500 uppercase">Navigation</p>

          <nav className="space-y-1">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = item.screen === activeScreen;

              return (
                <button
                  key={item.screen}
                  type="button"
                  onClick={() => onNavigate(item.screen)}
                  className={`flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium ${
                    isActive ? "bg-slate-800 text-white" : "text-slate-300 hover:bg-slate-800/60 hover:text-white"
                  }`}
                >
                  <Icon className="h-4 w-4 shrink-0" />
                  {item.label}
                </button>
              );
            })}
          </nav>
        </div>

        {user && (
          <div className="border-t border-slate-800 px-4 py-4">
            <div className="flex items-center gap-3">
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-red-800 text-xs font-semibold">
                {initials(user.firstName, user.lastName)}
              </div>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium text-white">
                  {user.firstName} {user.lastName}
                </p>
                <p className="text-xs text-slate-400">{user.role}</p>
              </div>
              <button
                type="button"
                onClick={signOut}
                title="Sign out"
                className="shrink-0 rounded-lg p-1.5 text-slate-400 hover:bg-slate-800 hover:text-white"
              >
                <LogOut className="h-4 w-4" />
              </button>
            </div>
          </div>
        )}
      </aside>

      <main className="flex-1 overflow-y-auto p-8">{children}</main>
    </div>
  );
}
