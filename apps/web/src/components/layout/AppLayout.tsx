"use client";

import { type ReactNode } from "react";
import { Header } from "./Header";
import { MobileNav } from "./MobileNav";

interface AppLayoutProps {
  children: ReactNode;
}

export function AppLayout({ children }: AppLayoutProps) {
  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <main className="pb-20 md:pb-0">{children}</main>
      <MobileNav />
    </div>
  );
}
