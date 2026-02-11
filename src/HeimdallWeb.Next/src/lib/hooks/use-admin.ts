import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import * as dashboardApi from "@/lib/api/dashboard.api";
import type { AdminDashboardResponse } from "@/types/dashboard";
import type { PaginatedUsersResponse } from "@/types/user";
import { toast } from "sonner";

export function useAdminDashboard(params?: {
  logPage?: number;
  logPageSize?: number;
  logLevel?: string;
}) {
  return useQuery<AdminDashboardResponse>({
    queryKey: ["admin-dashboard", params],
    queryFn: () => dashboardApi.getAdminDashboard(params),
  });
}

export function useAdminUsers(params?: {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
  isAdmin?: boolean;
}) {
  return useQuery<PaginatedUsersResponse>({
    queryKey: ["admin-users", params],
    queryFn: () => dashboardApi.getUsers(params),
  });
}

export function useToggleUserStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, isActive }: { userId: string; isActive: boolean }) =>
      dashboardApi.toggleUserStatus(userId, isActive),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["admin-users"] });
      queryClient.invalidateQueries({ queryKey: ["admin-dashboard"] });
      toast.success(
        `Usuário ${data.username} ${data.isActive ? "ativado" : "desativado"}`
      );
    },
    onError: () => {
      toast.error("Erro ao alterar status do usuário");
    },
  });
}

export function useDeleteUserByAdmin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => dashboardApi.deleteUserByAdmin(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-users"] });
      queryClient.invalidateQueries({ queryKey: ["admin-dashboard"] });
      toast.success("Usuário deletado com sucesso");
    },
    onError: () => {
      toast.error("Erro ao deletar usuário");
    },
  });
}
