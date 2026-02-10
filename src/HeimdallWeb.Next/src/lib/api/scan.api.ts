import { apiClient } from "./client";
import { endpoints } from "./endpoints";
import type { ExecuteScanRequest, ExecuteScanResponse } from "@/types/scan";
import type { PaginatedResponse } from "@/types/api";
import type { ScanHistorySummary } from "@/types/scan";

export async function executeScan(data: ExecuteScanRequest): Promise<ExecuteScanResponse> {
  const response = await apiClient.post<ExecuteScanResponse>(endpoints.scans.execute, data);
  return response.data;
}

export async function listScans(params: {
  page?: number;
  pageSize?: number;
}): Promise<PaginatedResponse<ScanHistorySummary>> {
  const response = await apiClient.get<PaginatedResponse<ScanHistorySummary>>(endpoints.scans.list, {
    params,
  });
  return response.data;
}
