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

const productDefaults = {
  productName: "",
  description: "",
  businessPurpose: "",
  lifecycleStatus: "Planned",
  currentVersion: "",
  supportedMarkets: "",
  criticality: "Medium",
  technologies: "",
  notes: "",
};
export function ProductForm({
  item,
  close,
  saved,
}: {
  item?: Entity;
  close: () => void;
  saved: (id?: number) => void;
}) {
  const [form, setForm] = useState({ ...productDefaults, ...item });
  const existingAssignments = useRequest<Responsibility[]>(item ? `/product-responsibilities?productId=${item.productId}` : null);
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
      const result = await api(item ? `/products/${item.productId}` : "/products", {
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
      saved(result?.productId);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <Modal title={item ? "Edit product" : "Add product"} onClose={() => { if (!busy) close(); }}>
      <form onSubmit={submit}>
        <div className="form-grid">
          <Field label="Product name">
            <input
              value={form.productName ?? ""}
              onChange={(e) => change("productName", e.target.value)}
              required
            />
          </Field>
          <Field label="Lifecycle status">
            <select
              value={form.lifecycleStatus ?? ""}
              onChange={(e) => change("lifecycleStatus", e.target.value)}
            >
              <option>Planned</option>
              <option>Active</option>
              <option>Maintenance</option>
              <option>Deprecated</option>
            </select>
          </Field>
          <Field label="Current version">
            <input
              value={form.currentVersion ?? ""}
              onChange={(e) => change("currentVersion", e.target.value)}
              placeholder="e.g. 2.4.0"
            />
          </Field>
          <Field label="Criticality">
            <select
              value={form.criticality ?? ""}
              onChange={(e) => change("criticality", e.target.value)}
            >
              <option>Low</option>
              <option>Medium</option>
              <option>High</option>
              <option>Critical</option>
            </select>
          </Field>
        </div>
        <Field label="Business purpose">
          <textarea
            value={form.businessPurpose ?? ""}
            onChange={(e) => change("businessPurpose", e.target.value)}
            rows={2}
          />
        </Field>
        <Field label="Description">
          <textarea
            value={form.description ?? ""}
            onChange={(e) => change("description", e.target.value)}
            rows={3}
          />
        </Field>
        <div className="form-grid">
          <Field label="Supported markets">
            <input
              value={form.supportedMarkets ?? ""}
              onChange={(e) => change("supportedMarkets", e.target.value)}
              placeholder="Lebanon, UAE"
            />
          </Field>
          <Field label="Technologies">
            <input
              value={form.technologies ?? ""}
              onChange={(e) => change("technologies", e.target.value)}
              placeholder=".NET, SQL Server"
            />
          </Field>
        </div>
        <Field label="Notes">
          <textarea
            value={form.notes ?? ""}
            onChange={(e) => change("notes", e.target.value)}
            rows={2}
          />
        </Field>
        {assignmentsReady ? (
          <ResponsibilityDrafts title="Ownership" value={responsibilities}
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
            {busy ? "Saving…" : "Save product"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
export function Products({ openDetail, canManage }: { openDetail: (id: number) => void; canManage: boolean }) {
  const [query, setQuery] = useState("");
  const [status, setStatus] = useState("");
  const [technology, setTechnology] = useState("");
  const [modal, setModal] = useState<"new" | Entity | null>(null);
  const params = new URLSearchParams();
  if (query) params.set("search", query);
  if (status) params.set("status", status);
  if (technology) params.set("technology", technology);
  const request = useRequest<Entity[]>(
    `/products${params.toString() ? `?${params.toString()}` : ""}`,
  );
  async function remove(item: Entity) {
    if (!confirm(`Delete ${item.productName}?`)) return;
    try {
      await api(`/products/${item.productId}`, { method: "DELETE" });
      request.reload();
    } catch (err: any) {
      alert(err.message);
    }
  }
  return (
    <>
      <PageHeader
        eyebrow="CATALOG"
        title="Products"
        description="The source of truth for products, versions and ownership."
        action={canManage && (<Button onClick={() => setModal("new")}>+ Add product</Button>)}
      />
      <div className="toolbar">
        <div className="search">
          <span>⌕</span>
          <input
            placeholder="Search products"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
        </div>
        <select
          className="filter-select"
          value={status}
          onChange={(e) => setStatus(e.target.value)}
        >
          <option value="">All statuses</option>
          <option>Planned</option>
          <option>Active</option>
          <option>Maintenance</option>
          <option>Deprecated</option>
        </select>
        <input
          className="filter-select filter-input"
          placeholder="Technology"
          value={technology}
          onChange={(e) => setTechnology(e.target.value)}
        />
        <span className="toolbar-count">
          {request.data?.length ?? 0} products
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
                <th>Product</th>
                <th>Status</th>
                <th>Version</th>
                <th>Criticality</th>
                <th>Updated</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {request.data.map((product) => (
                <tr key={product.productId}>
                  <td>
                    <button
                      className="name-cell"
                      onClick={() => openDetail(product.productId)}
                    >
                      <span className="product-icon">
                        {initials(product.productName)}
                      </span>
                      <span>
                        <strong>{product.productName}</strong>
                        <small>
                          {product.technologies || "No technologies listed"}
                        </small>
                      </span>
                    </button>
                  </td>
                  <td>
                    <span className={statusClass(product.lifecycleStatus)}>
                      {product.lifecycleStatus}
                    </span>
                  </td>
                  <td>{product.currentVersion || "—"}</td>
                  <td>
                    <span
                      className={`criticality criticality-${product.criticality?.toLowerCase()}`}
                    >
                      {product.criticality}
                    </span>
                  </td>
                  <td>{formatDate(product.updatedAt)}</td>
                  <td>
                    <div className="row-actions">
                      <Button
                        variant="ghost"
                        onClick={() => openDetail(product.productId)}
                      >
                        View
                      </Button>
                      {canManage && (<><Button variant="ghost" onClick={() => setModal(product)}>
                        Edit
                      </Button>
                      <Button
                        variant="danger-ghost"
                        onClick={() => remove(product)}
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
          title="No products found"
          text={
            query
              ? "Try a different search term."
              : canManage ? "Add your first product to start building the catalog." : "No products are available yet."
          }
          action={
            canManage && (<Button onClick={() => setModal("new")}>+ Add product</Button>)
          }
        />
      )}
      {canManage && modal && (
        <ProductForm
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
export function ProductDetail({ id, back, canManage }: { id: number; back: () => void; canManage: boolean }) {
  const request = useRequest<Entity>(`/products/${id}/details`);
  const [tab, setTab] = useState("Overview");
  if (request.loading)
    return (
      <>
        <Button variant="ghost" onClick={back}>
          ← Products
        </Button>
        <Loading />
      </>
    );
  if (request.error || !request.data)
    return (
      <>
        <Button variant="ghost" onClick={back}>
          ← Products
        </Button>
        <ErrorState
          message={request.error || "Product not found"}
          retry={request.reload}
        />
      </>
    );
  const product = request.data.product;
  const tabs = [
    "Overview",
    `Modules (${request.data.modules?.length ?? 0})`,
    `Deployments (${request.data.deployments?.length ?? 0})`,
    `Resources (${(request.data.repositories?.length ?? 0) + (request.data.documents?.length ?? 0)})`,
  ];
  return (
    <>
      <button className="back-link" onClick={back}>
        ← Back to products
      </button>
      <PageHeader
        eyebrow="PRODUCT DETAILS"
        title={product.productName}
        description={
          product.description || "Product information and operational context."
        }
        action={
          <span className={statusClass(product.lifecycleStatus)}>
            {product.lifecycleStatus}
          </span>
        }
      />
      <div className="detail-meta">
        <span>
          Version <b>{product.currentVersion || "Not set"}</b>
        </span>
        <span>
          Criticality <b>{product.criticality}</b>
        </span>
        <span>
          Updated <b>{formatDate(product.updatedAt)}</b>
        </span>
      </div>
      <div className="tabs">
        {tabs.map((item) => (
          <button
            className={tab === item.split(" (")[0] ? "active" : ""}
            key={item}
            onClick={() => setTab(item.split(" (")[0])}
          >
            {item}
          </button>
        ))}
      </div>
      {tab === "Overview" && (
        <div className="detail-grid">
          <section className="panel">
            <div className="panel-head">
              <h2>Product profile</h2>
            </div>
            <dl className="definition-list">
              <div>
                <dt>Business purpose</dt>
                <dd>{product.businessPurpose || "Not provided"}</dd>
              </div>
              <div>
                <dt>Supported markets</dt>
                <dd>{product.supportedMarkets || "Not provided"}</dd>
              </div>
              <div>
                <dt>Technologies</dt>
                <dd>{product.technologies || "Not provided"}</dd>
              </div>
              <div>
                <dt>Notes</dt>
                <dd>{product.notes || "No notes"}</dd>
              </div>
            </dl>
          </section>
          <ResponsibilityPanel kind="product" parentId={id}
            data={request.data.responsibilities ?? []} canManage={canManage} onChanged={request.reload} />
        </div>
      )}
      {tab === "Modules" && (
        <SimpleDetailTable
          title="Product modules"
          data={request.data.modules}
          columns={["moduleName", "status", "description"]}
          empty="No modules have been recorded."
        />
      )}
      {tab === "Deployments" && (
        <SimpleDetailTable
          title="Deployments"
          data={request.data.deployments}
          columns={[
            "clientName",
            "productVersion",
            "deploymentStatus",
            "environmentCount",
          ]}
          empty="No deployments for this product."
        />
      )}
      {tab === "Resources" && (
        <div className="detail-grid">
          <SimpleDetailTable
            title="Repositories"
            data={request.data.repositories}
            columns={["repositoryName", "mainBranch", "gitHubUrl"]}
            empty="No repositories linked."
          />
          <SimpleDetailTable
            title="Documents"
            data={request.data.documents}
            columns={["documentName", "documentType", "lastUpdatedDate"]}
            empty="No documents linked."
          />
        </div>
      )}
    </>
  );
}
