import { formatDate, statusClass } from "../lib/format";
import { EmptyState } from "./ui";
import type { Entity } from "../types";

export function SimpleDetailTable({
  title,
  data = [],
  columns,
  empty,
}: {
  title: string;
  data: Entity[];
  columns: string[];
  empty: string;
}) {
  return (
    <section className="panel">
      <div className="panel-head">
        <h2>{title}</h2>
        <span className="muted">{data.length} records</span>
      </div>
      {data.length ? (
        <div className="mini-table">
          {data.map((item, index) => (
            <div
              className="mini-table-row"
              key={
                item.id ??
                item.moduleId ??
                item.repositoryId ??
                item.documentId ??
                index
              }
            >
              {columns.map((column) => (
                <div key={column}>
                  <small>{column.replace(/([A-Z])/g, " $1")}</small>
                  <span
                    className={
                      column.toLowerCase().includes("status")
                        ? statusClass(item[column])
                        : ""
                    }
                  >
                    {column.toLowerCase().includes("date")
                      ? formatDate(item[column])
                      : item[column] || "—"}
                  </span>
                </div>
              ))}
            </div>
          ))}
        </div>
      ) : (
        <EmptyState title={empty} text="" />
      )}
    </section>
  );
}
