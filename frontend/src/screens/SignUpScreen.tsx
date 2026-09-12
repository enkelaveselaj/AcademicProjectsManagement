import { useState, type FormEvent } from "react";
import { ApiError } from "../lib/apiClient";
import { register, type RequestedRole } from "../lib/authApi";
import { LoginHeroPanel } from "./LoginHeroPanel";

interface SignUpScreenProps {
  onNavigateToLogin: () => void;
}

const KNOWN_FIELDS = new Set([
  "firstName",
  "lastName",
  "email",
  "password",
  "dateOfBirth",
  "personalIdNumber",
  "requestedRole",
  "studentId",
]);

export function SignUpScreen({ onNavigateToLogin }: SignUpScreenProps) {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [personalIdNumber, setPersonalIdNumber] = useState("");
  const [requestedRole, setRequestedRole] = useState<RequestedRole>("Student");
  const [studentId, setStudentId] = useState("");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [generalError, setGeneralError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);

  function fieldError(field: string): string | undefined {
    return fieldErrors[field]?.[0];
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setGeneralError(null);
    setFieldErrors({});
    setIsSubmitting(true);

    try {
      await register({
        firstName,
        lastName,
        email,
        password,
        dateOfBirth,
        personalIdNumber,
        requestedRole,
        studentId: requestedRole === "Student" ? studentId : undefined,
      });
      setIsSubmitted(true);
    } catch (err) {
      if (err instanceof ApiError && err.errors) {
        setFieldErrors(err.errors);

        const unmatched = Object.entries(err.errors)
          .filter(([field]) => !KNOWN_FIELDS.has(field))
          .flatMap(([, messages]) => messages);

        if (unmatched.length > 0) {
          setGeneralError(unmatched.join(" "));
        }
      } else if (err instanceof ApiError) {
        setGeneralError(err.message);
      } else {
        setGeneralError("Something went wrong. Please try again.");
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isSubmitted) {
    return (
      <div className="flex min-h-screen">
        <LoginHeroPanel />

        <div className="flex w-full items-center justify-center bg-cream px-6 py-12 lg:w-1/2">
          <div className="w-full max-w-sm text-center">
            <h1 className="font-serif text-4xl font-bold text-slate-900">Request submitted</h1>
            <p className="mt-4 text-sm text-slate-600">
              Your {requestedRole.toLowerCase()} account request is awaiting administrator approval. You'll be able
              to sign in once it's approved.
            </p>
            <button
              type="button"
              onClick={onNavigateToLogin}
              className="mt-8 w-full rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white hover:bg-slate-800"
            >
              Back to sign in
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen">
      <LoginHeroPanel />

      <div className="flex w-full items-center justify-center bg-cream px-6 py-12 lg:w-1/2">
        <div className="w-full max-w-sm">
          <h1 className="font-serif text-4xl font-bold text-slate-900">Create account</h1>
          <p className="mt-2 text-sm text-slate-500">Request a student or mentor account.</p>

          <form className="mt-8 space-y-4" onSubmit={handleSubmit}>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label htmlFor="firstName" className="block text-sm font-medium text-slate-800">
                  First name
                </label>
                <input
                  id="firstName"
                  required
                  value={firstName}
                  onChange={(event) => setFirstName(event.target.value)}
                  className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
                />
                {fieldError("firstName") && <p className="mt-1 text-xs text-red-600">{fieldError("firstName")}</p>}
              </div>

              <div>
                <label htmlFor="lastName" className="block text-sm font-medium text-slate-800">
                  Last name
                </label>
                <input
                  id="lastName"
                  required
                  value={lastName}
                  onChange={(event) => setLastName(event.target.value)}
                  className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
                />
                {fieldError("lastName") && <p className="mt-1 text-xs text-red-600">{fieldError("lastName")}</p>}
              </div>
            </div>

            <div>
              <label htmlFor="email" className="block text-sm font-medium text-slate-800">
                Email address
              </label>
              <input
                id="email"
                type="email"
                required
                placeholder="you@university.edu"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 placeholder:text-slate-400 focus:border-slate-400 focus:outline-none"
              />
              {fieldError("email") && <p className="mt-1 text-xs text-red-600">{fieldError("email")}</p>}
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-slate-800">
                Password
              </label>
              <input
                id="password"
                type="password"
                required
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
              />
              {fieldError("password") && <p className="mt-1 text-xs text-red-600">{fieldError("password")}</p>}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label htmlFor="dateOfBirth" className="block text-sm font-medium text-slate-800">
                  Date of birth
                </label>
                <input
                  id="dateOfBirth"
                  type="date"
                  required
                  value={dateOfBirth}
                  onChange={(event) => setDateOfBirth(event.target.value)}
                  className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
                />
                {fieldError("dateOfBirth") && (
                  <p className="mt-1 text-xs text-red-600">{fieldError("dateOfBirth")}</p>
                )}
              </div>

              <div>
                <label htmlFor="personalIdNumber" className="block text-sm font-medium text-slate-800">
                  Personal ID number
                </label>
                <input
                  id="personalIdNumber"
                  required
                  value={personalIdNumber}
                  onChange={(event) => setPersonalIdNumber(event.target.value)}
                  className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
                />
                {fieldError("personalIdNumber") && (
                  <p className="mt-1 text-xs text-red-600">{fieldError("personalIdNumber")}</p>
                )}
              </div>
            </div>

            <div>
              <span className="block text-sm font-medium text-slate-800">Account type</span>
              <div className="mt-1.5 grid grid-cols-2 gap-3">
                {(["Student", "Mentor"] as const).map((role) => (
                  <button
                    key={role}
                    type="button"
                    onClick={() => setRequestedRole(role)}
                    className={`rounded-lg border px-4 py-2.5 text-sm font-medium ${
                      requestedRole === role
                        ? "border-slate-900 bg-slate-900 text-white"
                        : "border-slate-200 bg-white text-slate-700 hover:border-slate-300"
                    }`}
                  >
                    {role}
                  </button>
                ))}
              </div>
              {fieldError("requestedRole") && (
                <p className="mt-1 text-xs text-red-600">{fieldError("requestedRole")}</p>
              )}
            </div>

            {requestedRole === "Student" && (
              <div>
                <label htmlFor="studentId" className="block text-sm font-medium text-slate-800">
                  Student ID
                </label>
                <input
                  id="studentId"
                  required
                  value={studentId}
                  onChange={(event) => setStudentId(event.target.value)}
                  className="mt-1.5 w-full rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 focus:border-slate-400 focus:outline-none"
                />
                {fieldError("studentId") && <p className="mt-1 text-xs text-red-600">{fieldError("studentId")}</p>}
              </div>
            )}

            {generalError && <p className="text-sm text-red-600">{generalError}</p>}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {isSubmitting ? "Submitting..." : "Request account"}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-slate-500">
            Already have an account?{" "}
            <button type="button" onClick={onNavigateToLogin} className="font-medium text-slate-900 underline">
              Sign in
            </button>
          </p>
        </div>
      </div>
    </div>
  );
}
