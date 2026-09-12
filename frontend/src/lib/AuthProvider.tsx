import { useState, useCallback, useMemo, type ReactNode } from "react";
import { decodeAccessToken, isTokenExpired, type DecodedAccessToken } from "./jwt";
import { login as loginRequest } from "./authApi";
import { recordRecentAccount } from "./recentAccounts";
import { AuthContext, type AuthUser, type AuthContextValue } from "./authContextValue";

const ACCESS_TOKEN_STORAGE_KEY = "academicProjects.accessToken";

interface AuthSession {
  token: string;
  user: AuthUser;
}

function toAuthUser(decoded: DecodedAccessToken): AuthUser {
  return {
    id: decoded.userId,
    email: decoded.email,
    firstName: decoded.firstName,
    lastName: decoded.lastName,
    role: decoded.role,
  };
}

function readStoredSession(): AuthSession | null {
  const storedToken = localStorage.getItem(ACCESS_TOKEN_STORAGE_KEY);
  if (!storedToken) {
    return null;
  }

  try {
    const decoded = decodeAccessToken(storedToken);
    if (isTokenExpired(decoded.expiresAt)) {
      localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);
      return null;
    }
    return { token: storedToken, user: toAuthUser(decoded) };
  } catch {
    localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(readStoredSession);

  const signIn = useCallback(async (email: string, password: string) => {
    const response = await loginRequest({ email, password });
    localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, response.accessToken);
    const user = toAuthUser(decodeAccessToken(response.accessToken));
    recordRecentAccount({ email: user.email, firstName: user.firstName, lastName: user.lastName });
    setSession({ token: response.accessToken, user });
  }, []);

  const signOut = useCallback(() => {
    localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);
    setSession(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      token: session?.token ?? null,
      signIn,
      signOut,
    }),
    [session, signIn, signOut],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
