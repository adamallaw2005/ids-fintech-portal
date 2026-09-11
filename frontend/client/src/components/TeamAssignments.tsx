import { Button, ErrorState, Field, Loading } from "./ui";
import { useRequest } from "../lib/api";
import type { Entity } from "../types";

export type TeamAssignment = { targetId: string; responsibilityRole: string; description: string };

export function TeamAssignmentFields({ kind, value, onChange }: {
  kind: "product" | "client";
  value: TeamAssignment[];
  onChange: (value: TeamAssignment[]) => void;
}) {
  const options = useRequest<Entity[]>(`/${kind}s`);
  const title = kind === "product" ? "Products" : "Clients";
  function change(index: number, patch: Partial<TeamAssignment>) {
    onChange(value.map((row, i) => i === index ? { ...row, ...patch } : row));
  }
  return <section className="responsibility-fields">
    <h3>{title}</h3>
    {options.loading ? <Loading /> : options.error ? <ErrorState message={options.error} retry={options.reload} /> : <>
      {value.map((row, index) => <div className="responsibility-draft" key={index}>
        <Field label={kind === "product" ? "Product" : "Client"}>
          <select required value={row.targetId} onChange={e => change(index, { targetId: e.target.value })}>
            <option value="">Select {kind}</option>
            {(options.data ?? []).map(option => <option key={option[`${kind}Id`]} value={option[`${kind}Id`]}>
              {kind === "product" ? option.productName : option.companyName}
            </option>)}
          </select>
        </Field>
        <Field label="Responsibility role">
          <input required maxLength={100} value={row.responsibilityRole} onChange={e => change(index, { responsibilityRole: e.target.value })} />
        </Field>
        <Field label="Description">
          <textarea rows={2} value={row.description} onChange={e => change(index, { description: e.target.value })} />
        </Field>
        <Button variant="danger-ghost" onClick={() => onChange(value.filter((_, i) => i !== index))}>Remove {kind}</Button>
      </div>)}
      {!value.length && <p>No {title.toLowerCase()} assigned.</p>}
      <Button variant="secondary" onClick={() => onChange([...value, { targetId: "", responsibilityRole: "", description: "" }])}>+ Add {kind}</Button>
    </>}
  </section>;
}

export function TeamAssignmentList({ kind, memberId }: { kind: "product" | "client"; memberId: number }) {
  const request = useRequest<Entity[]>(`/${kind}-responsibilities?teamMemberId=${memberId}`);
  return <section className="responsibility-fields">
    <h3>{kind === "product" ? "Products" : "Clients"}</h3>
    {request.loading ? <Loading /> : request.error ? <ErrorState message={request.error} retry={request.reload} /> :
      request.data?.length ? request.data.map(row => <div className="responsibility-row" key={row.responsibilityId ?? row.clientResponsibilityId}>
        <div><strong>{kind === "product" ? row.productName : row.companyName}</strong>
          <p>{row.responsibilityRole}</p>{row.description && <p>{row.description}</p>}
        </div>
      </div>) : <p>No {kind}s assigned.</p>}
  </section>;
}
