export default function SettingsPage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold text-gray-900 dark:text-white">Settings</h1>

      <div className="max-w-2xl space-y-8">
        {/* Account */}
        <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-4 text-xl font-semibold text-gray-900 dark:text-white">Account</h2>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Email</label>
              <input
                type="email"
                className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white"
                placeholder="your@email.com"
                disabled
              />
            </div>
            <button className="rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
              Change Password
            </button>
          </div>
        </section>

        {/* API Keys */}
        <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-4 text-xl font-semibold text-gray-900 dark:text-white">API Keys</h2>
          <p className="mb-4 text-sm text-gray-500 dark:text-gray-400">
            Manage your API keys for programmatic access.
          </p>
          <button className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
            Generate New Key
          </button>
        </section>

        {/* Billing */}
        <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-4 text-xl font-semibold text-gray-900 dark:text-white">Billing</h2>
          <div className="space-y-2 text-sm">
            <p>
              <span className="text-gray-500 dark:text-gray-400">Current Plan:</span>{" "}
              <span className="text-gray-900 dark:text-white">Free Trial</span>
            </p>
            <p>
              <span className="text-gray-500 dark:text-gray-400">Credits Remaining:</span>{" "}
              <span className="text-gray-900 dark:text-white">--</span>
            </p>
          </div>
          <button className="mt-4 rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
            Upgrade Plan
          </button>
        </section>

        {/* Webhooks */}
        <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-4 text-xl font-semibold text-gray-900 dark:text-white">Webhooks</h2>
          <p className="mb-4 text-sm text-gray-500 dark:text-gray-400">
            Configure webhooks to receive notifications when generations
            complete.
          </p>
          <button className="rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
            Add Webhook
          </button>
        </section>
      </div>
    </div>
  );
}
