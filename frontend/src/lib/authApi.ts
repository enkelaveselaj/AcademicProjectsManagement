import { apiRequest } from "./apiClient";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
}

export function login(request: LoginRequest): Promise<LoginResponse> {
  return apiRequest<LoginResponse>("/api/auth/login", {
    method: "POST",
    body: request,
  });
}

export type RequestedRole = "Student" | "Mentor";

// Must match AcademicProjects.Domain.Enums.UserRole on the backend — enums serialize as integers.
const REQUESTED_ROLE_CODES: Record<RequestedRole, number> = {
  Mentor: 2,
  Student: 3,
};

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  dateOfBirth: string;
  personalIdNumber: string;
  requestedRole: RequestedRole;
  studentId?: string;
}

export interface RegisteredUser {
  id: string;
  email: string;
  role: string;
  approvalStatus: string;
}

export function register(request: RegisterRequest): Promise<RegisteredUser> {
  return apiRequest<RegisteredUser>("/api/auth/register", {
    method: "POST",
    body: {
      firstName: request.firstName,
      lastName: request.lastName,
      email: request.email,
      password: request.password,
      dateOfBirth: request.dateOfBirth,
      personalIdNumber: request.personalIdNumber,
      requestedRole: REQUESTED_ROLE_CODES[request.requestedRole],
      studentId: request.requestedRole === "Student" ? request.studentId : null,
    },
  });
}
