/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from "react";
import type { FormEvent } from "react";
import { api, useRequest } from "../lib/api";
import { formatDate, statusClass } from "../lib/format";
import {
  Button,
  EmptyState,
  ErrorState,
  Field,
  Loading,
  Modal,
  PageHeader,
} from "../components/ui";
import type { Entity } from "../types";

const deploymentDefaults = {
  clientId: "",
  productId: "",
  productVersion: "",
  goLiveDate: "",
  deploymentStatus: "Testing",
  supportTier: "",
  clientSpecificNotes: "",
};
export function DeploymentForm({
  products,
  clients,
  close,
  saved,
}: {
  products: Entity[];
  clients: Entity[];
  close: () => void;
  saved: () => void;
}) {
  const [form, setForm] = useState(deploymentDefaults);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const change = (key: string, value: string) =>
    setForm((old) => ({ ...old, [key]: value }));
  async function submit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    try {
      await api("/deployments", {
        method: "POST",
        body: JSON.stringify({
          ...form,
          clientId: Number(form.clientId),
          productId: Number(form.productId),
          goLiveDate: form.goLiveDate || null,
          supportTier: form.supportTier || null,
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
    <Modal title="Add deployment" onClose={close}>
      <form onSubmit={submit}>
        <div className="form-grid">
          <Field label="Product">
            <select
              required
              value={form.productId}
              onChange={(e) => change("productId", e.target.value)}
            >
              <option value="">Select product</option>
              {products.map((item) => (
                <option key={item.productId} value={item.productId}>
                  {item.productName}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Client">
            <select
              required
              value={form.clientId}
              onChange={(e) => change("clientId", e.target.value)}
            >
              <option value="">Select client</option>
              {clients.map((item) => (
                <option key={item.clientId} value={item.clientId}>
                  {item.companyName}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Product version">
            <input
              required
              value={form.productVersion}
              onChange={(e) => change("productVersion", e.target.value)}
              placeholder="e.g. 2.4.0"
            />
          </Field>
          <Field label="Status">
            <select
              value={form.deploymentStatus}
              onChange={(e) => change("deploymentStatus", e.target.value)}
            >
              <option>Testing</option>
              <option>Development</option>
              <option>Production</option>
              <option>UAT</option>
              <option>Inactive</option>
            </select>
          </Field>
          <Field label="Go-live date">
            <input
              type="date"
              value={form.goLiveDate}
              onChange={(e) => change("goLiveDate", e.target.value)}
            />
          </Field>
          <Field label="Support tier">
            <select
              value={form.supportTier}
              onChange={(e) => change("supportTier", e.target.value)}
            >
              <option value="">Select support tier</option>
              <option>Standard</option>
              <option>Premium</option>
              <option>Critical</option>
            </select>
          </Field>
        </div>
        <Field label="Client-specific notes">
          <textarea
            value={form.clientSpecificNotes}
            onChange={(e) => change("clientSpecificNotes", e.target.value)}
            rows={3}
          />
        </Field>
        {error && <div className="form-error">{error}</div>}
        <div className="modal-actions">
          <Button variant="secondary" onClick={close}>
            Cancel
          </Button>
          <Button type="submit" disabled={busy}>
            {busy ? "Saving…" : "Save deployment"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
export function Deployments({ canManage }: { canManage: boolean }) {
  const [modal, setModal] = useState(false);
  const [clientId, setClientId] = useState("");
  const [productId, setProductId] = useState("");
  const [status, setStatus] = useState("");
  const params = new URLSearchParams();
  if (clientId) params.set("clientId", clientId);
  if (productId) params.set("productId", productId);
  if (status) params.set("status", status);
  const request = useRequest<Entity[]>(
    `/deployments${params.toString() ? `?${params.toString()}` : ""}`,
  );
  const products = useRequest<Entity[]>("/products");
  const clients = useRequest<Entity[]>("/clients");
  async function remove(item: Entity) {
    if (!confirm(`Delete deployment #${item.deploymentId}?`)) return;
    try {
      await api(`/deployments/${item.deploymentId}`, { method: "DELETE" });
      request.reload();
    } catch (err: any) {
      alert(err.message);
    }
  }
  return (
    <>
      <PageHeader
        eyebrow="RELEASE OPERATIONS"
        title="Deployments"
        description="See where each product is running and who it supports."
        action={
          canManage && (<Button onClick={() => setModal(true)}>+ Add deployment</Button>)
        }
      />
      <div className="filter-bar">
        <select
          className="filter-select"
          value={productId}
          onChange={(e) => setProductId(e.target.value)}
        >
          <option value="">All products</option>
          {(products.data ?? []).map((item) => (
            <option key={item.productId} value={item.productId}>
              {item.productName}
            </option>
          ))}
        </select>
        <select
          className="filter-select"
          value={clientId}
          onChange={(e) => setClientId(e.target.value)}
        >
          <option value="">All clients</option>
          {(clients.data ?? []).map((item) => (
            <option key={item.clientId} value={item.clientId}>
              {item.companyName}
            </option>
          ))}
        </select>
        <select
          className="filter-select"
          value={status}
          onChange={(e) => setStatus(e.target.value)}
        >
          <option value="">All statuses</option>
          <option>Development</option>
          <option>Testing</option>
          <option>Production</option>
          <option>UAT</option>
          <option>Inactive</option>
        </select>
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
                <th>Product</th>
                <th>Client</th>
                <th>Version</th>
                <th>Status</th>
                <th>Environments</th>
                <th>Go-live</th>
                {canManage && <th />}
              </tr>
            </thead>
            <tbody>
              {request.data.map((item) => (
                <tr key={item.deploymentId}>
                  <td>
                    <strong>{item.productName}</strong>
                  </td>
                  <td>{item.clientName}</td>
                  <td>{item.productVersion}</td>
                  <td>
                    <span className={statusClass(item.deploymentStatus)}>
                      {item.deploymentStatus}
                    </span>
                  </td>
                  <td>
                    <span className="count-badge">{item.environmentCount}</span>
                  </td>
                  <td>{formatDate(item.goLiveDate)}</td>
                  {canManage && (<td>
                    <Button variant="danger-ghost" onClick={() => remove(item)}>
                      Delete
                    </Button>
                  </td>)}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <EmptyState
          title="No deployments found"
          text={canManage ? "Start tracking an installation by adding a deployment." : "No deployments match these filters."}
          action={
            canManage && (<Button onClick={() => setModal(true)}>+ Add deployment</Button>)
          }
        />
      )}
      {canManage && modal && (
        <DeploymentForm
          products={products.data ?? []}
          clients={clients.data ?? []}
          close={() => setModal(false)}
          saved={() => {
            setModal(false);
            request.reload();
          }}
        />
      )}
    </>
  );
}
const envDefaults = {
  deploymentId: "",
  environmentName: "",
  environmentType: "Testing",
  purpose: "",
  serverName: "",
  operatingSystem: "",
  applicationUrl: "",
  databaseInformation: "",
  monitoringLink: "",
  accessInstructionsReference: "",
  notes: "",
};
export function EnvironmentForm({
  deployments,
  close,
  saved,
}: {
  deployments: Entity[];
  close: () => void;
  saved: () => void;
}) {
  const [form, setForm] = useState(envDefaults);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const change = (key: string, value: string) =>
    setForm((old) => ({ ...old, [key]: value }));
  async function submit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    try {
      await api("/environments", {
        method: "POST",
        body: JSON.stringify({
          ...form,
          deploymentId: Number(form.deploymentId),
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
    <Modal title="Add environment" onClose={close}>
      <form onSubmit={submit}>
        <div className="form-grid">
          <Field label="Deployment">
            <select
              required
              value={form.deploymentId}
              onChange={(e) => change("deploymentId", e.target.value)}
            >
              <option value="">Select deployment</option>
              {deployments.map((item) => (
                <option key={item.deploymentId} value={item.deploymentId}>
                  {item.productName} · {item.clientName} · {item.productVersion}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Environment type">
            <select
              value={form.environmentType}
              onChange={(e) => change("environmentType", e.target.value)}
            >
              <option>Development</option>
              <option>Testing</option>
              <option>UAT</option>
              <option>Production</option>
              <option>Other</option>
            </select>
          </Field>
        </div>
        <Field label="Environment name">
          <input
            required
            value={form.environmentName}
            onChange={(e) => change("environmentName", e.target.value)}
            placeholder="UAT - Dubai"
          />
        </Field>
        <div className="form-grid">
          <Field label="Server name">
            <input
              value={form.serverName}
              onChange={(e) => change("serverName", e.target.value)}
            />
          </Field>
          <Field label="Operating system">
            <input
              value={form.operatingSystem}
              onChange={(e) => change("operatingSystem", e.target.value)}
            />
          </Field>
          <Field label="Application URL">
            <input
              value={form.applicationUrl}
              onChange={(e) => change("applicationUrl", e.target.value)}
            />
          </Field>
          <Field label="Monitoring link">
            <input
              value={form.monitoringLink}
              onChange={(e) => change("monitoringLink", e.target.value)}
            />
          </Field>
        </div>
        <Field label="Purpose">
          <textarea
            value={form.purpose}
            onChange={(e) => change("purpose", e.target.value)}
            rows={2}
          />
        </Field>
        <Field label="Database information">
          <textarea
            value={form.databaseInformation}
            onChange={(e) => change("databaseInformation", e.target.value)}
            rows={2}
          />
        </Field>
        <Field label="Notes">
          <textarea
            value={form.notes}
            onChange={(e) => change("notes", e.target.value)}
            rows={2}
          />
        </Field>
        {error && <div className="form-error">{error}</div>}
        <div className="modal-actions">
          <Button variant="secondary" onClick={close}>
            Cancel
          </Button>
          <Button type="submit" disabled={busy}>
            {busy ? "Saving…" : "Save environment"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
export function Environments({ canManage }: { canManage: boolean }) {
  const [modal, setModal] = useState(false);
  const [type, setType] = useState("");
  const params = new URLSearchParams();
  if (type) params.set("type", type);
  const request = useRequest<Entity[]>(
    `/environments${params.toString() ? `?${params.toString()}` : ""}`,
  );
  const deployments = useRequest<Entity[]>("/deployments");
  async function remove(item: Entity) {
    if (!confirm(`Delete ${item.environmentName}?`)) return;
    try {
      await api(`/environments/${item.environmentId}`, { method: "DELETE" });
      request.reload();
    } catch (err: any) {
      alert(err.message);
    }
  }
  return (
    <>
      <PageHeader
        eyebrow="RUNTIME LANDSCAPE"
        title="Environments"
        description="Keep the technical details for every deployment close at hand."
        action={
          canManage && (<Button onClick={() => setModal(true)}>+ Add environment</Button>)
        }
      />
      <div className="filter-bar">
        <select
          className="filter-select"
          value={type}
          onChange={(e) => setType(e.target.value)}
        >
          <option value="">All environment types</option>
          <option>Development</option>
          <option>Testing</option>
          <option>UAT</option>
          <option>Production</option>
          <option>Other</option>
        </select>
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
                <th>Environment</th>
                <th>Product</th>
                <th>Client</th>
                <th>Type</th>
                <th>Server</th>
                <th>Application</th>
                {canManage && <th />}
              </tr>
            </thead>
            <tbody>
              {request.data.map((item) => (
                <tr key={item.environmentId}>
                  <td>
                    <strong>{item.environmentName}</strong>
                    <small className="table-sub">
                      Deployment #{item.deploymentId}
                    </small>
                  </td>
                  <td>{item.productName}</td>
                  <td>{item.clientName}</td>
                  <td>
                    <span className={statusClass(item.environmentType)}>
                      {item.environmentType}
                    </span>
                  </td>
                  <td>{item.serverName || "—"}</td>
                  <td>
                    {item.applicationUrl ? (
                      <a
                        href={item.applicationUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="table-link"
                      >
                        Open ↗
                      </a>
                    ) : (
                      "—"
                    )}
                  </td>
                  {canManage && (<td>
                    <Button variant="danger-ghost" onClick={() => remove(item)}>
                      Delete
                    </Button>
                  </td>)}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <EmptyState
          title="No environments found"
          text={canManage ? "Add the first environment for a deployment." : "No environments match this filter."}
          action={
            canManage && (<Button onClick={() => setModal(true)}>+ Add environment</Button>)
          }
        />
      )}
      {canManage && modal && (
        <EnvironmentForm
          deployments={deployments.data ?? []}
          close={() => setModal(false)}
          saved={() => {
            setModal(false);
            request.reload();
          }}
        />
      )}
    </>
  );
}
