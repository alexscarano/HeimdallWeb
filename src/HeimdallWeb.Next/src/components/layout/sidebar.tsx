"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Shield,
  Home,
  History,
  BarChart3,
  User,
  Users,
  ChevronLeft,
  ChevronRight,
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

interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
}

const mainNavItems: NavItem[] = [
  { href: routes.home, label: "Scanner", icon: Home },
  { href: routes.history, label: "Histórico", icon: History },
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
        "flex h-screen flex-col border-r border-sidebar-border bg-sidebar transition-all duration-200",
        collapsed ? "w-16" : "w-64"
      )}
    >
      {/* Logo */}
      <div className="flex h-14 items-center gap-3 border-b border-sidebar-border px-4">
        <Shield className="h-6 w-6 shrink-0 text-foreground" />
        {!collapsed && (
          <span className="text-base font-semibold tracking-tight text-foreground">
            Heimdall
          </span>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 overflow-y-auto px-2 py-4">
        <NavSection items={mainNavItems} pathname={pathname} collapsed={collapsed} />

        {isAdmin && (
          <>
            <Separator className="my-4" />
            {!collapsed && (
              <p className="mb-2 px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                Admin
              </p>
            )}
            <NavSection items={adminNavItems} pathname={pathname} collapsed={collapsed} />
          </>
        )}
      </nav>

      {/* Collapse toggle */}
      <div className="border-t border-sidebar-border p-2">
        <Button
          variant="ghost"
          size="icon"
          className="w-full"
          onClick={onToggle}
          aria-label={collapsed ? "Expandir sidebar" : "Recolher sidebar"}
        >
          {collapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
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
              "flex h-9 items-center gap-3 rounded-lg px-3 text-sm font-medium transition-colors",
              isActive
                ? "bg-sidebar-accent text-sidebar-accent-foreground"
                : "text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
            )}
          >
            <item.icon className="h-4 w-4 shrink-0" />
            {!collapsed && <span>{item.label}</span>}
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
