export type UserRole = "Administrator" | "Mentor" | "Student";

export interface DecodedAccessToken {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  expiresAt: number;
}

const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
const EMAIL_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
const NAME_ID_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

export function decodeAccessToken(token: string): DecodedAccessToken {
  const payloadSegment = token.split(".")[1];
  const base64 = payloadSegment.replace(/-/g, "+").replace(/_/g, "/");
  const payload = JSON.parse(atob(base64)) as Record<string, unknown>;

  return {
    userId: (payload.sub ?? payload[NAME_ID_CLAIM]) as string,
    email: (payload.email ?? payload[EMAIL_CLAIM]) as string,
    firstName: payload.given_name as string,
    lastName: payload.family_name as string,
    role: payload[ROLE_CLAIM] as UserRole,
    expiresAt: payload.exp as number,
  };
}

export function isTokenExpired(expiresAt: number): boolean {
  return Date.now() >= expiresAt * 1000;
}
