/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from "react";
import type { FormEvent } from "react";
import { api } from "../lib/api";
import { initials } from "../lib/format";
import { Button, Field, PageHeader } from "../components/ui";
import type { User } from "../types";

export function Account({
  user,
  refreshUser,
  logout,
}: {
  user: User;
  refreshUser: (u: User) => void;
  logout: () => void;
}) {
  const [mode, setMode] = useState<"password" | "promote" | "adminchange">(
    "password",
  );
  const [form, setForm] = useState({
    currentPassword: "",
    newPassword: "",
    adminPromotionPassword: "",
  });
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  async function submit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError("");
    setMessage("");
    try {
      if (mode === "password")
        await api("/auth/change-password", {
          method: "POST",
          body: JSON.stringify({
            currentPassword: form.currentPassword,
            newPassword: form.newPassword,
          }),
        });
      if (mode === "adminchange")
        await api("/auth/admin-password/change", {
          method: "POST",
          body: JSON.stringify({
            currentPassword: form.currentPassword,
            newPassword: form.newPassword,
          }),
        });
      if (mode === "promote") {
        const result = await api("/auth/become-admin", {
          method: "POST",
          body: JSON.stringify({
            adminPromotionPassword: form.adminPromotionPassword,
          }),
        });
        localStorage.setItem("ids_token", result.token);
        localStorage.setItem("ids_user", JSON.stringify(result.user));
        refreshUser(result.user);
      }
      setMessage(
        mode === "promote"
          ? "Your account is now an administrator."
          : "The change was saved successfully.",
      );
      setForm({
        currentPassword: "",
        newPassword: "",
        adminPromotionPassword: "",
      });
    } catch (err: any) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }
  const admin = user.roleName === "Admin";
  const chooseMode = (next: "password" | "promote" | "adminchange") => {
    setMode(next);
    setMessage("");
    setError("");
  };
  return (
    <>
      <PageHeader
        eyebrow="ACCOUNT & SECURITY"
        title="My account"
        description="Manage your profile access and security settings."
      />
      <div className="account-layout">
        <section className="panel profile-panel">
          <div className="avatar avatar-xl">{initials(user.fullName)}</div>
          <h2>{user.fullName}</h2>
          <p>{user.email}</p>
          <span className="status status-active">{user.roleName}</span>
          {!admin && (
            <>
              <div className="account-rule" />
              <Button variant="secondary" onClick={() => chooseMode("promote")}>
                Become an admin
              </Button>
            </>
          )}
          {admin && (
            <>
              <div className="account-rule" />
              <Button
                variant="secondary"
                onClick={() => chooseMode("adminchange")}
              >
                Change promotion password
              </Button>
            </>
          )}
          <div className="account-rule" />
          <Button variant="ghost" onClick={logout}>
            Sign out
          </Button>
        </section>
        <section className="panel security-panel">
          <div className="panel-head">
            <div>
              <h2>Security actions</h2>
              <p>Keep your access details current.</p>
            </div>
          </div>
          <div className="security-tabs">
            <button
              className={mode === "password" ? "active" : ""}
              onClick={() => chooseMode("password")}
            >
              Change my password
            </button>
            {admin && (
              <button
                className={mode === "adminchange" ? "active" : ""}
                onClick={() => chooseMode("adminchange")}
              >
                Change promotion password
              </button>
            )}
          </div>
          <form onSubmit={submit} className="security-form">
            {mode === "promote" ? (
              <Field
                label="Administrator promotion password"
                hint="Enter the shared password configured for this portal."
              >
                <input
                  type="password"
                  value={form.adminPromotionPassword}
                  onChange={(e) =>
                    setForm({ ...form, adminPromotionPassword: e.target.value })
                  }
                  required
                />
              </Field>
            ) : (
              <>
                <Field
                  label={
                    mode === "adminchange"
                      ? "Current promotion password"
                      : "Current password"
                  }
                >
                  <input
                    type="password"
                    value={form.currentPassword}
                    onChange={(e) =>
                      setForm({ ...form, currentPassword: e.target.value })
                    }
                    required
                  />
                </Field>
                <Field label="New password">
                  <input
                    type="password"
                    value={form.newPassword}
                    onChange={(e) =>
                      setForm({ ...form, newPassword: e.target.value })
                    }
                    required
                  />
                </Field>
              </>
            )}
            {error && <div className="form-error">{error}</div>}
            {message && <div className="form-success">{message}</div>}
            <Button type="submit" disabled={busy}>
              {busy
                ? "Saving…"
                : mode === "promote"
                  ? "Verify password"
                  : "Save change"}
            </Button>
          </form>
        </section>
      </div>
    </>
  );
}
