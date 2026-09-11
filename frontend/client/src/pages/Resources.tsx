/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from "react";
import type { FormEvent } from "react";
import { api, useRequest } from "../lib/api";
import { formatDate } from "../lib/format";
import {
  Button,
  EmptyState,
  Field,
  Loading,
  Modal,
  PageHeader,
} from "../components/ui";
import type { Entity } from "../types";

export function Resources({ canManage }: { canManage: boolean }) {
  const products = useRequest<Entity[]>("/products");
  const repos = useRequest<Entity[]>("/repositories");
  const docs = useRequest<Entity[]>("/documents");
  const [tab, setTab] = useState<"repositories" | "documents">("repositories");
  const [repoModal, setRepoModal] = useState(false);
  const [docModal, setDocModal] = useState(false);
  async function remove(path: string, reload: () => void) {
    if (!confirm("Delete this resource?")) return;
    try {
      await api(path, { method: "DELETE" });
      reload();
    } catch (err: any) {
      alert(err.message);
    }
  }
  return (
    <>
      <PageHeader
        eyebrow="KNOWLEDGE & CODE"
        title="Resources"
        description="Repositories and documents linked to your products."
        action={
          canManage && (<Button
            onClick={() =>
              tab === "repositories" ? setRepoModal(true) : setDocModal(true)
            }
          >
            + Add {tab === "repositories" ? "repository" : "document"}
          </Button>)
        }
      />
      <div className="tabs">
        <button
          className={tab === "repositories" ? "active" : ""}
          onClick={() => setTab("repositories")}
        >
          Repositories <span>{repos.data?.length ?? 0}</span>
        </button>
        <button
          className={tab === "documents" ? "active" : ""}
          onClick={() => setTab("documents")}
        >
          Documents <span>{docs.data?.length ?? 0}</span>
        </button>
      </div>
      {tab === "repositories" ? (
        <ResourceTable
          canManage={canManage}
          data={repos.data ?? []}
          loading={repos.loading}
          name="Repository"
          nameKey="repositoryName"
          columns={["productName", "mainBranch", "gitHubUrl"]}
          onDelete={(item) =>
            remove(`/repositories/${item.repositoryId}`, repos.reload)
          }
          add={() => setRepoModal(true)}
        />
      ) : (
        <ResourceTable
          canManage={canManage}
          data={docs.data ?? []}
          loading={docs.loading}
          name="Document"
          nameKey="documentName"
          columns={["productName", "documentType", "lastUpdatedDate"]}
          onDelete={(item) =>
            remove(`/documents/${item.documentId}`, docs.reload)
          }
          add={() => setDocModal(true)}
        />
      )}
      {canManage && repoModal && (
        <ResourceForm
          title="Add repository"
          products={products.data ?? []}
          type="repo"
          close={() => setRepoModal(false)}
          saved={() => {
            setRepoModal(false);
            repos.reload();
          }}
        />
      )}
      {canManage && docModal && (
        <ResourceForm
          title="Add document"
          products={products.data ?? []}
          type="doc"
          close={() => setDocModal(false)}
          saved={() => {
            setDocModal(false);
            docs.reload();
          }}
        />
      )}
    </>
  );
}
export function ResourceTable({
  canManage,
  data,
  loading,
  name,
  nameKey,
  columns,
  onDelete,
  add,
}: {
  canManage: boolean;
  data: Entity[];
  loading: boolean;
  name: string;
  nameKey: string;
  columns: string[];
  onDelete: (item: Entity) => void;
  add: () => void;
}) {
  if (loading) return <Loading />;
  return data.length ? (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>{name}</th>
            {columns.map((col) => (
              <th key={col}>{col.replace(/([A-Z])/g, " $1")}</th>
            ))}
            {canManage && <th />}
          </tr>
        </thead>
        <tbody>
          {data.map((item, index) => (
            <tr key={item.repositoryId ?? item.documentId ?? index}>
              <td>
                <strong>{item[nameKey]}</strong>
                <small className="table-sub">
                  #{item.repositoryId ?? item.documentId}
                </small>
              </td>
              {columns.map((col) => (
                <td key={col}>
                  {col.toLowerCase().includes("date") ? (
                    formatDate(item[col])
                  ) : col.toLowerCase().includes("url") && item[col] ? (
                    <a
                      className="table-link"
                      href={item[col]}
                      target="_blank"
                      rel="noreferrer"
                    >
                      Open ↗
                    </a>
                  ) : (
                    item[col] || "—"
                  )}
                </td>
              ))}
              {canManage && (<td>
                <Button variant="danger-ghost" onClick={() => onDelete(item)}>
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
      title={`No ${name.toLowerCase()}s found`}
      text={canManage ? `Link your first ${name.toLowerCase()} to a product.` : "No records are available yet."}
      action={canManage && (<Button onClick={add}>+ Add {name.toLowerCase()}</Button>)}
    />
  );
}
export function ResourceForm({
  title,
  products,
  type,
  close,
  saved,
}: {
  title: string;
  products: Entity[];
  type: "repo" | "doc";
  close: () => void;
  saved: () => void;
}) {
  const [form, setForm] = useState<any>(
    type === "repo"
      ? {
          productId: "",
          repositoryName: "",
          gitHubUrl: "",
          mainBranch: "main",
          description: "",
        }
      : {
          productId: "",
          documentName: "",
          documentType: "Technical Documentation",
          description: "",
          urlFileReference: "",
          lastUpdatedDate: "",
        },
  );
  const [error, setError] = useState("");
  const change = (key: string, value: string) =>
    setForm((old: Entity) => ({ ...old, [key]: value }));
  async function submit(e: FormEvent) {
    e.preventDefault();
    try {
      await api(type === "repo" ? "/repositories" : "/documents", {
        method: "POST",
        body: JSON.stringify({
          ...form,
          productId: Number(form.productId),
          lastUpdatedDate: form.lastUpdatedDate || null,
        }),
      });
      saved();
    } catch (err: any) {
      setError(err.message);
    }
  }
  return (
    <Modal title={title} onClose={close}>
      <form onSubmit={submit}>
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
        {type === "repo" ? (
          <>
            <Field label="Repository name">
              <input
                required
                value={form.repositoryName}
                onChange={(e) => change("repositoryName", e.target.value)}
              />
            </Field>
            <div className="form-grid">
              <Field label="GitHub URL">
                <input
                  required
                  type="url"
                  value={form.gitHubUrl}
                  onChange={(e) => change("gitHubUrl", e.target.value)}
                  placeholder="https://github.com/..."
                />
              </Field>
              <Field label="Main branch">
                <input
                  value={form.mainBranch}
                  onChange={(e) => change("mainBranch", e.target.value)}
                />
              </Field>
            </div>
          </>
        ) : (
          <>
            <Field label="Document name">
              <input
                required
                value={form.documentName}
                onChange={(e) => change("documentName", e.target.value)}
              />
            </Field>
            <div className="form-grid">
              <Field label="Document type">
                <select
                  required
                  value={form.documentType}
                  onChange={(e) => change("documentType", e.target.value)}
                >
                  {['Technical Documentation', 'Functional Documentation', 'Deployment Guide',
                    'Architecture Diagram', 'Postman Collection', 'API Documentation',
                    'Release Notes', 'User Guide', 'Other'].map((type) => (
                    <option key={type}>{type}</option>
                  ))}
                </select>
              </Field>
              <Field label="Last updated">
                <input
                  type="date"
                  value={form.lastUpdatedDate}
                  onChange={(e) => change("lastUpdatedDate", e.target.value)}
                />
              </Field>
            </div>
            <Field label="URL or file reference">
              <input
                required
                value={form.urlFileReference}
                onChange={(e) => change("urlFileReference", e.target.value)}
              />
            </Field>
          </>
        )}
        {error && <div className="form-error">{error}</div>}
        <div className="modal-actions">
          <Button variant="secondary" onClick={close}>
            Cancel
          </Button>
          <Button type="submit">Save resource</Button>
        </div>
      </form>
    </Modal>
  );
}
