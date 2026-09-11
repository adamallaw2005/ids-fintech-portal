import { useRequest } from "../lib/api";
import { formatDate, initials, statusClass } from "../lib/format";
import {
  Button,
  EmptyState,
  ErrorState,
  Loading,
  PageHeader,
} from "../components/ui";
import type { Entity, Page } from "../types";

export function Dashboard({ go, canManage }: { go: (page: Page) => void; canManage: boolean }) {
  const request = useRequest<Entity>("/dashboard");

  if (request.loading) return <Loading />;
  if (request.error || !request.data)
    return (
      <ErrorState
        message={request.error || "Dashboard data is unavailable."}
        retry={request.reload}
      />
    );

  const dashboard = request.data;
  const stats = [
    {
      label: "Products",
      value: dashboard.productsCount,
      note: `${dashboard.activeProductsCount} active`,
      page: "products" as Page,
    },
    {
      label: "Clients",
      value: dashboard.clientsCount,
      note: "accounts in the directory",
      page: "clients" as Page,
    },
    {
      label: "Deployments",
      value: dashboard.deploymentsCount,
      note: "tracked product releases",
      page: "deployments" as Page,
    },
    {
      label: "Team members",
      value: dashboard.teamMembersCount,
      note: "people with ownership context",
      page: "team" as Page,
    },
  ];

  return (
    <>
      <PageHeader
        eyebrow="IDS FINTECH PORTAL"
        title="Overview"
        description="A quick view of your products, clients, deployments, and ownership."
      />
      <div className="stat-grid">
        {stats.map((stat) => (
          <button
            className="stat-card"
            key={stat.label}
            onClick={() => go(stat.page)}
          >
            <span>{stat.label}</span>
            <strong>{stat.value ?? 0}</strong>
            <small>{stat.note}</small>
          </button>
        ))}
      </div>
      <div className="dashboard-grid">
        <section className="panel">
          <div className="panel-head">
            <div>
              <h2>Recently updated products</h2>
              <p>The latest changes in your product catalog.</p>
            </div>
            <Button variant="ghost" onClick={() => go("products")}>
              View all
            </Button>
          </div>
          {dashboard.recentlyUpdatedProducts?.length ? (
            <div className="recent-list">
              {dashboard.recentlyUpdatedProducts.map((product: Entity) => (
                <button
                  className="recent-row"
                  key={product.productId}
                  onClick={() => go("products")}
                >
                  <span className="product-icon">
                    {initials(product.productName)}
                  </span>
                  <span className="recent-main">
                    <strong>{product.productName}</strong>
                    <span>{product.currentVersion || "Version not set"}</span>
                  </span>
                  <span className={statusClass(product.lifecycleStatus)}>
                    {product.lifecycleStatus}
                  </span>
                  <time>{formatDate(product.updatedAt)}</time>
                  <span className="row-arrow">›</span>
                </button>
              ))}
            </div>
          ) : (
            <EmptyState
              title="No products yet"
              text={canManage ? "Add a product to start building your catalog." : "No products are available yet."}
              action={
                canManage && <Button onClick={() => go("products")}>Add product</Button>
              }
            />
          )}
        </section>
        <section className="panel quick-panel">
          <div className="panel-head">
            <div>
              <h2>Common actions</h2>
              <p>Jump into the work you do most often.</p>
            </div>
          </div>
          <button className="quick-action" onClick={() => go("products")}>
            <span className="action-mark mark-blue">PR</span>
            <span>
              <strong>Manage products</strong>
              <small>Versions, status, and ownership</small>
            </span>
            <b>›</b>
          </button>
          <button className="quick-action" onClick={() => go("clients")}>
            <span className="action-mark mark-orange">CL</span>
            <span>
              <strong>Review clients</strong>
              <small>Accounts and deployment context</small>
            </span>
            <b>›</b>
          </button>
          <button className="quick-action" onClick={() => go("deployments")}>
            <span className="action-mark mark-green">DP</span>
            <span>
              <strong>Track deployments</strong>
              <small>Release status and go-live dates</small>
            </span>
            <b>›</b>
          </button>
        </section>
      </div>
    </>
  );
}
