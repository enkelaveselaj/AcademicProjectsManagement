import { useState, type FormEvent } from "react";
import { useAuth } from "../lib/useAuth";
import { ApiError } from "../lib/apiClient";
import { getRecentAccounts } from "../lib/recentAccounts";
import { LoginHeroPanel } from "./LoginHeroPanel";

export function LoginScreen() {
  const { signIn } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [recentAccounts] = useState(getRecentAccounts);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await signIn(email, password);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        setError("Invalid email or password.");
      } else {
        setError("Something went wrong. Please try again.");
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  function selectRecentAccount(accountEmail: string) {
    setEmail(accountEmail);
    setPassword("");
    setError(null);
  }

  return (
    <div className="flex min-h-screen">
      <LoginHeroPanel />

      <div className="flex w-full items-center justify-center bg-cream px-6 py-12 lg:w-1/2">
        <div className="w-full max-w-sm">
          <h1 className="font-serif text-4xl font-bold text-slate-900">Sign in</h1>
          <p className="mt-2 text-sm text-slate-500">Enter your university credentials to continue</p>

          <form className="mt-8 space-y-5" onSubmit={handleSubmit}>
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-slate-800">
                Email address
              </label>
              <input
                id="email"
                type="email"
                required
                autoComplete="email"
                placeholder="you@university.edu"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 placeholder:text-slate-400 focus:border-slate-400 focus:outline-none"
              />
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-slate-800">
                Password
              </label>
              <input
                id="password"
                type="password"
                required
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
              />
            </div>

            {error && <p className="text-sm text-red-600">{error}</p>}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {isSubmitting ? "Signing in..." : "Sign in"}
            </button>
          </form>

          {recentAccounts.length > 0 && (
            <div className="mt-8">
              <div className="flex items-center gap-3">
                <div className="h-px flex-1 bg-slate-200" />
                <span className="text-xs tracking-wide text-slate-400 uppercase">Recent accounts</span>
                <div className="h-px flex-1 bg-slate-200" />
              </div>

              <div className="mt-4 space-y-2">
                {recentAccounts.map((account) => (
                  <button
                    key={account.email}
                    type="button"
                    onClick={() => selectRecentAccount(account.email)}
                    className="flex w-full items-center justify-between rounded-lg border border-slate-200 bg-white px-4 py-3 text-left hover:border-slate-300 hover:shadow-sm"
                  >
                    <span>
                      <span className="block text-sm font-semibold text-slate-900">
                        {account.firstName} {account.lastName}
                      </span>
                      <span className="block font-mono text-xs text-slate-500">{account.email}</span>
                    </span>
                    <span className="text-slate-400">→</span>
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
