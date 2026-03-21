"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useApiAccess } from "@/lib/posthog/use-api-access";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

interface ApiKey {
  id: string;
  name: string;
  keyPrefix: string;
  createdAt: string;
  lastUsedAt: string | null;
  expiresAt: string | null;
  isActive: boolean;
}

interface ApiKeyCreated extends ApiKey {
  apiKey: string;
}

export default function ApiKeysSettingsPage() {
  const router = useRouter();
  const hasApiAccess = useApiAccess();
  
  const [keys, setKeys] = useState<ApiKey[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [newKeyName, setNewKeyName] = useState("");
  const [newKeyValue, setNewKeyValue] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);
  const [error, setError] = useState<string | null>(null);

   useEffect(() => {
     if (!hasApiAccess) return;
     
     const fetchKeys = async () => {
       try {
         const response = await fetch(`${API_URL}/api/v1/auth/api-keys`, {
           credentials: "include",
         });

         if (response.ok) {
           const data = await response.json();
           setKeys(data);
         }
       } catch (err) {
         console.error("Failed to fetch API keys:", err);
       } finally {
         setIsLoading(false);
       }
     };

     fetchKeys();
   }, [hasApiAccess]);

   useEffect(() => {
     if (!hasApiAccess) {
       router.replace("/settings/profile");
     }
   }, [hasApiAccess, router]);
   
   if (!hasApiAccess) {
     return null;
   }

  const handleCreateKey = async () => {
    if (!newKeyName.trim()) return;

    setError(null);
    try {
      const response = await fetch(`${API_URL}/api/v1/auth/api-keys`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ name: newKeyName }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.error || "Failed to create API key");
      }

      const data: ApiKeyCreated = await response.json();
      setNewKeyValue(data.apiKey);
      setKeys([...keys, data]);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create API key");
    }
  };

  const handleCopyKey = async () => {
    if (newKeyValue) {
      await navigator.clipboard.writeText(newKeyValue);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  const handleRevokeKey = async (id: string) => {
    if (!confirm("Are you sure you want to revoke this API key? This action cannot be undone.")) {
      return;
    }

    try {
      const response = await fetch(`${API_URL}/api/v1/auth/api-keys/${id}`, {
        method: "DELETE",
        credentials: "include",
      });

      if (response.ok || response.status === 204) {
        setKeys(keys.filter((key) => key.id !== id));
      } else {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.error || "Failed to revoke API key");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to revoke API key");
    }
  };

  const handleCloseNewKey = () => {
    setNewKeyValue(null);
    setNewKeyName("");
    setIsCreating(false);
  };

  return (
    <div className="space-y-6">
      <section className="rounded-lg border border-border-subtle bg-bg-elevated p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-xl font-semibold text-text-primary">API Keys</h2>
            <p className="mt-1 text-sm text-text-muted">
              Manage your API keys for programmatic access to VoiceProcessor.
            </p>
          </div>
          {!isCreating && !newKeyValue && (
            <button
              onClick={() => setIsCreating(true)}
              className="rounded-lg bg-indigo px-4 py-2 text-sm text-white hover:bg-indigo-dark"
            >
              Create New Key
            </button>
          )}
        </div>

        {error && (
          <div className="mb-4 p-3 rounded-lg bg-state-error-bg border border-state-error-border text-sm text-state-error-text">
            {error}
          </div>
        )}

        {isCreating && !newKeyValue && (
          <div className="mb-6 p-4 rounded-lg border border-border-subtle bg-bg-sunken">
            <h3 className="text-sm font-medium text-text-primary mb-3">Create New API Key</h3>
            <div className="flex gap-3">
              <input
                type="text"
                value={newKeyName}
                onChange={(e) => setNewKeyName(e.target.value)}
                placeholder="Key name (e.g., Production, Development)"
                className="flex-1 rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-sm text-text-primary placeholder-text-muted"
              />
              <button
                onClick={handleCreateKey}
                disabled={!newKeyName.trim()}
                className="rounded-lg bg-indigo px-4 py-2 text-sm text-white hover:bg-indigo-dark disabled:opacity-50"
              >
                Create
              </button>
              <button
                onClick={() => setIsCreating(false)}
                className="rounded-lg border border-border-subtle px-4 py-2 text-sm text-text-secondary hover:bg-bg-sunken"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        {newKeyValue && (
          <div className="mb-6 p-4 rounded-lg border border-state-success-border bg-success-subtle">
            <div className="flex items-start gap-3">
              <div className="flex-shrink-0 text-xl">🔑</div>
              <div className="flex-1 min-w-0">
                <h3 className="text-sm font-medium text-state-success-text">
                  API Key Created Successfully
                </h3>
                <p className="mt-1 text-sm text-state-success-text">
                  Copy this key now. You won&apos;t be able to see it again.
                </p>
                <div className="mt-3 flex items-center gap-2">
                  <code className="flex-1 rounded bg-bg-elevated px-3 py-2 text-sm font-mono text-text-primary border border-border-subtle break-all">
                    {newKeyValue}
                  </code>
                  <button
                    onClick={handleCopyKey}
                    className="rounded-lg bg-success px-4 py-2 text-sm text-white hover:bg-success/80"
                  >
                    {copied ? "Copied!" : "Copy"}
                  </button>
                </div>
                <button
                  onClick={handleCloseNewKey}
                  className="mt-3 text-sm text-state-success-text hover:underline"
                >
                  I&apos;ve saved my key
                </button>
              </div>
            </div>
          </div>
        )}

        {isLoading && (
          <div className="py-8 text-center text-text-muted">
            Loading API keys...
          </div>
        )}

        {!isLoading && keys.length > 0 ? (
          <div className="border border-border-subtle rounded-lg overflow-hidden">
            <table className="w-full">
              <thead className="bg-bg-sunken">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-text-muted uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-text-muted uppercase tracking-wider">
                    Key
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-text-muted uppercase tracking-wider">
                    Created
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-text-muted uppercase tracking-wider">
                    Last Used
                  </th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-text-muted uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border-subtle">
                {keys.map((key) => (
                  <tr key={key.id}>
                    <td className="px-4 py-3 text-sm text-text-primary">
                      {key.name}
                    </td>
                    <td className="px-4 py-3 text-sm font-mono text-text-muted">
                      {key.keyPrefix}...
                    </td>
                    <td className="px-4 py-3 text-sm text-text-muted">
                      {new Date(key.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-4 py-3 text-sm text-text-muted">
                      {key.lastUsedAt ? new Date(key.lastUsedAt).toLocaleDateString() : "Never"}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        onClick={() => handleRevokeKey(key.id)}
                        className="text-sm text-error hover:underline"
                      >
                        Revoke
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : !isLoading ? (
          <div className="text-center py-8 text-text-muted">
            <p>No API keys yet.</p>
            <p className="text-sm mt-1">Create your first API key to get started.</p>
          </div>
        ) : null}
      </section>

      <section className="rounded-lg border border-border-subtle bg-indigo-subtle p-6">
        <h3 className="text-sm font-medium text-indigo mb-2">Using API Keys</h3>
        <p className="text-sm text-indigo mb-3">
          Include your API key in the <code className="bg-bg-sunken px-1 rounded">X-API-Key</code> header when making requests.
        </p>
        <pre className="bg-bg-elevated rounded-lg p-3 text-sm font-mono text-text-primary overflow-x-auto">
{`curl -X POST https://api.voiceprocessor.com/api/v1/generations \\
  -H "X-API-Key: vp_your_api_key_here" \\
  -H "Content-Type: application/json" \\
  -d '{"text": "Hello world", "voiceId": "..."}'`}
        </pre>
      </section>
    </div>
  );
}
