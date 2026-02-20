import { apiClient } from "./client";

export interface MonitoredTarget {
  id: number;
  url: string;
  frequency: "Daily" | "Weekly" | "Monthly";
  lastCheck: string | null;
  nextCheck: string | null;
  isActive: boolean;
}

export interface MonitorHistoryEntry {
  id: number;
  score: number | null;
  grade: string | null;
  findingsCount: number;
  criticalCount: number;
  highCount: number;
  createdAt: string;
}

export const monitorApi = {
  getAll: async (): Promise<MonitoredTarget[]> => {
    const { data } = await apiClient.get("/monitor");
    return data;
  },
  create: async (url: string, frequency: string): Promise<MonitoredTarget> => {
    const { data } = await apiClient.post("/monitor", { url, frequency });
    return data;
  },
  remove: async (id: number): Promise<void> => {
    await apiClient.delete(`/monitor/${id}`);
  },
  getHistory: async (id: number): Promise<MonitorHistoryEntry[]> => {
    const { data } = await apiClient.get(`/monitor/${id}/history`);
    return data;
  },
  getDistinctTargets: async (): Promise<string[]> => {
    const { data } = await apiClient.get("/scans/distinct-targets");
    return data;
  },
};
