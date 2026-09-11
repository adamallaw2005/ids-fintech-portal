/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from "react";
import type { FormEvent } from "react";
import { api, useRequest } from "../lib/api";
import { formatDate, initials } from "../lib/format";
import {
  Button,
  ErrorState,
  Field,
  Loading,
  Modal,
  PageHeader,
} from "../components/ui";
import type { Entity } from "../types";

export function EditUser({
  item,
  close,
  saved,
}: {
  item: Entity;
  close: () => void;
  saved: () => void;
}) {
  const [form, setForm] = useState({
    fullName: item.fullName,
    email: item.email,
    newPassword: "",
  });
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  async function submit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError("");
    try {
      await api(`/users/${item.userId}`, {
        method: "PUT",
        body: JSON.stringify({
          ...form,
          newPassword: form.newPassword || null,
        }),
      });
      saved();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <Modal title={`Edit ${item.fullName}`} onClose={close}>
      <form onSubmit={submit}>
        <Field label="Full name">
          <input
            required
            value={form.fullName}
            onChange={(e) => setForm({ ...form, fullName: e.target.value })}
          />
        </Field>
        <Field label="Email">
          <input
            type="email"
            required
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
          />
        </Field>
        <Field
          label="Reset password"
          hint="Leave blank to keep the current password."
        >
          <input
            type="password"
            value={form.newPassword}
            onChange={(e) => setForm({ ...form, newPassword: e.target.value })}
          />
        </Field>
        {error && <div className="form-error">{error}</div>}
        <div className="modal-actions">
          <Button variant="secondary" onClick={close}>
            Cancel
          </Button>
          <Button type="submit" disabled={busy}>
            {busy ? "Saving…" : "Save user"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
export function Admin() {
  const request = useRequest<Entity[]>("/users");
  const [create, setCreate] = useState(false);
  const [edit, setEdit] = useState<Entity | null>(null);
  async function update(path: string, body: Entity) {
    try {
      await api(path, { method: "PUT", body: JSON.stringify(body) });
      request.reload();
    } catch (err: any) {
      alert(err.message);
    }
  }
  return (
    <>
      <PageHeader
        eyebrow="ADMINISTRATION"
        title="User management"
        description="Manage portal access, profiles, passwords and roles."
        action={<Button onClick={() => setCreate(true)}>+ Add user</Button>}
      />
      {request.loading ? (
        <Loading />
      ) : request.error ? (
        <ErrorState message={request.error} retry={request.reload} />
      ) : (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>User</th>
                <th>Role</th>
                <th>Status</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {(request.data ?? []).map((item) => (
                <tr key={item.userId}>
                  <td>
                    <div className="name-cell">
                      <span className="avatar">{initials(item.fullName)}</span>
                      <span>
                        <strong>{item.fullName}</strong>
                        <small>{item.email}</small>
                      </span>
                    </div>
                  </td>
                  <td>
                    <select
                      className="inline-select"
                      value={item.roleName}
                      onChange={(e) =>
                        update(`/users/${item.userId}/role`, {
                          roleName: e.target.value,
                        })
                      }
                    >
                      <option>User</option>
                      <option>Admin</option>
                    </select>
                  </td>
                  <td>
                    <button
                      className={`status-toggle ${item.isActive ? "on" : ""}`}
                      onClick={() =>
                        update(`/users/${item.userId}/status`, {
                          isActive: !item.isActive,
                        })
                      }
                    >
                      <span />
                      {item.isActive ? "Active" : "Inactive"}
                    </button>
                  </td>
                  <td>{formatDate(item.createdAt)}</td>
                  <td>
                    <div className="row-actions">
                      <Button variant="ghost" onClick={() => setEdit(item)}>
                        Edit
                      </Button>
                      <span className="muted">#{item.userId}</span>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {create && (
        <CreateUser
          close={() => setCreate(false)}
          saved={() => {
            setCreate(false);
            request.reload();
          }}
        />
      )}
      {edit && (
        <EditUser
          item={edit}
          close={() => setEdit(null)}
          saved={() => {
            setEdit(null);
            request.reload();
          }}
        />
      )}
    </>
  );
}
export function CreateUser({
  close,
  saved,
}: {
  close: () => void;
  saved: () => void;
}) {
  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    roleName: "User",
    isActive: true,
  });
  const [error, setError] = useState("");
  async function submit(e: FormEvent) {
    e.preventDefault();
    try {
      await api("/users", { method: "POST", body: JSON.stringify(form) });
      saved();
    } catch (err: any) {
      setError(err.message);
    }
  }
  return (
    <Modal title="Add user" onClose={close}>
      <form onSubmit={submit}>
        <Field label="Full name">
          <input
            required
            value={form.fullName}
            onChange={(e) => setForm({ ...form, fullName: e.target.value })}
          />
        </Field>
        <Field label="Email">
          <input
            type="email"
            required
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
          />
        </Field>
        <div className="form-grid">
          <Field label="Temporary password">
            <input
              type="password"
              required
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
            />
          </Field>
          <Field label="Role">
            <select
              value={form.roleName}
              onChange={(e) => setForm({ ...form, roleName: e.target.value })}
            >
              <option>User</option>
              <option>Admin</option>
            </select>
          </Field>
        </div>
        {error && <div className="form-error">{error}</div>}
        <div className="modal-actions">
          <Button variant="secondary" onClick={close}>
            Cancel
          </Button>
          <Button type="submit">Create user</Button>
        </div>
      </form>
    </Modal>
  );
}
