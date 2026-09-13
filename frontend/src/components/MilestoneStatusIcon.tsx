import { AlertTriangle, Check } from "lucide-react";
import type { MilestoneStatus } from "../lib/milestonesApi";

export function MilestoneStatusIcon({ status }: { status: MilestoneStatus }) {
  if (status === 3) {
    return (
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-emerald-700 text-white">
        <Check className="h-4 w-4" />
      </div>
    );
  }
  if (status === 4) {
    return (
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-red-700 text-white">
        <AlertTriangle className="h-4 w-4" />
      </div>
    );
  }
  if (status === 2) {
    return <div className="h-8 w-8 shrink-0 rounded-full border-2 border-slate-900" />;
  }
  return <div className="h-8 w-8 shrink-0 rounded-full border-2 border-slate-300" />;
}
