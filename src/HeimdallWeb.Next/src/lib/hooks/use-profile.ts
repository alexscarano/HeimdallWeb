import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@/stores/auth-store";
import * as userApi from "@/lib/api/user.api";
import type {
  UpdateUserRequest,
  UpdatePasswordRequest,
  UpdateProfileImageRequest,
} from "@/types/user";
import { toast } from "sonner";

export function useUpdateProfile() {
  const { user, refreshUser } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: UpdateUserRequest) => {
      if (!user?.userId) throw new Error("Não autenticado");
      return userApi.updateUser(user.userId, data);
    },
    onSuccess: () => {
      refreshUser();
      queryClient.invalidateQueries({ queryKey: ["user-dashboard"] });
      toast.success("Perfil atualizado com sucesso");
    },
    onError: () => {
      toast.error("Erro ao atualizar perfil");
    },
  });
}

export function useUpdatePassword() {
  const { user } = useAuth();

  return useMutation({
    mutationFn: async (data: UpdatePasswordRequest) => {
      if (!user?.userId) throw new Error("Não autenticado");
      return userApi.updatePassword(user.userId, data);
    },
    onSuccess: () => {
      toast.success("Senha atualizada com sucesso");
    },
    onError: (error: unknown) => {
      const msg =
        error instanceof Error ? error.message : "Erro ao atualizar senha";
      toast.error(msg);
    },
  });
}

export function useUpdateProfileImage() {
  const { user, refreshUser } = useAuth();

  return useMutation({
    mutationFn: async (data: UpdateProfileImageRequest) => {
      if (!user?.userId) throw new Error("Não autenticado");
      return userApi.updateProfileImage(user.userId, data);
    },
    onSuccess: () => {
      refreshUser();
      toast.success("Imagem atualizada com sucesso");
    },
    onError: () => {
      toast.error("Erro ao atualizar imagem");
    },
  });
}

export function useDeleteAccount() {
  const { user, logout } = useAuth();

  return useMutation({
    mutationFn: async (password: string) => {
      if (!user?.userId) throw new Error("Não autenticado");
      return userApi.deleteUser(user.userId, password, true);
    },
    onSuccess: async () => {
      toast.success("Conta deletada com sucesso");
      await logout();
    },
    onError: () => {
      toast.error("Erro ao deletar conta. Verifique sua senha.");
    },
  });
}
