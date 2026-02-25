import { apiClient } from "./client";

export interface NotificationItem {
  id: number;
  title: string;
  body: string;
  type: "ScanComplete" | "RiskAlert";
  isRead: boolean;
  createdAt: string;
  readAt: string | null;
}

export interface UnreadCountResponse {
  count: number;
}

export const notificationsApi = {
  getAll: async (page = 1, pageSize = 10): Promise<NotificationItem[]> => {
    const { data } = await apiClient.get("/notifications", {
      params: { page, pageSize },
    });
    return data;
  },
  getUnreadCount: async (): Promise<number> => {
    const { data } = await apiClient.get<UnreadCountResponse>(
      "/notifications/unread-count"
    );
    return data.count;
  },
  markAsRead: async (id: number): Promise<void> => {
    await apiClient.patch(`/notifications/${id}/read`);
  },
  markAllAsRead: async (): Promise<void> => {
    await apiClient.patch("/notifications/read-all");
  },
  clearAll: async (): Promise<void> => {
    await apiClient.delete("/notifications/clear-all");
  },
};
