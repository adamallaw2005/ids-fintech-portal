/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from "react";
import type { FormEvent } from "react";
import { api, useRequest } from "../lib/api";
import { initials, statusClass } from "../lib/format";
import {
  Button,
  EmptyState,
  ErrorState,
  Field,
  Loading,
  Modal,
  PageHeader,
} from "../components/ui";
import { TeamAssignmentFields, TeamAssignmentList } from "../components/TeamAssignments";
import type { TeamAssignment } from "../components/TeamAssignments";
import type { Entity } from "../types";

const teamDefaults = {
  fullName: "",
  jobTitle: "",
  departmentTeam: "",
  email: "",
  status: "Active",
};
export function TeamForm({
  item,
  close,
  saved,
}: {
  item?: Entity;
  close: () => void;
  saved: () => void;
}) {
  const [form, setForm] = useState({ ...teamDefaults, ...item });
  const products = useRequest<Entity[]>(item ? `/product-responsibilities?teamMemberId=${item.teamMemberId}` : null);
  const clients = useRequest<Entity[]>(item ? `/client-responsibilities?teamMemberId=${item.teamMemberId}` : null);
  const [editedProducts, setProducts] = useState<TeamAssignment[] | null>(null);
  const [editedClients, setClients] = useState<TeamAssignment[] | null>(null);
  const productAssignments = editedProducts ?? (products.data ?? []).map(row => ({ targetId: String(row.productId), responsibilityRole: row.responsibilityRole, description: row.description ?? "" }));
  const clientAssignments = editedClients ?? (clients.data ?? []).map(row => ({ targetId: String(row.clientId), responsibilityRole: row.responsibilityRole, description: row.description ?? "" }));
  const ready = !item || Boolean(products.data && clients.data && !products.loading && !clients.loading && !products.error && !clients.error);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const change = (key: string, value: string) =>
    setForm((old) => ({ ...old, [key]: value }));
  async function submit(e: FormEvent) {
    e.preventDefault();
    if (busy || !ready) return;
    setError("");
    setBusy(true);
    try {
      await api(item ? `/team-members/${item.teamMemberId}` : "/team-members", {
        method: item ? "PUT" : "POST",
        body: JSON.stringify({ ...form,
          email: form.email?.trim() || null,
          productAssignments: item && editedProducts === null ? undefined : productAssignments.map(row => ({ ...row, targetId: Number(row.targetId) })),
          clientAssignments: item && editedClients === null ? undefined : clientAssignments.map(row => ({ ...row, targetId: Number(row.targetId) })),
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
    <Modal
      title={item ? "Edit team member" : "Add team member"}
      onClose={() => { if (!busy) close(); }}
    >
      <form onSubmit={submit}>
        <Field label="Full name">
          <input
            required
            value={form.fullName ?? ""}
            onChange={(e) => change("fullName", e.target.value)}
          />
        </Field>
        <div className="form-grid">
          <Field label="Job title">
            <input
              value={form.jobTitle ?? ""}
              onChange={(e) => change("jobTitle", e.target.value)}
            />
          </Field>
          <Field label="Department / team">
            <input
              value={form.departmentTeam ?? ""}
              onChange={(e) => change("departmentTeam", e.target.value)}
            />
          </Field>
          <Field label="Email">
            <input
              type="email"
              value={form.email ?? ""}
              onChange={(e) => change("email", e.target.value)}
            />
          </Field>
          <Field label="Status">
            <select
              value={form.status ?? ""}
              onChange={(e) => change("status", e.target.value)}
            >
              <option>Active</option>
              <option>Inactive</option>
            </select>
          </Field>
        </div>
        {products.error ? <ErrorState message={products.error} retry={products.reload} /> : clients.error ? <ErrorState message={clients.error} retry={clients.reload} /> : !ready ? <Loading /> : <fieldset disabled={busy} style={{ border: 0, padding: 0, margin: 0 }}>
          <TeamAssignmentFields kind="product" value={productAssignments} onChange={setProducts} />
          <TeamAssignmentFields kind="client" value={clientAssignments} onChange={setClients} />
        </fieldset>}
        {error && <div className="form-error">{error}</div>}
        <div className="modal-actions">
          <Button variant="secondary" disabled={busy} onClick={close}>
            Cancel
          </Button>
          <Button type="submit" disabled={busy || !ready}>
            {busy ? "Saving…" : "Save team member"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
export function Team({ canManage }: { canManage: boolean }) {
  const [modal, setModal] = useState<"new" | Entity | null>(null);
  const [viewing, setViewing] = useState<Entity | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [status, setStatus] = useState("");
  const request = useRequest<Entity[]>(
    `/team-members${status ? `?status=${encodeURIComponent(status)}` : ""}`,
  );
  async function remove(item: Entity) {
    if (deleting || !confirm(`Delete ${item.fullName} and remove all their product and client assignments? The products and clients will be kept.`)) return;
    setDeleting(true);
    try {
      await api(`/team-members/${item.teamMemberId}`, { method: "DELETE" });
      request.reload();
    } catch (err: any) {
      alert(err.message);
    } finally {
      setDeleting(false);
    }
  }
  return (
    <>
      <PageHeader
        eyebrow="PEOPLE & OWNERSHIP"
        title="Team members"
        description="See who is responsible for your products and client accounts."
        action={
          canManage && (<Button onClick={() => setModal("new")}>+ Add team member</Button>)
        }
      />
      <div className="filter-bar">
        <select
          className="filter-select"
          value={status}
          onChange={(e) => setStatus(e.target.value)}
        >
          <option value="">All statuses</option>
          <option>Active</option>
          <option>Inactive</option>
        </select>
      </div>
      {request.loading ? (
        <Loading />
      ) : request.error ? (
        <ErrorState message={request.error} retry={request.reload} />
      ) : request.data?.length ? (
        <div className="team-grid">
          {request.data.map((item) => (
            <article className="person-card" key={item.teamMemberId}>
              <div className="person-top">
                <div className="avatar avatar-large">
                  {initials(item.fullName)}
                </div>
                <span className={statusClass(item.status)}>{item.status}</span>
              </div>
              <h2>{item.fullName}</h2>
              <p>{item.jobTitle || "Team member"}</p>
              <div className="person-meta">
                <span>{item.departmentTeam || "No team specified"}</span>
                <span>{item.email || "No email"}</span>
              </div>
              <div className="person-stats">
                <span>
                  <b>{item.productResponsibilityCount ?? 0}</b> products
                </span>
                <span>
                  <b>{item.clientResponsibilityCount ?? 0}</b> clients
                </span>
              </div>
              <div className="card-actions">
                <Button variant="ghost" onClick={() => setViewing(item)}>View</Button>
              {canManage && (<>
                <Button variant="ghost" onClick={() => setModal(item)}>
                  Edit
                </Button>
                <Button variant="danger-ghost" disabled={deleting} onClick={() => remove(item)}>
                  Delete
                </Button>
              </>)}
              </div>
            </article>
          ))}
        </div>
      ) : (
        <EmptyState
          title="No team members found"
          text={canManage ? "Add people to assign responsibility across the portal." : "No team members match this filter."}
          action={
            canManage && (<Button onClick={() => setModal("new")}>+ Add team member</Button>)
          }
        />
      )}
      {viewing && <Modal title={viewing.fullName} onClose={() => setViewing(null)}>
        <p>{viewing.jobTitle || "Team member"} · {viewing.departmentTeam || "No team specified"}</p>
        <p>{viewing.email || "No email"} · {viewing.status}</p>
        <TeamAssignmentList kind="product" memberId={viewing.teamMemberId} />
        <TeamAssignmentList kind="client" memberId={viewing.teamMemberId} />
        <div className="modal-actions">
          <Button variant="secondary" onClick={() => setViewing(null)}>Close</Button>
          {canManage && <Button onClick={() => { setModal(viewing); setViewing(null); }}>Edit team member</Button>}
        </div>
      </Modal>}
      {canManage && modal && (
        <TeamForm
          item={modal === "new" ? undefined : modal}
          close={() => setModal(null)}
          saved={() => {
            setModal(null);
            request.reload();
          }}
        />
      )}
    </>
  );
}
