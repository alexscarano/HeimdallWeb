"use client";

import {
  CheckCircle2,
  XCircle,
  ExternalLink,
  Clock,
  RotateCcw,
} from "lucide-react";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import type { ExecuteScanResponse } from "@/types/scan";
import { routes } from "@/lib/constants/routes";
import Link from "next/link";

interface ScanResultSummaryProps {
  result: ExecuteScanResponse;
  onNewScan: () => void;
}

export function ScanResultSummary({ result, onNewScan }: ScanResultSummaryProps) {
  const isCompleted = result.hasCompleted;

  return (
    <Card className="w-full max-w-lg border shadow-lg">
      <CardContent className="flex flex-col items-center gap-5 pt-8 pb-4">
        <div
          className={`flex h-14 w-14 items-center justify-center rounded-2xl ${
            isCompleted ? "bg-success/10" : "bg-destructive/10"
          }`}
        >
          {isCompleted ? (
            <CheckCircle2 className="h-7 w-7 text-success" />
          ) : (
            <XCircle className="h-7 w-7 text-destructive" />
          )}
        </div>

        <div className="space-y-1 text-center">
          <h3 className="text-lg font-semibold">
            {isCompleted ? "Scan concluído" : "Scan falhou"}
          </h3>
          <p className="max-w-sm text-sm text-muted-foreground">
            {result.summary || (isCompleted ? "Análise finalizada com sucesso." : "Ocorreu um erro durante o scan.")}
          </p>
        </div>

        <div className="flex flex-wrap items-center justify-center gap-2">
          <Badge variant="outline" className="gap-1.5 font-mono text-xs">
            <ExternalLink className="h-3 w-3" />
            {result.target}
          </Badge>
          {result.duration && (
            <Badge variant="secondary" className="gap-1.5 text-xs">
              <Clock className="h-3 w-3" />
              {result.duration}
            </Badge>
          )}
          <Badge variant={isCompleted ? "default" : "destructive"} className="text-xs">
            {isCompleted ? "Completo" : "Falhou"}
          </Badge>
        </div>
      </CardContent>

      <CardFooter className="flex flex-col gap-2 pt-2 pb-6 sm:flex-row sm:justify-center">
        {isCompleted && (
          <Button asChild className="bg-accent-primary text-accent-primary-foreground hover:bg-accent-primary-hover">
            <Link href={routes.historyDetail(result.historyId)}>
              Ver detalhes completos
            </Link>
          </Button>
        )}
        <Button variant="outline" onClick={onNewScan} className="gap-2">
          <RotateCcw className="h-4 w-4" />
          Novo scan
        </Button>
      </CardFooter>
    </Card>
  );
}
