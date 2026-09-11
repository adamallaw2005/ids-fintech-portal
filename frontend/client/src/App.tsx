import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import "./App.css";
import "./branding.css";
import { Layout } from "./components/Layout";
import { Account } from "./pages/Account";
import { Admin } from "./pages/Admin";
import { ClientDetail, Clients } from "./pages/Clients";
import { Dashboard } from "./pages/Dashboard";
import { Deployments, Environments } from "./pages/Operations";
import { ProductDetail, Products } from "./pages/Products";
import { Resources } from "./pages/Resources";
import { Team } from "./pages/Team";
import { Login } from "./pages/Login";
import { ErrorState } from "./components/ui";
import type { Page, User } from "./types";

function App() {
  const [user, setUser] = useState<User | null>(() => {
    try {
      if (!localStorage.getItem("ids_token")) return null;
      return JSON.parse(localStorage.getItem("ids_user") || "null");
    } catch {
      return null;
    }
  });
  const [page, setPage] = useState<Page>("dashboard");
  const [detailId, setDetailId] = useState<number | null>(null);

  useEffect(() => {
    const handler = () => {
      setUser(null);
      setPage("dashboard");
      setDetailId(null);
    };
    window.addEventListener("ids-auth-expired", handler);
    return () => window.removeEventListener("ids-auth-expired", handler);
  }, []);

  function logout() {
    localStorage.removeItem("ids_token");
    localStorage.removeItem("ids_user");
    setUser(null);
    setPage("dashboard");
    setDetailId(null);
  }

  function go(next: Page) {
    setDetailId(null);
    setPage(next);
  }

  if (!user) return <Login onLogin={setUser} />;

  const canManage = user.roleName === "Admin";
  let content: ReactNode;
  if (detailId && page === "products")
    content = <ProductDetail id={detailId} back={() => setDetailId(null)} canManage={canManage} />;
  else if (detailId && page === "clients")
    content = <ClientDetail id={detailId} back={() => setDetailId(null)} canManage={canManage} />;
  else if (page === "dashboard") content = <Dashboard go={go} canManage={canManage} />;
  else if (page === "products") content = <Products openDetail={setDetailId} canManage={canManage} />;
  else if (page === "clients") content = <Clients openDetail={setDetailId} canManage={canManage} />;
  else if (page === "deployments") content = <Deployments canManage={canManage} />;
  else if (page === "environments") content = <Environments canManage={canManage} />;
  else if (page === "team") content = <Team canManage={canManage} />;
  else if (page === "resources") content = <Resources canManage={canManage} />;
  else if (page === "admin")
    content =
      user.roleName === "Admin" ? (
        <Admin />
      ) : (
        <ErrorState message="Administrator access is required." />
      );
  else content = <Account user={user} refreshUser={setUser} logout={logout} />;

  return (
    <Layout user={user} page={page} setPage={go} onLogout={logout}>
      {content}
    </Layout>
  );
}

export default App;
