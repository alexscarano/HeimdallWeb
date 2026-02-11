"use client";

import { Shield } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";

interface ScanLoadingProps {
  elapsedSeconds: number;
  timeoutSeconds: number;
}

export function ScanLoading({ elapsedSeconds, timeoutSeconds }: ScanLoadingProps) {
  const progress = Math.min((elapsedSeconds / timeoutSeconds) * 100, 100);

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return m > 0 ? `${m}m ${s}s` : `${s}s`;
  };

  const getStageLabel = () => {
    if (elapsedSeconds < 10) return "Verificando headers de segurança...";
    if (elapsedSeconds < 20) return "Analisando certificado SSL/TLS...";
    if (elapsedSeconds < 35) return "Escaneando portas abertas...";
    if (elapsedSeconds < 50) return "Verificando caminhos sensíveis...";
    if (elapsedSeconds < 65) return "Gerando análise de IA...";
    return "Finalizando análise...";
  };

  return (
    <Card className="w-full max-w-lg border shadow-lg">
      <CardContent className="flex flex-col items-center gap-6 py-10">
        <div className="relative flex h-20 w-20 items-center justify-center">
          <div className="absolute inset-0 animate-spin rounded-full border-2 border-transparent border-t-accent-primary" />
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-accent-primary-subtle">
            <Shield className="h-8 w-8 text-accent-primary" />
          </div>
        </div>

        <div className="space-y-1 text-center">
          <p className="text-sm font-medium">{getStageLabel()}</p>
          <p className="text-xs text-muted-foreground">
            {formatTime(elapsedSeconds)} / {formatTime(timeoutSeconds)} máx.
          </p>
        </div>

        <div className="w-full space-y-2">
          <div className="h-1 w-full overflow-hidden rounded-full bg-accent-primary-subtle">
            <div
              className="h-full rounded-full bg-accent-primary transition-all duration-1000 ease-linear"
              style={{ width: `${progress}%` }}
            />
          </div>
          <p className="text-center text-xs text-muted-foreground">
            {Math.round(progress)}%
          </p>
        </div>
      </CardContent>
    </Card>
  );
}
