"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { User, KeyRound, Link2, CreditCard } from "lucide-react";
import { cn } from "@/lib/utils";
import { useApiAccess } from "@/lib/posthog/use-api-access";

const settingsNav = [
  { href: "/settings/profile", label: "Profile", icon: User },
  { href: "/settings/api-keys", label: "API Keys", icon: KeyRound },
  { href: "/settings/connections", label: "Connections", icon: Link2 },
  { href: "/settings/billing", label: "Billing", icon: CreditCard },
];

export default function SettingsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const hasApiAccess = useApiAccess();

  const filteredNav = settingsNav.filter(
    (item) => item.href !== "/settings/api-keys" || hasApiAccess
  );

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold text-text-primary font-heading">Settings</h1>

      <div className="flex flex-col md:flex-row gap-8">
        {/* Sidebar Navigation */}
        <nav className="md:w-64 flex-shrink-0">
          <ul className="space-y-1">
            {filteredNav.map((item) => {
              const Icon = item.icon;
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    className={cn(
                      "flex items-center gap-3 rounded-lg px-4 py-3 text-sm font-medium transition-colors",
                      pathname === item.href
                        ? "bg-indigo-subtle text-indigo"
                        : "text-text-secondary hover:bg-bg-sunken hover:text-text-primary"
                    )}
                  >
                    <Icon className="w-5 h-5" />
                    {item.label}
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>

        {/* Content */}
        <div className="flex-1 min-w-0">
          {children}
        </div>
      </div>
    </div>
  );
}
