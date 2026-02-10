import { apiClient } from "./client";
import { endpoints } from "./endpoints";
import type { AdminDashboardResponse } from "@/types/dashboard";
import type {
  UserListItem,
  ToggleUserStatusResponse,
  DeleteUserByAdminResponse,
  PaginatedUsersResponse,
} from "@/types/user";

export async function getAdminDashboard(params?: {
  logPage?: number;
  logPageSize?: number;
  logLevel?: string;
  logStartDate?: string;
  logEndDate?: string;
}): Promise<AdminDashboardResponse> {
  const response = await apiClient.get<AdminDashboardResponse>(endpoints.dashboard.admin, {
    params,
  });
  return response.data;
}

export async function getUsers(params?: {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
  isAdmin?: boolean;
  createdFrom?: string;
  createdTo?: string;
}): Promise<PaginatedUsersResponse> {
  const response = await apiClient.get<PaginatedUsersResponse>(endpoints.dashboard.users, {
    params,
  });
  return response.data;
}

export async function toggleUserStatus(
  userId: string,
  isActive: boolean
): Promise<ToggleUserStatusResponse> {
  const response = await apiClient.patch<ToggleUserStatusResponse>(
    endpoints.admin.toggleUserStatus(userId),
    { isActive }
  );
  return response.data;
}

export async function deleteUserByAdmin(userId: string): Promise<DeleteUserByAdminResponse> {
  const response = await apiClient.delete<DeleteUserByAdminResponse>(
    endpoints.admin.deleteUser(userId)
  );
  return response.data;
}
