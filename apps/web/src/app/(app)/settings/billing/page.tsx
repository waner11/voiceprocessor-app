"use client";

import Link from "next/link";

export default function BillingSettingsPage() {
  // Mock data - replace with actual API calls
  const usage = {
    charactersUsed: 45000,
    charactersLimit: 100000,
    generationsCount: 23,
    totalAudioMinutes: 156,
  };

  const currentPlan = {
    name: "Pro",
    price: "$29",
    period: "month",
    renewsAt: "2026-02-18",
  };

  const usagePercentage = (usage.charactersUsed / usage.charactersLimit) * 100;

  return (
    <div className="space-y-6">
      {/* Current Plan */}
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <div className="flex items-start justify-between mb-6">
          <div>
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Current Plan</h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              Manage your subscription and billing
            </p>
          </div>
          <span className="rounded-full bg-blue-100 dark:bg-blue-900 px-3 py-1 text-sm font-medium text-blue-700 dark:text-blue-300">
            {currentPlan.name}
          </span>
        </div>

        <div className="flex items-baseline gap-1 mb-4">
          <span className="text-4xl font-bold text-gray-900 dark:text-white">{currentPlan.price}</span>
          <span className="text-gray-500 dark:text-gray-400">/{currentPlan.period}</span>
        </div>

        <p className="text-sm text-gray-600 dark:text-gray-400 mb-6">
          Your plan renews on {new Date(currentPlan.renewsAt).toLocaleDateString()}
        </p>

        <div className="flex gap-3">
          <Link
            href="/#pricing"
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
          >
            Change Plan
          </Link>
          <button className="rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
            Cancel Subscription
          </button>
        </div>
      </section>

      {/* Usage */}
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-6">Usage This Month</h2>

        <div className="space-y-6">
          {/* Characters Usage */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Characters</span>
              <span className="text-sm text-gray-500 dark:text-gray-400">
                {usage.charactersUsed.toLocaleString()} / {usage.charactersLimit.toLocaleString()}
              </span>
            </div>
            <div className="h-3 rounded-full bg-gray-200 dark:bg-gray-700 overflow-hidden">
              <div
                className={`h-full rounded-full transition-all ${
                  usagePercentage > 90
                    ? "bg-red-500"
                    : usagePercentage > 75
                    ? "bg-yellow-500"
                    : "bg-blue-500"
                }`}
                style={{ width: `${Math.min(usagePercentage, 100)}%` }}
              />
            </div>
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              {(usage.charactersLimit - usage.charactersUsed).toLocaleString()} characters remaining
            </p>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-2 gap-4">
            <div className="rounded-lg bg-gray-50 dark:bg-gray-800 p-4">
              <p className="text-2xl font-bold text-gray-900 dark:text-white">{usage.generationsCount}</p>
              <p className="text-sm text-gray-500 dark:text-gray-400">Generations</p>
            </div>
            <div className="rounded-lg bg-gray-50 dark:bg-gray-800 p-4">
              <p className="text-2xl font-bold text-gray-900 dark:text-white">{usage.totalAudioMinutes}</p>
              <p className="text-sm text-gray-500 dark:text-gray-400">Audio Minutes</p>
            </div>
          </div>
        </div>
      </section>

      {/* Payment Method */}
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-6">Payment Method</h2>

        <div className="flex items-center justify-between p-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800">
          <div className="flex items-center gap-4">
            <div className="h-10 w-16 rounded bg-gradient-to-r from-blue-600 to-blue-800 flex items-center justify-center text-white text-xs font-bold">
              VISA
            </div>
            <div>
              <p className="font-medium text-gray-900 dark:text-white">•••• •••• •••• 4242</p>
              <p className="text-sm text-gray-500 dark:text-gray-400">Expires 12/2027</p>
            </div>
          </div>
          <button className="text-sm text-blue-600 dark:text-blue-400 hover:underline">
            Update
          </button>
        </div>
      </section>

      {/* Billing History */}
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-6">Billing History</h2>

        <div className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 dark:bg-gray-800">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Description
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Amount
                </th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Invoice
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              <tr>
                <td className="px-4 py-3 text-sm text-gray-900 dark:text-white">Jan 18, 2026</td>
                <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">Pro Plan - Monthly</td>
                <td className="px-4 py-3 text-sm text-gray-900 dark:text-white">$29.00</td>
                <td className="px-4 py-3 text-right">
                  <button className="text-sm text-blue-600 dark:text-blue-400 hover:underline">
                    Download
                  </button>
                </td>
              </tr>
              <tr>
                <td className="px-4 py-3 text-sm text-gray-900 dark:text-white">Dec 18, 2025</td>
                <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">Pro Plan - Monthly</td>
                <td className="px-4 py-3 text-sm text-gray-900 dark:text-white">$29.00</td>
                <td className="px-4 py-3 text-right">
                  <button className="text-sm text-blue-600 dark:text-blue-400 hover:underline">
                    Download
                  </button>
                </td>
              </tr>
              <tr>
                <td className="px-4 py-3 text-sm text-gray-900 dark:text-white">Nov 18, 2025</td>
                <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">Pro Plan - Monthly</td>
                <td className="px-4 py-3 text-sm text-gray-900 dark:text-white">$29.00</td>
                <td className="px-4 py-3 text-right">
                  <button className="text-sm text-blue-600 dark:text-blue-400 hover:underline">
                    Download
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
