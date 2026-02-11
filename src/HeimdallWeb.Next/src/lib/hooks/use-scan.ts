"use client";

import { useState, useCallback, useRef, useEffect } from "react";
import { useMutation } from "@tanstack/react-query";
import { executeScan } from "@/lib/api/scan.api";
import type { ExecuteScanResponse } from "@/types/scan";

const SCAN_TIMEOUT_SECONDS = 75;

interface UseScanReturn {
  submit: (target: string) => void;
  result: ExecuteScanResponse | null;
  isScanning: boolean;
  elapsedSeconds: number;
  timeoutSeconds: number;
  error: string | null;
  reset: () => void;
}

export function useScan(): UseScanReturn {
  const [elapsedSeconds, setElapsedSeconds] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const stopTimer = useCallback(() => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = null;
    }
  }, []);

  const startTimer = useCallback(() => {
    setElapsedSeconds(0);
    stopTimer();
    timerRef.current = setInterval(() => {
      setElapsedSeconds((prev) => prev + 1);
    }, 1000);
  }, [stopTimer]);

  useEffect(() => {
    return () => stopTimer();
  }, [stopTimer]);

  const mutation = useMutation({
    mutationFn: (target: string) => executeScan({ target }),
    onMutate: () => {
      setError(null);
      startTimer();
    },
    onSuccess: () => {
      stopTimer();
    },
    onError: (err: unknown) => {
      stopTimer();
      const message =
        err instanceof Error ? err.message : "Erro ao executar o scan. Tente novamente.";
      setError(message);
    },
  });

  const submit = useCallback(
    (target: string) => {
      mutation.mutate(target);
    },
    [mutation]
  );

  const reset = useCallback(() => {
    mutation.reset();
    setElapsedSeconds(0);
    setError(null);
    stopTimer();
  }, [mutation, stopTimer]);

  return {
    submit,
    result: mutation.data ?? null,
    isScanning: mutation.isPending,
    elapsedSeconds,
    timeoutSeconds: SCAN_TIMEOUT_SECONDS,
    error,
    reset,
  };
}
