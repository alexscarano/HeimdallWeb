import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/stores/auth-store";

export interface UserStatistics {
  totalScans: number;
  completedScans: number;
  averageDuration: string | null;
  totalFindings: number;
  criticalFindings: number;
  highFindings: number;
  mediumFindings: number;
  lowFindings: number;
  informationalFindings: number;
  riskTrend?: Array<{ date: string; findingsCount: number }>;
  categoryBreakdown?: Array<{ category: string; count: number }>;
}

export function useUserDashboard() {
  const { user } = useAuth();
  const userId = user?.userId;

  return useQuery<UserStatistics>({
    queryKey: ["user-dashboard", userId],
    queryFn: async () => {
      if (!userId) throw new Error("Usuário não autenticado");
      const response = await fetch(`/api/v1/users/${userId}/statistics`);
      if (!response.ok) throw new Error("Erro ao carregar estatísticas");
      return response.json();
    },
    enabled: !!userId,
  });
}
