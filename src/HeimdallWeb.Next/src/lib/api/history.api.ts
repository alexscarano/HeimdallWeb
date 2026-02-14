import { apiClient } from "./client";
import { endpoints } from "./endpoints";
import type {
  ScanHistoryDetail,
  FindingResponse,
  TechnologyResponse,
  IASummaryResponse,
  DeleteScanHistoryResponse,
} from "@/types/scan";

export async function getScanHistoryById(id: string): Promise<ScanHistoryDetail> {
  const response = await apiClient.get<ScanHistoryDetail>(endpoints.scanHistories.getById(id));
  return response.data;
}

export async function getScanFindings(id: string): Promise<FindingResponse[]> {
  const response = await apiClient.get<FindingResponse[]>(endpoints.scanHistories.findings(id));
  return response.data;
}

export async function getScanTechnologies(id: string): Promise<TechnologyResponse[]> {
  const response = await apiClient.get<TechnologyResponse[]>(endpoints.scanHistories.technologies(id));
  return response.data;
}

export async function getAISummary(id: string): Promise<IASummaryResponse | null> {
  const response = await apiClient.get<IASummaryResponse>(endpoints.scanHistories.aiSummary(id));
  return response.data;
}

export async function exportScanPdf(id: string): Promise<Blob> {
  const response = await apiClient.get(endpoints.scanHistories.export(id), {
    responseType: "blob",
  });
  return response.data as Blob;
}

export async function exportAllScansPdf(): Promise<Blob> {
  const response = await apiClient.get(endpoints.scanHistories.exportAll, {
    responseType: "blob",
  });
  return response.data as Blob;
}

export async function deleteScanHistory(id: string): Promise<DeleteScanHistoryResponse> {
  const response = await apiClient.delete<DeleteScanHistoryResponse>(endpoints.scanHistories.delete(id));
  return response.data;
}
