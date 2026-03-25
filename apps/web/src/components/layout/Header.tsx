"use client";

import { useState, useRef, useEffect } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import { useUsage } from "@/hooks";
import { useAuthStore } from "@/stores";
import { formatNumber } from "@/utils/formatNumber";
import { navItems } from "@/lib/navigation";

export function Header() {
  const pathname = usePathname();
  const router = useRouter();
  const { data: usage } = useUsage();
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isMounted, setIsMounted] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

   // Handle hydration
   useEffect(() => {
     // Intentional: setIsMounted(true) on mount is the standard hydration pattern
     // eslint-disable-next-line react-hooks/set-state-in-effect
     setIsMounted(true);
   }, []);

  // Close menu when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsMenuOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleSignOut = () => {
    logout();
    setIsMenuOpen(false);
    router.push("/login");
  };

  // Get user initials for avatar
  const getInitials = (name?: string) => {
    if (!name) return "U";
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <header className="sticky top-0 z-50 border-b border-border-subtle bg-bg-base">
      <div className="container mx-auto flex h-14 items-center justify-between px-4">
        <div className="flex items-center gap-8">
          <Link href="/dashboard" className="text-xl font-bold text-indigo font-heading">
            VoiceProcessor
          </Link>

          <nav className="hidden md:flex items-center gap-1">
            {navItems.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  "rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  pathname === item.href
                    ? "bg-indigo-subtle text-indigo"
                    : "text-text-secondary hover:bg-bg-sunken hover:text-text-primary"
                )}
              >
                {item.label}
              </Link>
            ))}
          </nav>
        </div>

        <div className="flex items-center gap-4">
           {usage && (
             <div className="hidden sm:flex items-center gap-2 text-sm">
                <span className="text-text-secondary">Credits:</span>
                <span className="font-semibold tabular-nums text-indigo">
                 {formatNumber(usage.creditsRemaining || 0)}
               </span>
             </div>
           )}

          {/* Profile Dropdown */}
          <div className="relative" ref={menuRef}>
            <button
              onClick={() => setIsMenuOpen(!isMenuOpen)}
               className="flex items-center justify-center h-9 w-9 rounded-full bg-indigo text-text-inverse text-sm font-medium hover:bg-indigo-dark transition-colors focus:outline-none focus:shadow-[var(--shadow-focus)]"
              title={isMounted ? (user?.name || "Profile") : "Profile"}
              aria-label={isMounted ? (user?.name || "Open profile menu") : "Open profile menu"}
            >
              {isMounted ? getInitials(user?.name) : "U"}
            </button>

            {isMenuOpen && (
              <div className="absolute right-0 mt-2 w-56 rounded-lg border border-border-subtle bg-bg-base shadow-elevated py-1 z-50">
                {/* User Info */}
                <div className="px-4 py-3 border-b border-border-subtle">
                  <p className="text-sm font-medium text-text-primary truncate">
                    {isMounted ? (user?.name || "User") : "User"}
                  </p>
                  <p className="text-xs text-text-muted truncate">
                    {isMounted ? (user?.email || "user@example.com") : "user@example.com"}
                  </p>
                </div>

                {/* Menu Items */}
                <div className="py-1">
                  <Link
                    href="/settings"
                    onClick={() => setIsMenuOpen(false)}
                    className="flex items-center gap-3 px-4 py-2 text-sm text-text-secondary hover:bg-bg-sunken hover:text-text-primary"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    >
                      <path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                    Settings
                  </Link>
                </div>

                {/* Sign Out */}
                <div className="border-t border-border-subtle py-1">
                  <button
                    onClick={handleSignOut}
                    className="flex w-full items-center gap-3 px-4 py-2 text-sm text-error hover:bg-bg-sunken"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    >
                      <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                      <polyline points="16 17 21 12 16 7" />
                      <line x1="21" y1="12" x2="9" y2="12" />
                    </svg>
                    Sign out
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
