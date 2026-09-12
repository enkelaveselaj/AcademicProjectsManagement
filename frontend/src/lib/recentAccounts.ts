const STORAGE_KEY = "academicProjects.recentAccounts";
const MAX_RECENT_ACCOUNTS = 3;

export interface RecentAccount {
  email: string;
  firstName: string;
  lastName: string;
}

export function getRecentAccounts(): RecentAccount[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as RecentAccount[]) : [];
  } catch {
    return [];
  }
}

export function recordRecentAccount(account: RecentAccount): void {
  const remaining = getRecentAccounts().filter((existing) => existing.email !== account.email);
  const updated = [account, ...remaining].slice(0, MAX_RECENT_ACCOUNTS);
  localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
}
