"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Home,
  History,
  BarChart3,
  User,
  Users,
  ChevronLeft,
  ChevronRight,
  Radar,
  HelpCircle,
} from "lucide-react";
import { type LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { useAuth } from "@/stores/auth-store";
import { routes } from "@/lib/constants/routes";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { LogoMark } from "@/components/ui/logo-mark";

interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
}

const mainNavItems: NavItem[] = [
  { href: routes.scan, label: "Scanner", icon: Home },
  { href: routes.history, label: "Histórico", icon: History },
  { href: routes.monitor, label: "Monitor", icon: Radar },
  { href: routes.dashboardUser, label: "Dashboard", icon: BarChart3 },
  { href: routes.profile, label: "Perfil", icon: User },
];

const adminNavItems: NavItem[] = [
  { href: routes.dashboardAdmin, label: "Admin Dashboard", icon: BarChart3 },
  { href: routes.adminUsers, label: "Gerenciar Usuários", icon: Users },
];

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

export function Sidebar({ collapsed, onToggle }: SidebarProps) {
  const pathname = usePathname();
  const { isAdmin } = useAuth();

  return (
    <aside
      className={cn(
        "relative z-10 flex h-screen flex-col border-r border-border bg-sidebar overflow-hidden transition-[width] duration-300 ease-in-out",
        collapsed ? "w-16" : "w-64"
      )}
    >
      {/* Logo */}
      <div className="flex h-14 items-center gap-3 border-b border-border px-4">
        <Link href={routes.home} className="flex items-center gap-3 hover:opacity-80 transition-opacity">
          <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-accent-primary-subtle">
            <LogoMark size={16} />
          </div>
          {!collapsed && (
            <span className="whitespace-nowrap text-base font-semibold tracking-tight text-foreground">
              Heimdall
            </span>
          )}
        </Link>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 overflow-y-auto px-2 py-4">
        <NavSection items={mainNavItems} pathname={pathname} collapsed={collapsed} />

        {isAdmin && (
          <>
            {collapsed ? (
              <div className="mx-auto my-4 w-6 border-t border-border" />
            ) : (
              <Separator className="my-4" />
            )}
            {!collapsed && (
              <p className="mb-2 px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                Admin
              </p>
            )}
            <NavSection items={adminNavItems} pathname={pathname} collapsed={collapsed} />
          </>
        )}
      </nav>

      {/* Support link */}
      <div className="border-t border-border px-2 py-2">
        <NavSection
          items={[{ href: routes.support, label: "Suporte", icon: HelpCircle }]}
          pathname={pathname}
          collapsed={collapsed}
        />
      </div>

      {/* Collapse toggle */}
      <div className="border-t border-border p-2">
        <Button
          variant="ghost"
          size="icon"
          className="mx-auto w-fit"
          onClick={onToggle}
          aria-label={collapsed ? "Expandir sidebar" : "Recolher sidebar"}
        >
          <ChevronLeft
            className={cn(
              "h-4 w-4 transition-transform duration-200",
              collapsed && "rotate-180"
            )}
          />
        </Button>
      </div>
    </aside>
  );
}

function NavSection({
  items,
  pathname,
  collapsed,
}: {
  items: NavItem[];
  pathname: string;
  collapsed: boolean;
}) {
  return (
    <>
      {items.map((item) => {
        const isActive = pathname === item.href;
        const linkContent = (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              "relative flex h-9 items-center gap-3 rounded-lg px-3 text-sm font-medium transition-colors duration-150 hover:translate-x-1 transition-transform duration-150",
              collapsed ? "justify-center px-0 hover:translate-x-0" : "",
              isActive
                ? "bg-accent-primary-subtle font-semibold text-accent-primary"
                : "text-sidebar-foreground hover:bg-accent/50 hover:text-foreground"
            )}
          >
            {isActive && (
              <span className="absolute left-0 top-1/2 h-[60%] w-0.5 -translate-y-1/2 rounded-full bg-accent-primary transition-all duration-200" />
            )}
            <item.icon className={cn("h-4 w-4 shrink-0", isActive && "text-accent-primary")} />
            {!collapsed && <span className="whitespace-nowrap">{item.label}</span>}
          </Link>
        );

        if (collapsed) {
          return (
            <Tooltip key={item.href}>
              <TooltipTrigger asChild>{linkContent}</TooltipTrigger>
              <TooltipContent side="right">{item.label}</TooltipContent>
            </Tooltip>
          );
        }

        return linkContent;
      })}
    </>
  );
}
