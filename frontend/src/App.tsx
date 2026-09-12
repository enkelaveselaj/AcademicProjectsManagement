import { useState } from "react";
import { AuthProvider } from "./lib/AuthProvider";
import { useAuth } from "./lib/useAuth";
import { Layout, type Screen } from "./components/Layout";
import { LoginScreen } from "./screens/LoginScreen";
import { SignUpScreen } from "./screens/SignUpScreen";
import { DashboardScreen } from "./screens/DashboardScreen";
import { UserManagementScreen } from "./screens/UserManagementScreen";
import { CategoriesScreen } from "./screens/CategoriesScreen";
import { ProjectsScreen } from "./screens/ProjectsScreen";
import { MilestonesScreen } from "./screens/MilestonesScreen";
import { DocumentsScreen } from "./screens/DocumentsScreen";
import { NotificationsScreen } from "./screens/NotificationsScreen";
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

function renderScreen(screen: Screen, onNavigate: (screen: Screen) => void) {
  switch (screen) {
    case "dashboard":
      return <DashboardScreen onNavigate={onNavigate} />;
    case "userManagement":
      return <UserManagementScreen />;
    case "categories":
      return <CategoriesScreen />;
    case "projects":
      return <ProjectsScreen />;
    case "milestones":
      return <MilestonesScreen />;
    case "documents":
      return <DocumentsScreen />;
    case "notifications":
      return <NotificationsScreen />;
    default:
      return <ComingSoonScreen title={SCREEN_TITLES[screen]} />;
  }
}

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
      {renderScreen(activeScreen, setActiveScreen)}
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
