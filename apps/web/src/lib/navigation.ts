import { LayoutDashboard, Mic, Library, AudioLines } from "lucide-react";

export const navItems = [
  { href: "/dashboard", label: "Dashboard", mobileLabel: "Home", icon: LayoutDashboard },
  { href: "/generate", label: "Generate", mobileLabel: "Generate", icon: Mic },
  { href: "/generations", label: "Generations", mobileLabel: "Library", icon: Library },
  { href: "/voices", label: "Voices", mobileLabel: "Voices", icon: AudioLines },
];
