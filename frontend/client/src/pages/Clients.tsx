/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from "react";
import type { FormEvent } from "react";
import { api, useRequest } from "../lib/api";
import { formatDate, initials, statusClass } from "../lib/format";
import {
  Button,
  EmptyState,
  ErrorState,
  Field,
  Loading,
  Modal,
  PageHeader,
} from "../components/ui";
import { ResponsibilityDrafts, ResponsibilityPanel } from "../components/Responsibilities";
import type { Responsibility, ResponsibilityDraft } from "../components/Responsibilities";
import { SimpleDetailTable } from "../components/DetailTable";
import type { Entity } from "../types";

const clientDefaults = {
  companyName: "",
  country: "",
  contactInformation: "",
  status: "Prospect",
  notes: "",
};
export function ClientForm({
  item,
  close,
  saved,
}: {
  item?: Entity;
  close: () => void;
  saved: (id?: number) => void;
}) {
  const [form, setForm] = useState({ ...clientDefaults, ...item });
  const existingAssignments = useRequest<Responsibility[]>(item ? `/client-responsibilities?clientId=${item.clientId}` : null);
  const [editedAssignments, setResponsibilities] = useState<ResponsibilityDraft[] | null>(null);
  const responsibilities = editedAssignments ?? (existingAssignments.data ?? []).map((assignment) => ({
    teamMemberId: String(assignment.teamMemberId),
    responsibilityRole: assignment.responsibilityRole,
    description: assignment.description ?? "",
  }));
  const assignmentsReady = !item || (!existingAssignments.loading && !existingAssignments.error && existingAssignments.data !== null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const change = (key: string, value: string) =>
    setForm((old) => ({ ...old, [key]: value }));
  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!assignmentsReady) return;
    if (responsibilities.some((assignment) => !assignment.responsibilityRole.trim())) {
      setError("Enter a role for each assigned team member.");
      return;
    }
    setBusy(true);
    setError("");
    try {
      const result = await api(item ? `/clients/${item.clientId}` : "/clients", {
        method: item ? "PUT" : "POST",
        body: JSON.stringify({
          ...form,
          responsibilities: item && editedAssignments === null ? undefined : responsibilities.map((assignment) => ({
            ...assignment,
            teamMemberId: Number(assignment.teamMemberId),
            responsibilityRole: assignment.responsibilityRole.trim(),
          })),
        }),
      });
      saved(result?.clientId);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <Modal title={item ? "Edit client" : "Add client"} onClose={() => { if (!busy) close(); }}>
      <form onSubmit={submit}>
        <Field label="Company name">
          <input
            value={form.companyName ?? ""}
            onChange={(e) => change("companyName", e.target.value)}
            required
          />
        </Field>
        <div className="form-grid">
          <Field label="Country">
            <input
              value={form.country ?? ""}
              onChange={(e) => change("country", e.target.value)}
            />
          </Field>
          <Field label="Status">
            <select
              value={form.status ?? ""}
              onChange={(e) => change("status", e.target.value)}
            >
              <option>Prospect</option>
              <option>Active</option>
              <option>Inactive</option>
            </select>
          </Field>
        </div>
        <Field label="Contact information">
          <textarea
            value={form.contactInformation ?? ""}
            onChange={(e) => change("contactInformation", e.target.value)}
            rows={3}
            placeholder="Primary contact, email, phone"
          />
        </Field>
        <Field label="Notes">
          <textarea
            value={form.notes ?? ""}
            onChange={(e) => change("notes", e.target.value)}
            rows={3}
          />
        </Field>
        {assignmentsReady ? (
          <ResponsibilityDrafts title="Account responsibilities" value={responsibilities}
            onChange={setResponsibilities} disabled={busy} />
        ) : existingAssignments.error ? (
          <ErrorState message={existingAssignments.error} retry={existingAssignments.reload} />
        ) : <Loading />}
        {error && <div className="form-error">{error}</div>}
        <div className="modal-actions">
          <Button variant="secondary" onClick={close} disabled={busy}>
            Cancel
          </Button>
          <Button type="submit" disabled={busy || !assignmentsReady}>
            {busy ? "Saving…" : "Save client"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
export function Clients({ openDetail, canManage }: { openDetail: (id: number) => void; canManage: boolean }) {
  const [query, setQuery] = useState("");
  const [country, setCountry] = useState("");
  const [status, setStatus] = useState("");
  const [modal, setModal] = useState<"new" | Entity | null>(null);
  const params = new URLSearchParams();
  if (query) params.set("search", query);
  if (country) params.set("country", country);
  if (status) params.set("status", status);
  const request = useRequest<Entity[]>(
    `/clients${params.toString() ? `?${params.toString()}` : ""}`,
  );
  async function remove(item: Entity) {
    if (!confirm(`Delete ${item.companyName}?`)) return;
    try {
      await api(`/clients/${item.clientId}`, { method: "DELETE" });
      request.reload();
    } catch (err: any) {
      alert(err.message);
    }
  }
  return (
    <>
      <PageHeader
        eyebrow="CUSTOMER DIRECTORY"
        title="Clients"
        description="Accounts connected to your product deployments."
        action={canManage && (<Button onClick={() => setModal("new")}>+ Add client</Button>)}
      />
      <div className="toolbar">
        <div className="search">
          <span>⌕</span>
          <input
            placeholder="Search clients"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
        </div>
        <input
          className="filter-select filter-input"
          placeholder="Country"
          value={country}
          onChange={(e) => setCountry(e.target.value)}
        />
        <select
          className="filter-select"
          value={status}
          onChange={(e) => setStatus(e.target.value)}
        >
          <option value="">All statuses</option>
          <option>Prospect</option>
          <option>Active</option>
          <option>Inactive</option>
        </select>
        <span className="toolbar-count">
          {request.data?.length ?? 0} clients
        </span>
      </div>
      {request.loading ? (
        <Loading />
      ) : request.error ? (
        <ErrorState message={request.error} retry={request.reload} />
      ) : request.data?.length ? (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Company</th>
                <th>Country</th>
                <th>Status</th>
                <th>Contact</th>
                <th>Updated</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {request.data.map((client) => (
                <tr key={client.clientId}>
                  <td>
                    <button
                      className="name-cell"
                      onClick={() => openDetail(client.clientId)}
                    >
                      <span className="company-icon">
                        {initials(client.companyName)}
                      </span>
                      <span>
                        <strong>{client.companyName}</strong>
                        <small>Client #{client.clientId}</small>
                      </span>
                    </button>
                  </td>
                  <td>{client.country || "—"}</td>
                  <td>
                    <span className={statusClass(client.status)}>
                      {client.status}
                    </span>
                  </td>
                  <td className="truncate">
                    {client.contactInformation || "—"}
                  </td>
                  <td>{formatDate(client.updatedAt)}</td>
                  <td>
                    <div className="row-actions">
                      <Button
                        variant="ghost"
                        onClick={() => openDetail(client.clientId)}
                      >
                        View
                      </Button>
                      {canManage && (<><Button variant="ghost" onClick={() => setModal(client)}>
                        Edit
                      </Button>
                      <Button
                        variant="danger-ghost"
                        onClick={() => remove(client)}
                      >
                        Delete
                      </Button></>)}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <EmptyState
          title="No clients found"
          text={
            query
              ? "Try a different search term."
              : canManage ? "Add a client to start tracking deployments." : "No clients are available yet."
          }
          action={canManage && (<Button onClick={() => setModal("new")}>+ Add client</Button>)}
        />
      )}
      {canManage && modal && (
        <ClientForm
          item={modal === "new" ? undefined : modal}
          close={() => setModal(null)}
          saved={(id) => {
            setModal(null);
            request.reload();
            if (id) openDetail(id);
          }}
        />
      )}
    </>
  );
}
export function ClientDetail({ id, back, canManage }: { id: number; back: () => void; canManage: boolean }) {
  const request = useRequest<Entity>(`/clients/${id}/details`);
  if (request.loading)
    return (
      <>
        <Button variant="ghost" onClick={back}>
          ← Clients
        </Button>
        <Loading />
      </>
    );
  if (request.error || !request.data)
    return (
      <>
        <Button variant="ghost" onClick={back}>
          ← Clients
        </Button>
        <ErrorState
          message={request.error || "Client not found"}
          retry={request.reload}
        />
      </>
    );
  const client = request.data.client;
  return (
    <>
      <button className="back-link" onClick={back}>
        ← Back to clients
      </button>
      <PageHeader
        eyebrow="CLIENT DETAILS"
        title={client.companyName}
        description={client.notes || "Client account and deployment context."}
        action={
          <span className={statusClass(client.status)}>{client.status}</span>
        }
      />
      <div className="detail-meta">
        <span>
          Country <b>{client.country || "Not set"}</b>
        </span>
        <span>
          Contact <b>{client.contactInformation || "Not set"}</b>
        </span>
        <span>
          Updated <b>{formatDate(client.updatedAt)}</b>
        </span>
      </div>
      <div className="detail-grid">
        <SimpleDetailTable
          title="Deployments"
          data={request.data.deployments}
          columns={[
            "productName",
            "productVersion",
            "deploymentStatus",
            "environmentCount",
          ]}
          empty="No deployments for this client."
        />
        <SimpleDetailTable
          title="Environments"
          data={request.data.environments}
          columns={[
            "environmentName",
            "environmentType",
            "serverName",
            "applicationUrl",
          ]}
          empty="No environments for this client."
        />
        <ResponsibilityPanel kind="client" parentId={id}
            data={request.data.responsibilities ?? []} canManage={canManage} onChanged={request.reload} />
      </div>
    </>
  );
}
