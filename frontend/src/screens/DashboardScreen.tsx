import { useAuth } from "../lib/useAuth";

export function DashboardScreen() {
  const { user } = useAuth();

  return (
    <div>
      <h1 className="text-lg font-semibold text-slate-900">Welcome, {user?.firstName}.</h1>
      <p className="mt-1 text-sm text-slate-500">
        You're signed in as {user?.role}. Real project data lands here in the next commit.
      </p>
    </div>
  );
}
