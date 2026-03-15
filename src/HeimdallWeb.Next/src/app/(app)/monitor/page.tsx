"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod/v4";
import { toast } from "sonner";
import { formatDistanceToNow } from "date-fns";
import { ptBR } from "date-fns/locale";
import { Radar, MoreHorizontal, Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogCancel,
  AlertDialogAction,
} from "@/components/ui/alert-dialog";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Popover,
  PopoverAnchor,
  PopoverContent,
} from "@/components/ui/popover";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { GradeBadge } from "@/components/scan/score-gauge";

import {
  useMonitoredTargets,
  useCreateMonitor,
  useDeleteMonitor,
  useMonitorHistory,
  useDistinctTargets,
} from "@/lib/hooks/use-monitor";
import type { MonitoredTarget } from "@/lib/api/monitor.api";

// ─── Zod schema ─────────────────────────────────────────────────────────────

const addTargetSchema = z.object({
  url: z.url("Insira uma URL válida com https://"),
  frequency: z.enum(["Daily", "Weekly", "Monthly"]),
});

type AddTargetForm = z.infer<typeof addTargetSchema>;

// ─── Frequency badge ─────────────────────────────────────────────────────────

function FrequencyBadge({ frequency }: { frequency: MonitoredTarget["frequency"] }) {
  const map: Record<MonitoredTarget["frequency"], { label: string; className: string }> = {
    Daily: { label: "Diário", className: "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20" },
    Weekly: { label: "Semanal", className: "bg-violet-500/10 text-violet-600 dark:text-violet-400 border-violet-500/20" },
    Monthly: { label: "Mensal", className: "bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-500/20" },
  };
  const { label, className } = map[frequency];
  return (
    <Badge variant="outline" className={className}>
      {label}
    </Badge>
  );
}

// ─── Status badge ─────────────────────────────────────────────────────────────

function StatusBadge({ isActive }: { isActive: boolean }) {
  return isActive ? (
    <Badge variant="outline" className="bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20">
      Ativo
    </Badge>
  ) : (
    <Badge variant="outline" className="bg-zinc-500/10 text-zinc-500 border-zinc-500/20">
      Inativo
    </Badge>
  );
}

// ─── Date helper ─────────────────────────────────────────────────────────────

function formatRelative(date: string | null): string {
  if (!date) return "—";
  return formatDistanceToNow(new Date(date), { addSuffix: true, locale: ptBR });
}

// ─── History Sheet ────────────────────────────────────────────────────────────

