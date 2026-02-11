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
  
  // All hooks must be called before conditional returns
  const [keys, setKeys] = useState<ApiKey[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [newKeyName, setNewKeyName] = useState("");
  const [newKeyValue, setNewKeyValue] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);
  const [error, setError] = useState<string | null>(null);

   // Fetch existing API keys on mount
   useEffect(() => {
     if (!hasApiAccess) return;
     
     const fetchKeys = async () => {
       try {
         const response = await fetch(`${API_URL}/api/v1/Auth/api-keys`, {
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

   // Redirect if no API access
   useEffect(() => {
     if (!hasApiAccess) {
       router.replace("/settings/profile");
     }
   }, [hasApiAccess, router]);
   
   // Redirect guard after all hooks
   if (!hasApiAccess) {
     return null;
   }

  const handleCreateKey = async () => {
    if (!newKeyName.trim()) return;

    setError(null);
    try {
      const response = await fetch(`${API_URL}/api/v1/Auth/api-keys`, {
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
      const response = await fetch(`${API_URL}/api/v1/Auth/api-keys/${id}`, {
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
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white">API Keys</h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              Manage your API keys for programmatic access to VoiceProcessor.
            </p>
          </div>
          {!isCreating && !newKeyValue && (
            <button
              onClick={() => setIsCreating(true)}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
            >
              Create New Key
            </button>
          )}
        </div>

        {/* Error Message */}
        {error && (
          <div className="mb-4 p-3 rounded-lg bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-800 text-sm text-red-600 dark:text-red-400">
            {error}
          </div>
        )}

        {/* New Key Creation Form */}
        {isCreating && !newKeyValue && (
          <div className="mb-6 p-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800">
            <h3 className="text-sm font-medium text-gray-900 dark:text-white mb-3">Create New API Key</h3>
            <div className="flex gap-3">
              <input
                type="text"
                value={newKeyName}
                onChange={(e) => setNewKeyName(e.target.value)}
                placeholder="Key name (e.g., Production, Development)"
                className="flex-1 rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-700 px-4 py-2 text-sm text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
              />
              <button
                onClick={handleCreateKey}
                disabled={!newKeyName.trim()}
                className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
              >
                Create
              </button>
              <button
                onClick={() => setIsCreating(false)}
                className="rounded-lg border border-gray-200 dark:border-gray-600 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        {/* New Key Display (shown once after creation) */}
        {newKeyValue && (
          <div className="mb-6 p-4 rounded-lg border border-green-200 dark:border-green-900 bg-green-50 dark:bg-green-950/30">
            <div className="flex items-start gap-3">
              <div className="flex-shrink-0 text-xl">🔑</div>
              <div className="flex-1 min-w-0">
                <h3 className="text-sm font-medium text-green-800 dark:text-green-300">
                  API Key Created Successfully
                </h3>
                <p className="mt-1 text-sm text-green-700 dark:text-green-400">
                  Copy this key now. You won&apos;t be able to see it again.
                </p>
                <div className="mt-3 flex items-center gap-2">
                  <code className="flex-1 rounded bg-white dark:bg-gray-800 px-3 py-2 text-sm font-mono text-gray-900 dark:text-white border border-green-200 dark:border-green-800 break-all">
                    {newKeyValue}
                  </code>
                  <button
                    onClick={handleCopyKey}
                    className="rounded-lg bg-green-600 px-4 py-2 text-sm text-white hover:bg-green-700"
                  >
                    {copied ? "Copied!" : "Copy"}
                  </button>
                </div>
                <button
                  onClick={handleCloseNewKey}
                  className="mt-3 text-sm text-green-700 dark:text-green-400 hover:underline"
                >
                  I&apos;ve saved my key
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Loading State */}
        {isLoading && (
          <div className="py-8 text-center text-gray-500 dark:text-gray-400">
            Loading API keys...
          </div>
        )}

        {/* Keys List */}
        {!isLoading && keys.length > 0 ? (
          <div className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Key
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Created
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Last Used
                  </th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                {keys.map((key) => (
                  <tr key={key.id}>
                    <td className="px-4 py-3 text-sm text-gray-900 dark:text-white">
                      {key.name}
                    </td>
                    <td className="px-4 py-3 text-sm font-mono text-gray-500 dark:text-gray-400">
                      {key.keyPrefix}...
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">
                      {new Date(key.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">
                      {key.lastUsedAt ? new Date(key.lastUsedAt).toLocaleDateString() : "Never"}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        onClick={() => handleRevokeKey(key.id)}
                        className="text-sm text-red-600 dark:text-red-400 hover:underline"
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
          <div className="text-center py-8 text-gray-500 dark:text-gray-400">
            <p>No API keys yet.</p>
            <p className="text-sm mt-1">Create your first API key to get started.</p>
          </div>
        ) : null}
      </section>

      {/* Usage Info */}
      <section className="rounded-lg border border-blue-200 dark:border-blue-900 bg-blue-50 dark:bg-blue-950/30 p-6">
        <h3 className="text-sm font-medium text-blue-800 dark:text-blue-300 mb-2">Using API Keys</h3>
        <p className="text-sm text-blue-700 dark:text-blue-400 mb-3">
          Include your API key in the <code className="bg-blue-100 dark:bg-blue-900 px-1 rounded">X-API-Key</code> header when making requests.
        </p>
        <pre className="bg-white dark:bg-gray-800 rounded-lg p-3 text-sm font-mono text-gray-800 dark:text-gray-200 overflow-x-auto">
{`curl -X POST https://api.voiceprocessor.com/api/v1/generations \\
  -H "X-API-Key: vp_your_api_key_here" \\
  -H "Content-Type: application/json" \\
  -d '{"text": "Hello world", "voiceId": "..."}'`}
        </pre>
      </section>
    </div>
  );
}
