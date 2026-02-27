"use client";

import { Shield } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { useEffect, useState } from "react";

const MESSAGES = [
  "Verificando segurança",
  "Analisando TLS",
  "Escaneando portas",
  "Consultando histórico",
  "Processando com IA",
  "Quase lá",
];

interface ScanLoadingProps {
  elapsedSeconds: number;
  timeoutSeconds: number;
}

export function ScanLoading({ elapsedSeconds, timeoutSeconds }: ScanLoadingProps) {
  const progress = Math.min((elapsedSeconds / timeoutSeconds) * 100, 100);
  const [messageIndex, setMessageIndex] = useState(0);

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return m > 0 ? `${m}m ${s}s` : `${s}s`;
  };

  useEffect(() => {
    const interval = setInterval(() => {
      setMessageIndex((prev) => (prev + 1) % MESSAGES.length);
    }, 12000);
    return () => clearInterval(interval);
  }, []);

  return (
    <Card className="mx-auto w-full max-w-lg border shadow-lg">
      <CardContent className="flex flex-col items-center gap-6 py-10">
        <div className="relative flex h-28 w-28 items-center justify-center">
          {/* Anéis pulsantes com delay escalonado */}
          <span className="absolute h-24 w-24 rounded-full border border-accent-primary/50 animate-ping [animation-duration:2s] [animation-delay:0ms]" />
          <span className="absolute h-24 w-24 rounded-full border border-accent-primary/35 animate-ping [animation-duration:2s] [animation-delay:667ms]" />
          <span className="absolute h-24 w-24 rounded-full border border-accent-primary/20 animate-ping [animation-duration:2s] [animation-delay:1334ms]" />

          {/* Shield com breathing suave */}
          <div className="icon-box flex h-16 w-16 items-center justify-center rounded-2xl bg-accent-primary-subtle animate-pulse [animation-duration:3s]">
            <Shield className="h-8 w-8 text-accent-primary" />
          </div>
        </div>

        <div className="space-y-1 text-center">
          <p className="text-sm font-medium flex items-center justify-center gap-1.5">
            {MESSAGES[messageIndex]}
            <span className="inline-flex items-center gap-0.5">
              <span className="animate-pulse [animation-delay:0ms] text-accent-primary text-[10px]">●</span>
              <span className="animate-pulse [animation-delay:200ms] text-accent-primary text-[10px]">●</span>
              <span className="animate-pulse [animation-delay:400ms] text-accent-primary text-[10px]">●</span>
            </span>
          </p>
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
