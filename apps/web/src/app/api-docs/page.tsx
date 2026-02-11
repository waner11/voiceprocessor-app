"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useApiAccess } from "@/lib/posthog/use-api-access";

export default function ApiDocsPage() {
  const router = useRouter();
  const hasAccess = useApiAccess();

  useEffect(() => {
    if (!hasAccess) {
      router.replace("/");
    }
  }, [hasAccess, router]);

  if (!hasAccess) {
    return null;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold">API Documentation</h1>

      <div className="grid gap-8 lg:grid-cols-4">
        <nav className="lg:col-span-1">
          <h2 className="mb-4 font-semibold">Endpoints</h2>
          <ul className="space-y-2 text-sm">
            <li>
              <a href="#authentication" className="hover:underline">
                Authentication
              </a>
            </li>
            <li>
              <a href="#generations" className="hover:underline">
                Generations
              </a>
            </li>
            <li>
              <a href="#voices" className="hover:underline">
                Voices
              </a>
            </li>
            <li>
              <a href="#usage" className="hover:underline">
                Usage
              </a>
            </li>
            <li>
              <a href="#webhooks" className="hover:underline">
                Webhooks
              </a>
            </li>
          </ul>
        </nav>

        <main className="lg:col-span-3 space-y-12">
          <section id="authentication">
            <h2 className="mb-4 text-2xl font-semibold">Authentication</h2>
            <p className="mb-4 text-gray-600">
              Authenticate API requests using Bearer tokens in the Authorization
              header.
            </p>
            <pre className="rounded-lg bg-gray-900 p-4 text-sm text-gray-100 overflow-x-auto">
              {`curl -H "Authorization: Bearer YOUR_API_KEY" \\
  https://api.voiceprocessor.com/v1/generations`}
            </pre>
          </section>

          <section id="generations">
            <h2 className="mb-4 text-2xl font-semibold">Generations</h2>

            <div className="space-y-6">
              <div>
                <h3 className="mb-2 font-semibold">
                  <span className="rounded bg-green-100 px-2 py-1 text-sm text-green-800">
                    POST
                  </span>{" "}
                  /v1/generations
                </h3>
                <p className="text-gray-600">Create a new audio generation.</p>
              </div>

              <div>
                <h3 className="mb-2 font-semibold">
                  <span className="rounded bg-blue-100 px-2 py-1 text-sm text-blue-800">
                    GET
                  </span>{" "}
                  /v1/generations/:id
                </h3>
                <p className="text-gray-600">Get generation details by ID.</p>
              </div>

              <div>
                <h3 className="mb-2 font-semibold">
                  <span className="rounded bg-green-100 px-2 py-1 text-sm text-green-800">
                    POST
                  </span>{" "}
                  /v1/generations/estimate
                </h3>
                <p className="text-gray-600">
                  Get cost estimate for a generation.
                </p>
              </div>
            </div>
          </section>

          <section id="voices">
            <h2 className="mb-4 text-2xl font-semibold">Voices</h2>
            <div>
              <h3 className="mb-2 font-semibold">
                <span className="rounded bg-blue-100 px-2 py-1 text-sm text-blue-800">
                  GET
                </span>{" "}
                /v1/voices
              </h3>
              <p className="text-gray-600">List all available voices.</p>
            </div>
          </section>

          <section id="usage">
            <h2 className="mb-4 text-2xl font-semibold">Usage</h2>
            <div>
              <h3 className="mb-2 font-semibold">
                <span className="rounded bg-blue-100 px-2 py-1 text-sm text-blue-800">
                  GET
                </span>{" "}
                /v1/user/usage
              </h3>
              <p className="text-gray-600">Get current usage and credits.</p>
            </div>
          </section>

          <section id="webhooks">
            <h2 className="mb-4 text-2xl font-semibold">Webhooks</h2>
            <p className="text-gray-600">
              Configure webhooks in Settings to receive POST notifications when
              generations complete.
            </p>
          </section>
        </main>
      </div>
    </div>
  );
}
