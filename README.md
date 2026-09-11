# IDS Products Portal

A React and TypeScript frontend with an ASP.NET Core 9 API, Dapper, and SQL Server. The portal manages products, clients, deployments, environments, team responsibilities, repositories, documents, and user accounts.

## Run locally

1. Install .NET 9 SDK, Node.js 22.12 or later, and SQL Server 2022 (or a newer compatible version).
2. Open the included IDSProductsPortal.sql in SQL Server Management Studio and execute it on a server where IDSProductsPortal does not already exist. It creates the database, tables, and exported data using the server's default data-file locations. It is a fresh database setup script, not an update script for an existing database.
3. In backend/MyWebsite_API, configure your local settings and start the API:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=IDSProductsPortal;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True"
dotnet user-secrets set "JwtSettings:Key" "YOUR_RANDOM_SIGNING_KEY_AT_LEAST_32_BYTES"
dotnet restore
dotnet run --launch-profile http
```

Replace both example values. The existing development connection points to the original local SQL Server. Keep the JWT signing key outside the submitted source.

4. In frontend/client, run npm ci, then npm run dev. Open the localhost URL printed by Vite. The API runs on http://localhost:5149; the frontend expects http://localhost:5149/api. Override it with VITE_API_URL in .env.local if needed. Allowed frontend origins are localhost ports 5173 and 5174.
5. Sign in with an active account in the database. The included administrator email is admin@idsfintech.com; obtain the demo password from the project author.

## Verification

From the project root:

```powershell
dotnet build backend/MyWebsite_API/MyWebsite_API.sln
cd frontend/client
npm run lint
npm run build
```

Build and lint checks detect compilation and code-quality issues. They do not verify every application behavior. To check behavior manually, sign in as an admin, create and edit a temporary record with assignments, reopen it to confirm the changes, then remove the temporary record. Sign in as a normal user and confirm that records can be viewed but management controls are unavailable.

Controllers handle HTTP requests, services hold application logic, and repositories run parameterized SQL. Authentication uses JWT bearer tokens and PBKDF2 password hashes.

## Access permissions

Normal users can view records, open details, search, and filter. Only admins can create, update, or delete products, clients, deployments, environments, modules, team members, responsibilities, repositories, and documents. These rules are enforced by API authorization and by hiding management controls in the frontend.

Both roles can change their own password. The Become an admin flow remains available to normal users who supply the promotion password. Successful promotion updates the token and user state so admin controls become available. User management and promotion-password configuration remain admin-only.


## Assigning ownership and account responsibilities

On **Team members**, click **View** to see a person's assigned products and clients, including roles and descriptions. Admins can manage both lists in **Add team member** and **Edit team member**. Deleting a member removes their assignments but keeps the products and clients. Member fields and assignment changes save in one transaction.

When adding a product or client as an admin, use **Assign team member** in the optional Ownership or Account responsibilities section. Select an active employee, enter their role, and optionally add a description. You can add multiple assignments. Saving creates the record and all its initial assignments in one transaction, then opens its details page.

Existing assignments can be added, edited, or removed from the Ownership or Account responsibilities panel on the details page. Normal users can view these assignments only. Team members are employee records from the Team members page, not portal login accounts.

The Product and Client **Edit** forms also prefill existing assignments and support adding, editing, and removing them. Changes are applied only when Save is clicked; Cancel discards the draft. Save is blocked if existing assignments cannot be loaded. Record fields and assignment changes save in one transaction, so a failed save preserves the existing data.
