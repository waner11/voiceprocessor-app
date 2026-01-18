export default function SettingsPage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold">Settings</h1>

      <div className="max-w-2xl space-y-8">
        {/* Account */}
        <section className="rounded-lg border p-6">
          <h2 className="mb-4 text-xl font-semibold">Account</h2>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Email</label>
              <input
                type="email"
                className="w-full rounded-lg border px-4 py-2"
                placeholder="your@email.com"
                disabled
              />
            </div>
            <button className="rounded-lg border px-4 py-2 text-sm hover:bg-gray-50">
              Change Password
            </button>
          </div>
        </section>

        {/* API Keys */}
        <section className="rounded-lg border p-6">
          <h2 className="mb-4 text-xl font-semibold">API Keys</h2>
          <p className="mb-4 text-sm text-gray-500">
            Manage your API keys for programmatic access.
          </p>
          <button className="rounded-lg bg-black px-4 py-2 text-sm text-white hover:bg-gray-800">
            Generate New Key
          </button>
        </section>

        {/* Billing */}
        <section className="rounded-lg border p-6">
          <h2 className="mb-4 text-xl font-semibold">Billing</h2>
          <div className="space-y-2 text-sm">
            <p>
              <span className="text-gray-500">Current Plan:</span> Free Trial
            </p>
            <p>
              <span className="text-gray-500">Credits Remaining:</span> --
            </p>
          </div>
          <button className="mt-4 rounded-lg border px-4 py-2 text-sm hover:bg-gray-50">
            Upgrade Plan
          </button>
        </section>

        {/* Webhooks */}
        <section className="rounded-lg border p-6">
          <h2 className="mb-4 text-xl font-semibold">Webhooks</h2>
          <p className="mb-4 text-sm text-gray-500">
            Configure webhooks to receive notifications when generations
            complete.
          </p>
          <button className="rounded-lg border px-4 py-2 text-sm hover:bg-gray-50">
            Add Webhook
          </button>
        </section>
      </div>
    </div>
  );
}
