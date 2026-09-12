import { AuthProvider } from "./lib/AuthProvider";
import { useAuth } from "./lib/useAuth";
import { Layout } from "./components/Layout";
import { LoginScreen } from "./screens/LoginScreen";
import { DashboardScreen } from "./screens/DashboardScreen";

function AppShell() {
  const { user } = useAuth();

  if (!user) {
    return <LoginScreen />;
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