function HistorySheet({
  target,
  open,
  onOpenChange,
}: {
  target: MonitoredTarget | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const { data: history, isLoading } = useMonitorHistory(target?.id ?? 0);

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="w-full sm:max-w-lg overflow-y-auto">
        <SheetHeader className="mb-4">
          <SheetTitle>Scans de {target?.url}</SheetTitle>
        </SheetHeader>

        {isLoading ? (
          <div className="space-y-2 p-4">
            {Array.from({ length: 5 }).map((_, i) => (
              <Skeleton key={i} className="h-10 w-full" />
            ))}
          </div>
        ) : !history || history.length === 0 ? (
          <div className="flex flex-col items-center justify-center gap-3 py-16 text-center">
            <Radar className="h-10 w-10 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">Nenhum histórico disponível.</p>
          </div>
        ) : (
          <div className="p-4">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Data</TableHead>
                  <TableHead>Score</TableHead>
                  <TableHead>Grade</TableHead>
                  <TableHead>Vulnerabilidades</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {history.map((entry) => (
                  <TableRow key={entry.id}>
                    <TableCell className="text-sm text-muted-foreground whitespace-nowrap">
                      {formatRelative(entry.createdAt)}
                    </TableCell>
                    <TableCell>{entry.score ?? "—"}</TableCell>
                    <TableCell>
                      <GradeBadge grade={entry.grade} score={entry.score ?? undefined} />
                    </TableCell>
                    <TableCell className="text-sm whitespace-nowrap">
                      {entry.findingsCount === 0 ? (
                        <span className="text-emerald-500">Sem riscos</span>
                      ) : (
                        <span>
                          <span className="text-red-500 font-medium">{entry.criticalCount} Críticos</span>
                          {" / "}
                          <span className="text-orange-500 font-medium">{entry.highCount} Altos</span>
                        </span>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </SheetContent>
    </Sheet>
  );
}

// ─── Add Target Dialog ────────────────────────────────────────────────────────

function AddTargetDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const createMonitor = useCreateMonitor();
  const { data: suggestions = [] } = useDistinctTargets();
  const [dropdownOpen, setDropdownOpen] = useState(false);

  const form = useForm<AddTargetForm>({
    resolver: zodResolver(addTargetSchema),
    defaultValues: { url: "", frequency: "Weekly" },
  });

  const urlValue = form.watch("url");

  const filtered = suggestions.filter((s) =>
    urlValue === "" || s.toLowerCase().includes(urlValue.toLowerCase())
  );

  const onSubmit = (values: AddTargetForm) => {
    createMonitor.mutate(
      { url: values.url, frequency: values.frequency },
      {
        onSuccess: () => {
          toast.success("Alvo adicionado com sucesso!");
          form.reset();
          onOpenChange(false);
        },
        onError: () => {
          toast.error("Erro ao adicionar alvo. Tente novamente.");
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={(val) => {
      if (!val) form.reset();
      onOpenChange(val);
    }}>
      <DialogContent className="overflow-visible">
        <DialogHeader>
          <DialogTitle>Adicionar alvo</DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="url"
              render={({ field }) => (
                <FormItem className="relative">
                  <FormLabel>URL</FormLabel>
                  <FormControl>
                    <Input
                      type="url"
                      placeholder="https://exemplo.com"
                      autoComplete="off"
                      {...field}
                      onFocus={() => setDropdownOpen(true)}
                      onBlur={(e) => {
                        field.onBlur();
                        setTimeout(() => setDropdownOpen(false), 200);
                      }}
                    />
                  </FormControl>
                  {dropdownOpen && filtered.length > 0 && (
                    <div className="absolute top-[calc(100%+0.25rem)] left-0 w-full z-[100] rounded-md border bg-popover text-popover-foreground shadow-md outline-none animate-in fade-in-0 zoom-in-95">
                      <ul className="max-h-48 overflow-y-auto py-1">
                        {filtered.map((suggestion) => (
                          <li key={suggestion}>
                            <button
                              type="button"
                              className="w-full truncate px-3 py-2 text-left text-sm hover:bg-accent hover:text-accent-foreground"
                              onMouseDown={(e) => {
                                e.preventDefault(); // Prevent input blur
                                const targetUrl = suggestion.startsWith("http") ? suggestion : `https://${suggestion}`;
                                form.setValue("url", targetUrl, { shouldValidate: true });
                                setDropdownOpen(false);
                              }}
                            >
                              {suggestion}
                            </button>
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="frequency"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Frequência</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Selecione a frequência" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="Daily">Diário</SelectItem>
                      <SelectItem value="Weekly">Semanal</SelectItem>
                      <SelectItem value="Monthly">Mensal</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button type="submit" disabled={createMonitor.isPending}>
                {createMonitor.isPending ? "Adicionando..." : "Adicionar"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

// ─── Monitor Page ─────────────────────────────────────────────────────────────

export default function MonitorPage() {
  const { data: targets, isLoading } = useMonitoredTargets();
  const deleteMonitor = useDeleteMonitor();

  const [addOpen, setAddOpen] = useState(false);
  const [historyTarget, setHistoryTarget] = useState<MonitoredTarget | null>(null);
  const [historyOpen, setHistoryOpen] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<MonitoredTarget | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);

  function handleOpenHistory(target: MonitoredTarget) {
    setHistoryTarget(target);
    setHistoryOpen(true);
  }

  function handleOpenDelete(target: MonitoredTarget) {
    setDeleteTarget(target);
    setDeleteOpen(true);
  }

  function handleConfirmDelete() {
    if (!deleteTarget) return;
    deleteMonitor.mutate(deleteTarget.id, {
      onSuccess: () => {
        toast.success("Alvo removido.");
        setDeleteOpen(false);
        setDeleteTarget(null);
      },
      onError: () => {
        toast.error("Erro ao remover alvo.");
      },
    });
  }

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col items-start gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Alvos Monitorados</h1>
          <p className="text-sm text-muted-foreground">
            Gerencie os sites que serão escaneados automaticamente.
          </p>
        </div>
        <Button onClick={() => setAddOpen(true)} className="w-full sm:w-auto">
          <Plus className="mr-2 h-4 w-4" />
          Adicionar alvo
        </Button>
      </div>

      {/* Table & Cards Skeleton */}
      {isLoading ? (
        <>
          {/* Desktop Skeleton */}
          <div className="hidden md:block rounded-xl border border-border bg-card overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>URL</TableHead>
                  <TableHead>Frequência</TableHead>
                  <TableHead>Último Check</TableHead>
                  <TableHead>Próximo Check</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead />
                </TableRow>
              </TableHeader>
              <TableBody>
                {Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    {Array.from({ length: 6 }).map((__, j) => (
                      <TableCell key={j}>
                        <Skeleton className="h-5 w-full" />
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          {/* Mobile Skeleton */}
          <div className="grid grid-cols-1 gap-4 md:hidden">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex flex-col gap-3 rounded-xl border bg-card p-4 shadow-sm">
                <div className="flex items-start justify-between gap-2">
                  <Skeleton className="h-5 w-3/4" />
                  <Skeleton className="h-8 w-8 rounded-md shrink-0" />
                </div>
                <div className="grid grid-cols-2 gap-y-3 gap-x-4 mt-1">
                  <Skeleton className="h-10 w-full" />
                  <Skeleton className="h-10 w-full" />
                  <Skeleton className="h-10 w-full" />
                  <Skeleton className="h-10 w-full" />
                </div>
              </div>
            ))}
          </div>
        </>
      ) : !targets || targets.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-4 rounded-lg border border-border bg-card py-24 text-center">
          <Radar className="h-12 w-12 text-muted-foreground" />
          <div>
            <p className="font-medium">Nenhum alvo monitorado ainda</p>
            <p className="text-sm text-muted-foreground">
              Adicione um site para receber scans automáticos periódicos.
            </p>
          </div>
          <Button onClick={() => setAddOpen(true)}>
            <Plus className="h-4 w-4" />
            Adicionar primeiro alvo
          </Button>
        </div>
      ) : (
        <>
          {/* Desktop Table View */}
          <div className="hidden md:block rounded-xl border border-border bg-card overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>URL</TableHead>
                  <TableHead>Frequência</TableHead>
                  <TableHead>Último Check</TableHead>
                  <TableHead>Próximo Check</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="w-10" />
                </TableRow>
              </TableHeader>
              <TableBody>
                {targets.map((target) => (
                  <TableRow key={target.id}>
                    <TableCell className="font-medium">{target.url}</TableCell>
                    <TableCell>
                      <FrequencyBadge frequency={target.frequency} />
                    </TableCell>
                    <TableCell className="text-sm text-muted-foreground">
                      {target.lastCheck ? formatRelative(target.lastCheck) : "Nunca"}
                    </TableCell>
                    <TableCell className="text-sm text-muted-foreground">
                      {formatRelative(target.nextCheck)}
                    </TableCell>
                    <TableCell>
                      <StatusBadge isActive={target.isActive} />
                    </TableCell>
                    <TableCell>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon" aria-label="Ações">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem onClick={() => handleOpenHistory(target)}>
                            Ver histórico
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            className="text-destructive focus:text-destructive"
                            onClick={() => handleOpenDelete(target)}
                          >
                            Excluir
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          {/* Mobile Card View */}
          <div className="grid grid-cols-1 gap-4 md:hidden">
            {targets.map((target) => (
              <div key={target.id} className="flex flex-col gap-3 rounded-xl border bg-card p-4 shadow-sm">
                <div className="flex items-start justify-between gap-2">
                  <div className="font-medium break-all">{target.url}</div>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="icon" className="h-8 w-8 shrink-0">
                        <MoreHorizontal className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem onClick={() => handleOpenHistory(target)}>
                        Ver histórico
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        className="text-destructive focus:text-destructive"
                        onClick={() => handleOpenDelete(target)}
                      >
                        Excluir
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
                <div className="grid grid-cols-2 gap-y-3 gap-x-4 text-sm mt-1">
                  <div>
                    <p className="text-xs text-muted-foreground mb-1">Status</p>
                    <StatusBadge isActive={target.isActive} />
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground mb-1">Frequência</p>
                    <FrequencyBadge frequency={target.frequency} />
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground mb-1">Último Check</p>
                    <span className="text-muted-foreground">{target.lastCheck ? formatRelative(target.lastCheck) : "Nunca"}</span>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground mb-1">Próximo</p>
                    <span className="text-muted-foreground">{formatRelative(target.nextCheck)}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {/* Add Dialog */}
      <AddTargetDialog open={addOpen} onOpenChange={setAddOpen} />

      {/* History Sheet */}
      <HistorySheet
        target={historyTarget}
        open={historyOpen}
        onOpenChange={setHistoryOpen}
      />

      {/* Delete Confirm */}
      <AlertDialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remover alvo</AlertDialogTitle>
            <AlertDialogDescription>
              Tem certeza que deseja remover <strong>{deleteTarget?.url}</strong>?
              Esta ação não pode ser desfeita.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              className="bg-destructive text-white hover:bg-destructive/90"
              onClick={handleConfirmDelete}
            >
              {deleteMonitor.isPending ? "Removendo..." : "Remover"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
