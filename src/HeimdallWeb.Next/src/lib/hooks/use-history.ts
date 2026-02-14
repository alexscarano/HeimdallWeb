import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@/stores/auth-store";
import { toast } from "sonner";
import { listScans } from "@/lib/api/scan.api";
import {
  getScanHistoryById,
  getScanFindings,
  getScanTechnologies,
  getAISummary,
  exportScanPdf,
  exportAllScansPdf,
  deleteScanHistory,
} from "@/lib/api/history.api";
import type { ScanHistorySummary, FindingResponse, TechnologyResponse, IASummaryResponse } from "@/types/scan";
import type { PaginatedResponse } from "@/types/api";

// Re-export for backward compatibility with consumers of this hook
export type { ScanHistorySummary as ScanHistory } from "@/types/scan";
export type { FindingResponse as Finding, TechnologyResponse as Technology, IASummaryResponse as AISummary } from "@/types/scan";
export type { PaginatedResponse } from "@/types/api";

export interface ScanHistoryDetail {
  historyId: string;
  target: string;
  createdDate: string;
  duration: string | null;
  hasCompleted: boolean;
  summary: string | null;
  rawJsonResult?: string | null;
}

export function useScanHistories(
  page: number = 1,
  pageSize: number = 10,
  search?: string,
  status?: string
) {
  const { user } = useAuth();

  return useQuery<PaginatedResponse<ScanHistorySummary>>({
    queryKey: ["scan-histories", user?.userId, page, pageSize, search, status],
    queryFn: () =>
      listScans({ page, pageSize, search, status }),
    enabled: !!user,
  });
}

export function useScanHistoryDetail(historyId: string) {
  return useQuery({
    queryKey: ["scan-history", historyId],
    queryFn: () => getScanHistoryById(historyId),
    enabled: !!historyId,
    retry: false,
  });
}

export function useScanFindings(historyId: string) {
  return useQuery<FindingResponse[]>({
    queryKey: ["scan-findings", historyId],
    queryFn: () => getScanFindings(historyId),
    enabled: !!historyId,
    retry: false,
  });
}

export function useScanTechnologies(historyId: string) {
  return useQuery<TechnologyResponse[]>({
    queryKey: ["scan-technologies", historyId],
    queryFn: () => getScanTechnologies(historyId),
    enabled: !!historyId,
    retry: false,
  });
}

export function useAISummary(historyId: string) {
  return useQuery<IASummaryResponse | null>({
    queryKey: ["ai-summary", historyId],
    queryFn: async () => {
      try {
        return await getAISummary(historyId);
      } catch (error: unknown) {
        // 404 is expected for scans without an AI summary - return null instead of throwing
        const axiosError = error as { response?: { status?: number } };
        if (axiosError?.response?.status === 404) {
          return null;
        }
        throw error;
      }
    },
    enabled: !!historyId,
    retry: false,
  });
}

export function useDeleteScanHistory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (historyId: string) => deleteScanHistory(historyId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["scan-histories"] });
      toast.success("Scan deletado com sucesso");
    },
    onError: () => {
      toast.error("Erro ao deletar scan");
    },
  });
}

export function useExportPdf() {
  return useMutation({
    mutationFn: async (historyId: string) => {
      const blob = await exportScanPdf(historyId);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `scan-${historyId}.pdf`;
      link.click();
      URL.revokeObjectURL(url);
    },
    onSuccess: () => {
      toast.success("PDF exportado com sucesso");
    },
    onError: () => {
      toast.error("Erro ao exportar PDF");
    },
  });
}

export function useExportAllPdf() {
  return useMutation({
    mutationFn: async () => {
      const blob = await exportAllScansPdf();
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `all-scans.pdf`;
      link.click();
      URL.revokeObjectURL(url);
    },
    onSuccess: () => {
      toast.success("PDFs exportados com sucesso");
    },
    onError: () => {
      toast.error("Erro ao exportar PDFs");
    },
  });
}
