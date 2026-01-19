import Link from "next/link";

export default function DashboardPage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Credits</h2>
          <p className="text-3xl font-bold text-gray-900 dark:text-white">--</p>
          <p className="text-sm text-gray-500 dark:text-gray-400">characters remaining</p>
        </div>

        <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Generations</h2>
          <p className="text-3xl font-bold text-gray-900 dark:text-white">--</p>
          <p className="text-sm text-gray-500 dark:text-gray-400">total generations</p>
        </div>

        <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Audio Duration</h2>
          <p className="text-3xl font-bold text-gray-900 dark:text-white">--</p>
          <p className="text-sm text-gray-500 dark:text-gray-400">total hours</p>
        </div>
      </div>

      <div className="mt-8">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Recent Generations</h2>
          <Link href="/generations" className="text-sm text-blue-600 dark:text-blue-400 hover:underline">
            View all
          </Link>
        </div>
        <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-8 text-center text-gray-500 dark:text-gray-400">
          No generations yet. Start by creating your first audiobook.
        </div>
      </div>

      <div className="mt-8 flex justify-center">
        <Link
          href="/generate"
          className="rounded-lg bg-blue-600 px-6 py-3 text-white hover:bg-blue-700"
        >
          Create New Generation
        </Link>
      </div>
    </div>
  );
}
