/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from "react";
import type { FormEvent } from "react";
import idsLogo from "../assets/ids-fintech-logo.jpeg";
import { Button, Field } from "../components/ui";
import { api } from "../lib/api";
import type { User } from "../types";

export function Login({ onLogin }: { onLogin: (user: User) => void }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  async function submit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError("");
    try {
      const response = await api("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });
      localStorage.setItem("ids_token", response.token);
      localStorage.setItem("ids_user", JSON.stringify(response.user));
      onLogin(response.user);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-aside">
        <img className="company-logo-full" src={idsLogo} alt="IDS Fintech" />
        <div className="login-message">
          <div className="eyebrow">IDS FINTECH PRODUCTS PORTAL</div>
          <h1>A clearer way to manage your product work.</h1>
          <p>
            Keep product details, client accounts, deployments, environments,
            and ownership information easy to find and up to date.
          </p>
        </div>
        <div className="login-footer">
          PRODUCTS <span>•</span> CLIENTS <span>•</span> OPERATIONS
        </div>
      </section>
      <section className="login-panel">
        <form className="login-card" onSubmit={submit}>
          <div className="login-title">
            <h2>Welcome back</h2>
            <p>Sign in to pick up where you left off.</p>
          </div>
          <Field label="Email address">
            <input
              type="email"
              autoComplete="username"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </Field>
          <Field label="Password">
            <input
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </Field>
          {error && <div className="form-error">{error}</div>}
          <Button type="submit" disabled={busy} className="btn-full">
            {busy ? "Signing in..." : "Sign in"}
          </Button>
          <p className="login-note">
            Use your IDS Fintech account. Your session stays active for up to 60
            minutes.
          </p>
        </form>
      </section>
    </main>
  );
}
