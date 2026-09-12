import { useState } from "react";
import { AuthProvider } from "./lib/AuthProvider";
import { useAuth } from "./lib/useAuth";
import { Layout } from "./components/Layout";
import { LoginScreen } from "./screens/LoginScreen";
import { SignUpScreen } from "./screens/SignUpScreen";
import { DashboardScreen } from "./screens/DashboardScreen";

function AppShell() {
  const { user } = useAuth();
  const [authScreen, setAuthScreen] = useState<"login" | "signup">("login");

  if (!user) {
    return authScreen === "login" ? (
      <LoginScreen onNavigateToSignUp={() => setAuthScreen("signup")} />
    ) : (
      <SignUpScreen onNavigateToLogin={() => setAuthScreen("login")} />
    );
  }

  return (
    <Layout>
      <DashboardScreen />
    </Layout>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppShell />
    </AuthProvider>
  );
}
