"use client";

import { useLayoutEffect } from "react";
import { useUIStore } from "@/stores/uiStore";

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const theme = useUIStore((state) => state.theme);

  // Apply theme whenever it changes
  useLayoutEffect(() => {
    const root = document.documentElement;

    const applyTheme = (resolvedTheme: "light" | "dark") => {
      root.classList.remove("light", "dark");
      root.classList.add(resolvedTheme);
      // Also update color-scheme for native elements
      root.style.colorScheme = resolvedTheme;
    };

    if (theme === "system") {
      const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
      applyTheme(mediaQuery.matches ? "dark" : "light");

      const handleChange = (e: MediaQueryListEvent) => {
        applyTheme(e.matches ? "dark" : "light");
      };

      mediaQuery.addEventListener("change", handleChange);
      return () => mediaQuery.removeEventListener("change", handleChange);
    } else {
      applyTheme(theme);
    }
  }, [theme]);

  return <>{children}</>;
}
