import Link from "next/link";

export default function GenerationsPage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Generations</h1>
        <Link
          href="/generate"
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
        >
          New Generation
        </Link>
      </div>

      <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
        <div className="border-b border-gray-200 dark:border-gray-800 p-4">
          <div className="flex gap-4">
            <input
              type="text"
              placeholder="Search generations..."
              className="flex-1 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
            />
            <select className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white">
              <option>All Status</option>
              <option>Completed</option>
              <option>Processing</option>
              <option>Failed</option>
            </select>
          </div>
        </div>

        <div className="p-8 text-center text-gray-500 dark:text-gray-400">
          No generations yet. Create your first audiobook to get started.
        </div>
      </div>
    </div>
  );
}
