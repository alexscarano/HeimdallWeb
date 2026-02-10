export interface AdminDashboardResponse {
  userStats: UserStatsSection;
  scanStats: ScanStatsSection;
  logs: PaginatedLogsSection;
  recentActivity: RecentActivityItem[];
  scanTrend: TrendItem[];
  userRegistrationTrend: TrendItem[];
}

export interface UserStatsSection {
  totalUsers: number;
  activeUsers: number;
  blockedUsers: number;
  adminUsers: number;
  regularUsers: number;
}

export interface ScanStatsSection {
  totalScans: number;
  completedScans: number;
  incompleteScans: number;
  totalFindings: number;
  criticalFindings: number;
  highFindings: number;
  mediumFindings: number;
  lowFindings: number;
}

export interface PaginatedLogsSection {
  items: LogItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface LogItem {
  logId: string;
  timestamp: string;
  level: string;
  source: string;
  message: string;
  userId: string | null;
  username: string | null;
  remoteIp: string | null;
}

export interface RecentActivityItem {
  historyId: string;
  target: string;
  createdDate: string;
  userId: string;
  username: string;
  hasCompleted: boolean;
  findingsCount: number;
}

export interface TrendItem {
  date: string;
  count: number;
}
