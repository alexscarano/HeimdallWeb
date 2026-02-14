import { apiClient } from "./client";
import { endpoints } from "./endpoints";
import type { ExecuteScanRequest, ExecuteScanResponse } from "@/types/scan";
import type { PaginatedResponse } from "@/types/api";
import type { ScanHistorySummary } from "@/types/scan";

export async function executeScan(data: ExecuteScanRequest): Promise<ExecuteScanResponse> {
  const response = await apiClient.post<ExecuteScanResponse>(endpoints.scans.execute, data);
  return response.data;
}

export interface ListScansParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
}

export async function listScans(params: ListScansParams): Promise<PaginatedResponse<ScanHistorySummary>> {
  const query: Record<string, string | number> = {};

  if (params.page !== undefined) query.page = params.page;
  if (params.pageSize !== undefined) query.pageSize = params.pageSize;
  if (params.search?.trim()) query.search = params.search.trim();
  if (params.status && params.status !== "all") query.status = params.status;

  const response = await apiClient.get<PaginatedResponse<ScanHistorySummary>>(endpoints.scans.list, {
    params: query,
  });
  return response.data;
}
