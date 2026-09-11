import { useState } from "react";
import type { FormEvent } from "react";
import { api, useRequest } from "../lib/api";
import { initials } from "../lib/format";
import { Button, EmptyState, ErrorState, Field, Loading, Modal } from "./ui";

export type ResponsibilityDraft = {
  teamMemberId: string;
  responsibilityRole: string;
  description: string;
};

type TeamMember = { teamMemberId: number; fullName: string; status: string };
export type Responsibility = {
  responsibilityId?: number;
  clientResponsibilityId?: number;
  teamMemberId: number;
  teamMemberName: string;
  responsibilityRole: string;
  description?: string | null;
};

function AssignmentFields({ value, onChange, members }: {
  value: ResponsibilityDraft;
  onChange: (value: ResponsibilityDraft) => void;
  members: TeamMember[];
}) {
  return (
    <>
      <div className="form-grid">
        <Field label="Team member">
          <select required value={value.teamMemberId}
            onChange={(event) => onChange({ ...value, teamMemberId: event.target.value })}>
            <option value="">Select team member</option>
            {members.map((member) => (
              <option key={member.teamMemberId} value={member.teamMemberId}
                disabled={member.status !== "Active" && String(member.teamMemberId) !== value.teamMemberId}>
                {member.fullName}{member.status !== "Active" ? " (inactive)" : ""}
              </option>
            ))}
          </select>
        </Field>
        <Field label="Responsibility / role">
          <input required maxLength={100} value={value.responsibilityRole}
            placeholder="e.g. Technical owner, Account manager"
            onChange={(event) => onChange({ ...value, responsibilityRole: event.target.value })} />
        </Field>
      </div>
      <Field label="Responsibility description (optional)">
        <textarea rows={2} value={value.description}
          onChange={(event) => onChange({ ...value, description: event.target.value })} />
      </Field>
    </>
  );
}

export function ResponsibilityDrafts({ title, value, onChange, disabled }: {
  title: string;
  value: ResponsibilityDraft[];
  onChange: (value: ResponsibilityDraft[]) => void;
  disabled: boolean;
}) {
  const team = useRequest<TeamMember[]>("/team-members");
  return (
    <fieldset className="responsibility-fields" disabled={disabled}>
      <legend>{title} (optional)</legend>
      <p className="muted">Assign team members now or manage responsibilities from the details page later.</p>
      {team.loading ? <Loading /> : team.error ? (
        <ErrorState message={team.error} retry={team.reload} />
      ) : !team.data?.some((member) => member.status === "Active") ? (
        <p className="muted">Add an active employee on the Team members page before assigning responsibilities.</p>
      ) : null}
      {value.map((assignment, index) => (
        <fieldset key={index} className="responsibility-draft">
          <legend>Assignment {index + 1}</legend>
          <AssignmentFields value={assignment} members={team.data ?? []}
            onChange={(updated) => onChange(value.map((row, i) => i === index ? updated : row))} />
          <Button variant="danger-ghost" onClick={() => onChange(value.filter((_, i) => i !== index))}>
            Remove assignment
          </Button>
        </fieldset>
      ))}
      <Button variant="secondary"
        disabled={disabled || team.loading || Boolean(team.error) || !team.data?.some((member) => member.status === "Active")}
        onClick={() => onChange([...value, { teamMemberId: "", responsibilityRole: "", description: "" }])}>
        + Assign team member
      </Button>
    </fieldset>
  );
}

function ResponsibilityForm({ kind, parentId, item, close, saved }: {
  kind: "product" | "client";
  parentId: number;
  item?: Responsibility;
  close: () => void;
  saved: () => void;
}) {
  const team = useRequest<TeamMember[]>("/team-members");
  const [form, setForm] = useState<ResponsibilityDraft>({
    teamMemberId: item ? String(item.teamMemberId) : "",
    responsibilityRole: item?.responsibilityRole ?? "",
    description: item?.description ?? "",
  });
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!form.responsibilityRole.trim()) {
      setError("Enter a responsibility or role.");
      return;
    }
    setBusy(true);
    setError("");
    try {
      const id = item?.responsibilityId ?? item?.clientResponsibilityId;
      await api(`/${kind}-responsibilities${item ? `/${id}` : ""}`, {
        method: item ? "PUT" : "POST",
        body: JSON.stringify({
          [`${kind}Id`]: parentId,
          teamMemberId: Number(form.teamMemberId),
          responsibilityRole: form.responsibilityRole.trim(),
          description: form.description,
        }),
      });
      saved();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not save the responsibility.");
    } finally {
      setBusy(false);
    }
  }
  return (
    <Modal title={item ? "Edit responsibility" : "Assign team member"} onClose={() => { if (!busy) close(); }}>
      <form onSubmit={submit}>
        {team.loading ? <Loading /> : team.error ? (
          <ErrorState message={team.error} retry={team.reload} />
        ) : (
          <fieldset className="responsibility-fields" disabled={busy}>
            <AssignmentFields value={form} onChange={setForm} members={team.data ?? []} />
            {!team.data?.length && <p>Add an employee on the Team members page first.</p>}
          </fieldset>
        )}
        {error && <div className="form-error">{error}</div>}
        <div className="modal-actions">
          <Button variant="secondary" onClick={close} disabled={busy}>Cancel</Button>
          <Button type="submit" disabled={busy || team.loading || Boolean(team.error) || !form.teamMemberId}>
            {busy ? "Saving…" : "Save responsibility"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}

export function ResponsibilityPanel({ kind, parentId, data, canManage, onChanged }: {
  kind: "product" | "client";
  parentId: number;
  data: Responsibility[];
  canManage: boolean;
  onChanged: () => void;
}) {
  const [editing, setEditing] = useState<Responsibility | "new" | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  async function remove(item: Responsibility) {
    if (!confirm(`Remove ${item.teamMemberName} from this responsibility?`)) return;
    setBusy(true);
    setError("");
    try {
      await api(`/${kind}-responsibilities/${item.responsibilityId ?? item.clientResponsibilityId}`, { method: "DELETE" });
      onChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not remove the responsibility.");
    } finally {
      setBusy(false);
    }
  }
  return (
    <section className="panel">
      <div className="panel-head">
        <h2>{kind === "product" ? "Ownership" : "Account responsibilities"}</h2>
        {canManage && <Button disabled={busy} onClick={() => setEditing("new")}>+ Assign team member</Button>}
      </div>
      {error && <div className="form-error">{error}</div>}
      {data.length ? data.map((item) => (
        <div className="mini-row responsibility-row" key={item.responsibilityId ?? item.clientResponsibilityId}>
          <div className="avatar">{initials(item.teamMemberName)}</div>
          <div className="responsibility-person">
            <strong>{item.teamMemberName}</strong>
            <small>{item.responsibilityRole}</small>
            {item.description && <p>{item.description}</p>}
          </div>
          {canManage && <div className="row-actions">
            <Button variant="ghost" disabled={busy} onClick={() => setEditing(item)}>Edit</Button>
            <Button variant="danger-ghost" disabled={busy} onClick={() => remove(item)}>Remove</Button>
          </div>}
        </div>
      )) : <EmptyState title="No team members assigned"
        text={canManage ? "Assign a team member and describe their responsibility." : "Team responsibilities will appear here."} />}
      {canManage && editing && <ResponsibilityForm kind={kind} parentId={parentId}
        item={editing === "new" ? undefined : editing} close={() => setEditing(null)}
        saved={() => { setEditing(null); onChanged(); }} />}
    </section>
  );
}
