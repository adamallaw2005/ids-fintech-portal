import { useCallback, useEffect, useRef, useState } from "react";

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5149/api";

export async function api(path: string, options: RequestInit = {}) {
  const token = localStorage.getItem("ids_token");
  const headers = new Headers(options.headers);
  headers.set("Content-Type", "application/json");
  if (token) headers.set("Authorization", `Bearer ${token}`);

  const response = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (response.status === 401) {
    localStorage.removeItem("ids_token");
    localStorage.removeItem("ids_user");
    window.dispatchEvent(new Event("ids-auth-expired"));
  }

  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    try {
      const text = await response.text();
      const body = response.headers.get("content-type")?.includes("json")
        ? JSON.parse(text)
        : text;
      const validationErrors = body?.errors
        ? Object.values(body.errors).flat().join(" ")
        : "";
      message = validationErrors || (
        body?.detail ??
        body?.title ??
        body?.message ??
        (typeof body === "string" && body ? body : message));
    } catch {
      // Use the HTTP status if the error body cannot be read.
    }
    throw new Error(message);
  }

  if (response.status === 204) return null;
  return response.json();
}

export function useRequest<T>(path: string | null) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(Boolean(path));
  const [error, setError] = useState("");
  const activeRequest = useRef<AbortController | null>(null);

  const reload = useCallback(() => {
    activeRequest.current?.abort();
    if (!path) {
      setData(null);
      setLoading(false);
      setError("");
      return;
    }

    const controller = new AbortController();
    activeRequest.current = controller;
    setLoading(true);
    setError("");
    api(path, { signal: controller.signal })
      .then((result) => {
        if (!controller.signal.aborted) setData(result);
      })
      .catch((err) => {
        if (!controller.signal.aborted) setError(err.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });
  }, [path]);

  useEffect(() => {
    const timer = window.setTimeout(reload, 0);
    return () => {
      window.clearTimeout(timer);
      activeRequest.current?.abort();
    };
  }, [reload]);

  return { data, loading, error, reload };
}
