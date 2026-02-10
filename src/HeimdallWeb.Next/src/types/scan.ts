import { SeverityLevel } from "./common";

export interface ScanHistorySummary {
  historyId: string;
  target: string;
  createdDate: string;
  duration: string | null;
  hasCompleted: boolean;
  summary: string | null;
  findingsCount: number;
  technologiesCount: number;
}

export interface ScanHistoryDetail {
  historyId: string;
  target: string;
  rawJsonResult: string | null;
  createdDate: string;
  userId: string;
  duration: string | null;
  hasCompleted: boolean;
  summary: string | null;
  findings: FindingResponse[];
  technologies: TechnologyResponse[];
  iaSummary: IASummaryResponse | null;
}

export interface FindingResponse {
  findingId: string;
  type: string;
  description: string;
  severity: SeverityLevel;
  evidence: string | null;
  recommendation: string | null;
  historyId: string | null;
  createdAt: string;
}

export interface TechnologyResponse {
  technologyId: string;
  name: string;
  version: string | null;
  category: string;
  description: string | null;
  historyId: string | null;
  createdAt: string;
}

export interface IASummaryResponse {
  iaSummaryId: string;
  summaryText: string | null;
  mainCategory: string | null;
  overallRisk: string | null;
  totalFindings: number;
  findingsCritical: number;
  findingsHigh: number;
  findingsMedium: number;
  findingsLow: number;
  historyId: string | null;
  createdDate: string;
}

export interface ExecuteScanRequest {
  target: string;
}

export interface ExecuteScanResponse {
  historyId: string;
  target: string;
  summary: string;
  duration: string;
  hasCompleted: boolean;
  createdDate: string;
}

export interface DeleteScanHistoryResponse {
  success: boolean;
  historyId: string;
  target: string;
}
