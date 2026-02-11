import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@/stores/auth-store";
import { toast } from "sonner";

export interface ScanHistory {
  historyId: string;
  target: string;
  createdDate: string;
  duration: string;
  hasCompleted: boolean;
  summary?: string;
}

export interface ScanHistoryDetail {
  historyId: string;
  target: string;
  createdDate: string;
  duration: string;
  hasCompleted: boolean;
  summary?: string;
  rawJsonResult?: string;
}

export interface Finding {
  findingId: string;
  type: string;
  description: string;
  severity: "Critical" | "High" | "Medium" | "Low" | "Informational";
  evidence?: string;
  recommendation?: string;
}

export interface Technology {
  technologyId: string;
  name: string;
  version?: string;
  category: string;
  description?: string;
}

export interface AISummary {
  iaSummaryId: string;
  mainCategory: string;
  overallRisk: string;
  summaryText: string;
  findingsCritical: number;
  findingsHigh: number;
  findingsMedium: number;
  findingsLow: number;
  findingsInformational: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export function useScanHistories(
  page: number = 1,
  pageSize: number = 10,
  search?: string,
  status?: string
) {
  const { user } = useAuth();

  return useQuery<PaginatedResponse<ScanHistory>>({
    queryKey: ["scan-histories", user?.userId, page, pageSize, search, status],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      if (search?.trim()) params.set("search", search.trim());
      if (status && status !== "all") params.set("status", status);

      const response = await fetch(`/api/v1/scans?${params.toString()}`);
      if (!response.ok) throw new Error("Erro ao carregar histórico");
      return response.json();
    },
    enabled: !!user, // Wait for user to be loaded before fetching
  });
}

export function useScanHistoryDetail(historyId: string) {
  return useQuery<ScanHistoryDetail>({
    queryKey: ["scan-history", historyId],
    queryFn: async () => {
      const response = await fetch(`/api/v1/scan-histories/${historyId}`);
      
      if (response.status === 404) {
        throw new Error("Scan não encontrado ou você não tem permissão para acessá-lo");
      }
      
      if (response.status === 403) {
        throw new Error("Você não tem permissão para acessar este scan");
      }
      
      if (!response.ok) {
        throw new Error("Erro ao carregar detalhes do scan");
      }
      
      return response.json();
    },
    enabled: !!historyId,
    retry: false, // Não tentar novamente em erros de autorização
  });
}

export function useScanFindings(historyId: string) {
  return useQuery<Finding[]>({
    queryKey: ["scan-findings", historyId],
    queryFn: async () => {
      const response = await fetch(`/api/v1/scan-histories/${historyId}/findings`);
      
      if (response.status === 404) {
        throw new Error("Scan não encontrado ou sem permissão");
      }
      
      if (response.status === 403) {
        throw new Error("Sem permissão para acessar vulnerabilidades");
      }
      
      if (!response.ok) {
        throw new Error("Erro ao carregar vulnerabilidades");
      }
      
      return response.json();
    },
    enabled: !!historyId,
    retry: false,
  });
}

export function useScanTechnologies(historyId: string) {
  return useQuery<Technology[]>({
    queryKey: ["scan-technologies", historyId],
    queryFn: async () => {
      const response = await fetch(`/api/v1/scan-histories/${historyId}/technologies`);
      
      if (response.status === 404) {
        throw new Error("Scan não encontrado ou sem permissão");
      }
      
      if (response.status === 403) {
        throw new Error("Sem permissão para acessar tecnologias");
      }
      
      if (!response.ok) {
        throw new Error("Erro ao carregar tecnologias");
      }
      
      return response.json();
    },
    enabled: !!historyId,
    retry: false,
  });
}

export function useAISummary(historyId: string) {
  return useQuery<AISummary | null>({
    queryKey: ["ai-summary", historyId],
    queryFn: async () => {
      const response = await fetch(`/api/v1/scan-histories/${historyId}/ai-summary`);
      
      // 404 é esperado para scans sem AI summary - retorna null
      if (response.status === 404) {
        return null;
      }
      
      if (!response.ok) {
        throw new Error("Erro ao carregar análise de IA");
      }
      
      return response.json();
    },
    enabled: !!historyId,
    retry: false, // Não tentar novamente em caso de 404
  });
}

export function useDeleteScanHistory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (historyId: string) => {
      const response = await fetch(`/api/v1/scan-histories/${historyId}`, {
        method: "DELETE",
      });
      if (!response.ok) throw new Error("Erro ao deletar scan");
    },
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
      const response = await fetch(`/api/v1/scan-histories/${historyId}/export`);
      if (!response.ok) throw new Error("Erro ao exportar PDF");
      const blob = await response.blob();
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
      const response = await fetch(`/api/v1/scan-histories/export`);
      if (!response.ok) throw new Error("Erro ao exportar todos os PDFs");
      const blob = await response.blob();
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
