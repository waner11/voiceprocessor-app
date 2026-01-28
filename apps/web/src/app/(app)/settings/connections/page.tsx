"use client";

import { useState } from "react";

interface OAuthConnection {
  provider: "google" | "github";
  providerId: string;
  email: string;
  linkedAt: string;
}

const providerInfo = {
  google: {
    name: "Google",
    icon: (
      <svg className="h-5 w-5" viewBox="0 0 24 24">
        <path
          fill="currentColor"
          d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
        />
        <path
          fill="currentColor"
          d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
        />
        <path
          fill="currentColor"
          d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
        />
        <path
          fill="currentColor"
          d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
        />
      </svg>
    ),
    color: "text-red-500",
  },
  github: {
    name: "GitHub",
    icon: (
      <svg className="h-5 w-5" fill="currentColor" viewBox="0 0 24 24">
        <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
      </svg>
    ),
    color: "text-gray-900 dark:text-white",
  },
};

export default function ConnectionsSettingsPage() {
  const [connections, setConnections] = useState<OAuthConnection[]>([]);
  const [isLinking, setIsLinking] = useState<string | null>(null);

  const handleLinkProvider = async (provider: "google" | "github") => {
    setIsLinking(provider);
    try {
      // TODO: Call API to get OAuth URL and redirect
      // const response = await api.GET(`/api/v1/auth/oauth/${provider}/url`);
      // window.location.href = response.data.authorizationUrl;

      // Mock for demo
      await new Promise((resolve) => setTimeout(resolve, 1000));
      alert(`Redirecting to ${providerInfo[provider].name} for authorization...`);
    } catch (error) {
      console.error(`Failed to link ${provider}:`, error);
    } finally {
      setIsLinking(null);
    }
  };

  const handleUnlinkProvider = async (provider: "google" | "github") => {
    const connection = connections.find((c) => c.provider === provider);
    if (!connection) return;

    // Check if this is the only login method
    const hasPassword = true; // TODO: Check from user profile
    const otherConnections = connections.filter((c) => c.provider !== provider);

    if (!hasPassword && otherConnections.length === 0) {
      alert("You cannot unlink your only login method. Please set a password first.");
      return;
    }

    if (!confirm(`Are you sure you want to unlink your ${providerInfo[provider].name} account?`)) {
      return;
    }

    try {
      // TODO: Call API to unlink
      // await api.DELETE(`/api/v1/auth/oauth/${provider}`);
      setConnections(connections.filter((c) => c.provider !== provider));
    } catch (error) {
      console.error(`Failed to unlink ${provider}:`, error);
    }
  };

  const isConnected = (provider: "google" | "github") =>
    connections.some((c) => c.provider === provider);

  return (
    <div className="space-y-6">
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <div className="mb-6">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Connected Accounts</h2>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Link your social accounts to enable single sign-on.
          </p>
        </div>

        <div className="space-y-4">
          {(["google", "github"] as const).map((provider) => {
            const info = providerInfo[provider];
            const connection = connections.find((c) => c.provider === provider);
            const connected = !!connection;

            return (
              <div
                key={provider}
                className="flex items-center justify-between p-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
              >
                <div className="flex items-center gap-4">
                  <div className={`${info.color}`}>{info.icon}</div>
                  <div>
                    <h3 className="font-medium text-gray-900 dark:text-white">{info.name}</h3>
                    {connected ? (
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        Connected as {connection.email}
                      </p>
                    ) : (
                      <p className="text-sm text-gray-500 dark:text-gray-400">Not connected</p>
                    )}
                  </div>
                </div>

                {connected ? (
                  <button
                    onClick={() => handleUnlinkProvider(provider)}
                    className="rounded-lg border border-gray-200 dark:border-gray-600 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
                  >
                    Disconnect
                  </button>
                ) : (
                  <button
                    onClick={() => handleLinkProvider(provider)}
                    disabled={isLinking === provider}
                    className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
                  >
                    {isLinking === provider ? "Connecting..." : "Connect"}
                  </button>
                )}
              </div>
            );
          })}
        </div>
      </section>

      {/* Benefits Info */}
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h3 className="text-sm font-medium text-gray-900 dark:text-white mb-3">
          Benefits of connecting accounts
        </h3>
        <ul className="space-y-2 text-sm text-gray-600 dark:text-gray-400">
          <li className="flex items-start gap-2">
            <span className="text-green-500 mt-0.5">✓</span>
            <span>Sign in faster with one click</span>
          </li>
          <li className="flex items-start gap-2">
            <span className="text-green-500 mt-0.5">✓</span>
            <span>No need to remember another password</span>
          </li>
          <li className="flex items-start gap-2">
            <span className="text-green-500 mt-0.5">✓</span>
            <span>Multiple login options for account recovery</span>
          </li>
        </ul>
      </section>
    </div>
  );
}
