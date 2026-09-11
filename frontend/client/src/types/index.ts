export type User = {
  userId: number;
  fullName: string;
  email: string;
  roleName: string;
  isActive: boolean;
};
export type Page =
  | "dashboard"
  | "products"
  | "clients"
  | "deployments"
  | "environments"
  | "team"
  | "resources"
  | "admin"
  | "account";
/* eslint-disable @typescript-eslint/no-explicit-any */
export type Entity = Record<string, any>;
