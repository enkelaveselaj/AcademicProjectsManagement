import { useCallback, useEffect, useMemo, useState } from "react";
import { AlertTriangle, CheckCircle2, Info, XCircle } from "lucide-react";
import { useAuth } from "../lib/useAuth";
import {
  getNotifications,
  markAllNotificationsAsRead,
  markNotificationAsRead,
  type Notification,
  type NotificationType,
} from "../lib/notificationsApi";

const TYPE_META: Record<NotificationType, { label: string; badge: string; icon: typeof Info; iconBg: string; iconText: string }> = {
  1: { label: "Information", badge: "bg-sky-50 text-sky-700", icon: Info, iconBg: "bg-sky-50", iconText: "text-sky-600" },
  2: { label: "Warning", badge: "bg-amber-50 text-amber-700", icon: AlertTriangle, iconBg: "bg-amber-50", iconText: "text-amber-700" },
  3: { label: "Success", badge: "bg-emerald-50 text-emerald-700", icon: CheckCircle2, iconBg: "bg-emerald-50", iconText: "text-emerald-700" },
  4: { label: "Error", badge: "bg-red-50 text-red-700", icon: XCircle, iconBg: "bg-red-50", iconText: "text-red-700" },
};

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString(undefined, { day: "numeric", month: "long", year: "numeric" });
}

export function NotificationsScreen() {
  const { token } = useAuth();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [filter, setFilter] = useState<"all" | "unread">("all");
  const [isMarkingAll, setIsMarkingAll] = useState(false);
  const [busyIds, setBusyIds] = useState<Set<string>>(new Set());

  const loadNotifications = useCallback(async () => {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setLoadError(null);

    try {
      setNotifications(await getNotifications(token));
    } catch {
      setLoadError("Could not load notifications. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void loadNotifications();
  }, [loadNotifications]);

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const visibleNotifications = useMemo(() => {
    const list = filter === "unread" ? notifications.filter((n) => !n.isRead) : notifications;
    return [...list].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }, [notifications, filter]);

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

  async function handleMarkAsRead(notification: Notification) {
    if (!token || notification.isRead) {
      return;
    }

    setBusy(notification.id, true);
    try {
      const updated = await markNotificationAsRead(notification.id, token);
      setNotifications((previous) => previous.map((n) => (n.id === updated.id ? updated : n)));
    } catch {
      // Leave the item as-is; the user can retry by clicking again.
    } finally {
      setBusy(notification.id, false);
    }
  }

  async function handleMarkAllAsRead() {
    if (!token || unreadCount === 0) {
      return;
    }

    setIsMarkingAll(true);
    try {
      await markAllNotificationsAsRead(token);
      await loadNotifications();
    } catch {
      setLoadError("Could not mark all notifications as read. Please try again.");
    } finally {
      setIsMarkingAll(false);
    }
  }

  return (
    <div>
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="font-serif text-3xl font-bold text-slate-900">Notifications</h1>
          <p className="mt-1 text-sm text-slate-500">
            {unreadCount} unread · {notifications.length} total
          </p>
        </div>

        <button
          type="button"
          onClick={handleMarkAllAsRead}
          disabled={isMarkingAll || unreadCount === 0}
          className="shrink-0 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-900 hover:bg-slate-50 disabled:opacity-50"
        >
          {isMarkingAll ? "Marking..." : "Mark all read"}
        </button>
      </div>

      <div className="mt-6 flex gap-2">
        <button
          type="button"
          onClick={() => setFilter("all")}
          className={`rounded-full border px-4 py-1.5 text-sm font-medium ${
            filter === "all"
              ? "border-slate-900 bg-slate-900 text-white"
              : "border-slate-200 bg-white text-slate-600 hover:border-slate-300"
          }`}
        >
          All
        </button>
        <button
          type="button"
          onClick={() => setFilter("unread")}
          className={`flex items-center gap-2 rounded-full border px-4 py-1.5 text-sm font-medium ${
            filter === "unread"
              ? "border-slate-900 bg-slate-900 text-white"
              : "border-slate-200 bg-white text-slate-600 hover:border-slate-300"
          }`}
        >
          Unread
          {unreadCount > 0 && (
            <span
              className={`rounded-full px-1.5 py-0.5 text-xs font-semibold ${
                filter === "unread" ? "bg-white/20 text-white" : "bg-red-800 text-white"
              }`}
            >
              {unreadCount}
            </span>
          )}
        </button>
      </div>

      {loadError && <p className="mt-4 text-sm text-red-600">{loadError}</p>}

      <div className="mt-6 space-y-3">
        {isLoading ? (
          <p className="text-sm text-slate-500">Loading...</p>
        ) : notifications.length === 0 ? (
          <p className="text-sm text-slate-500">No notifications yet.</p>
        ) : visibleNotifications.length === 0 ? (
          <p className="text-sm text-slate-500">No unread notifications.</p>
        ) : (
          visibleNotifications.map((notification) => {
            const meta = TYPE_META[notification.type];
            const Icon = meta.icon;

            return (
              <button
                key={notification.id}
                type="button"
                onClick={() => handleMarkAsRead(notification)}
                disabled={notification.isRead || busyIds.has(notification.id)}
                className={`flex w-full items-start gap-4 rounded-xl border bg-white p-4 text-left ${
                  notification.isRead ? "border-slate-200" : "border-l-4 border-l-red-800 border-slate-200"
                } ${notification.isRead ? "" : "cursor-pointer hover:bg-slate-50"}`}
              >
                <div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-lg ${meta.iconBg}`}>
                  <Icon className={`h-5 w-5 ${meta.iconText}`} />
                </div>

                <div className="min-w-0 flex-1">
                  <p className="text-sm font-semibold text-slate-900">{notification.message}</p>
                  <div className="mt-2 flex items-center gap-3">
                    <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${meta.badge}`}>{meta.label}</span>
                    <span className="font-mono text-xs text-slate-400">{formatDate(notification.createdAt)}</span>
                  </div>
                </div>

                {!notification.isRead && <span className="mt-1 h-2.5 w-2.5 shrink-0 rounded-full bg-red-800" />}
              </button>
            );
          })
        )}
      </div>
    </div>
  );
}
