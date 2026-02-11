"use client";

import { useState, useEffect } from "react";
import { History, Trash2, SearchX } from "lucide-react";
import { ScanTable } from "@/components/history/scan-table";
import { ScanFilters } from "@/components/history/scan-filters";
import { useScanHistories, useDeleteScanHistory, useExportPdf } from "@/lib/hooks/use-history";
import { EmptyState } from "@/components/ui/empty-state";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

export default function HistoryPage() {
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("all");
  const [deleteId, setDeleteId] = useState<string | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => setSearch(searchInput), 300);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const { data, isLoading } = useScanHistories(page, 10, search, status);
  const deleteMutation = useDeleteScanHistory();
  const exportMutation = useExportPdf();

  const handleSearchChange = (value: string) => {
    setSearchInput(value);
    setPage(1);
  };

  const handleStatusChange = (value: string) => {
    setStatus(value);
    setPage(1);
  };

  const handleDelete = () => {
    if (deleteId) {
      deleteMutation.mutate(deleteId);
      setDeleteId(null);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent-primary-subtle">
          <History className="h-5 w-5 text-accent-primary" />
        </div>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Histórico de Scans</h1>
          <p className="text-sm text-muted-foreground">
            Visualize e gerencie seus scans anteriores
          </p>
        </div>
      </div>

      <ScanFilters
        search={searchInput}
        onSearchChange={handleSearchChange}
        status={status}
        onStatusChange={handleStatusChange}
      />

      {/* Empty State */}
      {!isLoading && data?.items.length === 0 && (
        <EmptyState
          icon={SearchX}
          title="Nenhum scan encontrado"
          description={
            search || status !== "all"
              ? "Tente ajustar os filtros de busca ou status para ver mais resultados."
              : "Você ainda não realizou nenhum scan. Vá para a página inicial para começar."
          }
          action={
            !search && status === "all"
              ? { label: "Realizar primeiro scan", href: "/" }
              : undefined
          }
        />
      )}

      {/* Table with data */}
      {(isLoading || (data && data.items.length > 0)) && (
        <ScanTable
          scans={data?.items ?? []}
          isLoading={isLoading}
          onDelete={setDeleteId}
          onExportPdf={(id) => exportMutation.mutate(id)}
        />
      )}

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-center gap-2">
          {Array.from({ length: data.totalPages }, (_, i) => i + 1).map((p) => (
            <Button
              key={p}
              variant={p === page ? "default" : "outline"}
              size="sm"
              onClick={() => setPage(p)}
              className={p === page ? "bg-accent-primary hover:bg-accent-primary/90" : ""}
            >
              {p}
            </Button>
          ))}
        </div>
      )}

      <Dialog open={deleteId !== null} onOpenChange={() => setDeleteId(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirmar exclusão</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja deletar este scan? Esta ação não pode ser desfeita.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteId(null)}>
              Cancelar
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={deleteMutation.isPending}
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Deletar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
