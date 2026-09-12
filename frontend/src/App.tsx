import { useState } from "react";
import { AuthProvider } from "./lib/AuthProvider";
import { useAuth } from "./lib/useAuth";
import { Layout, type Screen } from "./components/Layout";
import { LoginScreen } from "./screens/LoginScreen";
import { SignUpScreen } from "./screens/SignUpScreen";
import { DashboardScreen } from "./screens/DashboardScreen";
import { UserManagementScreen } from "./screens/UserManagementScreen";
import { ComingSoonScreen } from "./screens/ComingSoonScreen";

const SCREEN_TITLES: Record<Screen, string> = {
  dashboard: "Dashboard",
  projects: "Projects",
  milestones: "Milestones",
  documents: "Documents",
  notifications: "Notifications",
  userManagement: "User Management",
  categories: "Categories",
};

function AppShell() {
  const { user } = useAuth();
  const [authScreen, setAuthScreen] = useState<"login" | "signup">("login");
  const [activeScreen, setActiveScreen] = useState<Screen>("dashboard");

  if (!user) {
    return authScreen === "login" ? (
      <LoginScreen onNavigateToSignUp={() => setAuthScreen("signup")} />
    ) : (
      <SignUpScreen onNavigateToLogin={() => setAuthScreen("login")} />
    );
  }

  return (
    <Layout activeScreen={activeScreen} onNavigate={setActiveScreen}>
      {activeScreen === "dashboard" && <DashboardScreen />}
      {activeScreen === "userManagement" && <UserManagementScreen />}
      {activeScreen !== "dashboard" && activeScreen !== "userManagement" && (
        <ComingSoonScreen title={SCREEN_TITLES[activeScreen]} />
      )}
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
