"use client";

import { useState } from "react";
import { formatDistanceToNow } from "date-fns";
import { ptBR } from "date-fns/locale";
import { Bell, CheckCircle2, AlertTriangle } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { cn } from "@/lib/utils";

import {
  useUnreadCount,
  useNotifications,
  useMarkAsRead,
  useMarkAllAsRead,
  useClearAllNotifications,
} from "@/lib/hooks/use-notifications";
import type { NotificationItem } from "@/lib/api/notifications.api";

// ─── Notification icon by type ────────────────────────────────────────────────

function NotificationIcon({ type }: { type: NotificationItem["type"] }) {
  if (type === "ScanComplete") {
    return <CheckCircle2 className="h-4 w-4 shrink-0 text-emerald-500" />;
  }
  return <AlertTriangle className="h-4 w-4 shrink-0 text-amber-500" />;
}

// ─── Notification Bell ────────────────────────────────────────────────────────

export function NotificationBell() {
  const [open, setOpen] = useState(false);

  const { data: unreadCount = 0 } = useUnreadCount();
  const { data: notifications, isLoading } = useNotifications();

  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();
  const clearAll = useClearAllNotifications();

  function handleNotificationClick(notification: NotificationItem) {
    if (!notification.isRead) {
      markAsRead.mutate(notification.id);
    }
    setOpen(false);
  }

  function handleMarkAllAsRead() {
    markAllAsRead.mutate();
  }

  function handleClearAll() {
    clearAll.mutate();
  }

  const displayCount = unreadCount > 9 ? "9+" : unreadCount > 0 ? String(unreadCount) : null;

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          aria-label="Notificações"
          className="relative"
        >
          <Bell className="h-4 w-4" />
          {displayCount && (
            <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-0.5 text-[10px] font-bold leading-none text-white">
              {displayCount}
            </span>
          )}
        </Button>
      </PopoverTrigger>

      <PopoverContent
        align="end"
        className="w-80 p-0"
        sideOffset={8}
      >
        {/* Header */}
        <div className="flex flex-col border-b border-border px-4 py-3 gap-2">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold">Notificações</h3>
            <div className="flex gap-2">
              {unreadCount > 0 && (
                <Button
                  variant="ghost"
                  size="xs"
                  onClick={handleMarkAllAsRead}
                  disabled={markAllAsRead.isPending}
                  className="text-[10px] h-6 px-2"
                >
                  Marcar como lido
                </Button>
              )}
              {notifications && notifications.length > 0 && (
                <Button
                  variant="ghost"
                  size="xs"
                  onClick={handleClearAll}
                  disabled={clearAll.isPending}
                  className="text-[10px] h-6 px-2 text-destructive hover:text-destructive hover:bg-destructive/10"
                >
                  Limpar
                </Button>
              )}
            </div>
          </div>
        </div>

        {/* List */}
        <div className="max-h-96 overflow-y-auto">
          {isLoading ? (
            <div className="space-y-px p-2">
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="flex gap-3 rounded-md p-3">
                  <Skeleton className="h-4 w-4 shrink-0 rounded-full" />
                  <div className="flex-1 space-y-1.5">
                    <Skeleton className="h-3.5 w-3/4" />
                    <Skeleton className="h-3 w-full" />
                    <Skeleton className="h-3 w-1/2" />
                  </div>
                </div>
              ))}
            </div>
          ) : !notifications || notifications.length === 0 ? (
            <div className="py-10 text-center">
              <Bell className="mx-auto h-8 w-8 text-muted-foreground/40" />
              <p className="mt-2 text-sm text-muted-foreground">
                Nenhuma notificação
              </p>
            </div>
          ) : (
            <div className="space-y-px p-2">
              {notifications.map((notification) => (
                <button
                  key={notification.id}
                  type="button"
                  onClick={() => handleNotificationClick(notification)}
                  className={cn(
                    "flex w-full gap-3 rounded-md p-3 text-left transition-colors hover:bg-accent",
                    !notification.isRead && "bg-muted/50"
                  )}
                >
                  <NotificationIcon type={notification.type} />
                  <div className="flex-1 min-w-0">
                    <p className="truncate text-sm font-medium">
                      {notification.title}
                    </p>
                    <p className="line-clamp-2 text-xs text-muted-foreground">
                      {notification.body}
                    </p>
                    <p className="mt-1 text-xs text-muted-foreground/70">
                      {formatDistanceToNow(new Date(notification.createdAt), {
                        addSuffix: true,
                        locale: ptBR,
                      })}
                    </p>
                  </div>
                  {!notification.isRead && (
                    <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-blue-500" />
                  )}
                </button>
              ))}
            </div>
          )}
        </div>
      </PopoverContent>
    </Popover>
  );
}
