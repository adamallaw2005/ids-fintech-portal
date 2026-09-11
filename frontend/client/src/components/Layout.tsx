import { useState } from "react";
import type { ReactNode } from "react";
import { initials } from "../lib/format";
import type { Page, User } from "../types";

const navItems: { id: Page; label: string; mark: string; admin?: boolean }[] = [
  { id: "dashboard", label: "Overview", mark: "OV" },
  { id: "products", label: "Products", mark: "PR" },
  { id: "clients", label: "Clients", mark: "CL" },
  { id: "deployments", label: "Deployments", mark: "DP" },
  { id: "environments", label: "Environments", mark: "EN" },
  { id: "team", label: "Team members", mark: "TM" },
  { id: "resources", label: "Resources", mark: "RS" },
  { id: "admin", label: "Administration", mark: "AD", admin: true },
];

export function Layout({
  user,
  page,
  setPage,
  onLogout,
  children,
}: {
  user: User;
  page: Page;
  setPage: (page: Page) => void;
  onLogout: () => void;
  children: ReactNode;
}) {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  function navigate(nextPage: Page) {
    setPage(nextPage);
    setSidebarOpen(false);
  }

  return (
    <div className="app-shell">
      <aside className={`sidebar ${sidebarOpen ? "sidebar-open" : ""}`}>
        <div className="sidebar-top">
          <button
            className="brand brand-button"
            onClick={() => navigate("dashboard")}
            aria-label="Go to overview"
          >
            <span className="brand-symbol">›</span>
            <span>
              <b>IDS</b>
              <small>FINTECH PORTAL</small>
            </span>
          </button>
          <button
            className="icon-btn mobile-close"
            onClick={() => setSidebarOpen(false)}
            aria-label="Close navigation"
          >
            ×
          </button>
        </div>
        <nav aria-label="Main navigation">
          {navItems
            .filter((item) => !item.admin || user.roleName === "Admin")
            .map((item) => (
              <button
                key={item.id}
                className={`nav-item ${page === item.id ? "active" : ""}`}
                onClick={() => navigate(item.id)}
              >
                <span className="nav-mark">{item.mark}</span>
                <span>{item.label}</span>
              </button>
            ))}
        </nav>
        <div className="sidebar-bottom">
          <div className="sidebar-rule" />
          <button
            className={`nav-item ${page === "account" ? "active" : ""}`}
            onClick={() => navigate("account")}
          >
            <span className="nav-mark">AC</span>
            <span>My account</span>
          </button>
          <div className="sidebar-user">
            <div className="avatar avatar-small">{initials(user.fullName)}</div>
            <span>
              <strong>{user.fullName}</strong>
              <small>{user.roleName}</small>
            </span>
            <button
              className="logout-mini"
              onClick={onLogout}
              aria-label="Sign out"
              title="Sign out"
            >
              ↪
            </button>
          </div>
        </div>
      </aside>
      <div className="main-shell">
        <header className="topbar">
          <button
            className="menu-btn"
            onClick={() => setSidebarOpen(true)}
            aria-label="Open navigation"
          >
            ☰
          </button>
          <span className="topbar-context">
            IDS FINTECH /{" "}
            {navItems.find((item) => item.id === page)?.label.toUpperCase() ??
              "ACCOUNT"}
          </span>
          <div className="topbar-user">
            <span>{user.fullName}</span>
            <span className="role-label">{user.roleName}</span>
          </div>
        </header>
        <main className="content">{children}</main>
      </div>
    </div>
  );
}
