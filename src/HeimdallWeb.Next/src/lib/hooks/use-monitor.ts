"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { monitorApi } from "@/lib/api/monitor.api";

export function useMonitoredTargets() {
  return useQuery({
    queryKey: ["monitor"],
    queryFn: monitorApi.getAll,
  });
}

export function useCreateMonitor() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ url, frequency }: { url: string; frequency: string }) =>
      monitorApi.create(url, frequency),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["monitor"] });
    },
  });
}

export function useDeleteMonitor() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => monitorApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["monitor"] });
    },
  });
}

export function useMonitorHistory(id: number) {
  return useQuery({
    queryKey: ["monitor", id, "history"],
    queryFn: () => monitorApi.getHistory(id),
    enabled: id > 0,
  });
}

export function useDistinctTargets() {
  return useQuery({
    queryKey: ["scan-distinct-targets"],
    queryFn: monitorApi.getDistinctTargets,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}
