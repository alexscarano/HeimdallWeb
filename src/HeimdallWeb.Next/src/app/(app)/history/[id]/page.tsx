"use client";

import { use, useState } from "react";
import { format } from "date-fns";
import { ArrowLeft, FileDown, Shield, ToggleLeft, ToggleRight } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
  useScanHistoryDetail,
  useScanFindings,
  useScanTechnologies,
  useExportPdf,
} from "@/lib/hooks/use-history";
import { FindingsList } from "@/components/history/findings-list";
import { RiskCards } from "@/components/history/risk-cards";
import { ScoreTimeline } from "@/components/history/score-timeline";
import { JsonViewer } from "@/components/history/json-viewer";
import { TechnologiesList } from "@/components/history/technologies-list";
import { ScoreGauge, GradeBadge } from "@/components/scan/score-gauge";
import { ScannerResultCards } from "@/components/scan/scanner-result-cards";

interface Props {
  params: Promise<{ id: string }>;
}

export default function HistoryDetailPage({ params }: Props) {
  const resolvedParams = use(params);
  const scanId = resolvedParams.id;
  const [isAdvancedView, setIsAdvancedView] = useState(false);

  const { data: scan, isLoading: scanLoading } = useScanHistoryDetail(scanId);
  const { data: findings, isLoading: findingsLoading } = useScanFindings(scanId);
  const { data: technologies, isLoading: techLoading } = useScanTechnologies(scanId);
  const exportMutation = useExportPdf();

  if (scanLoading) {
    return <DetailSkeleton />;
  }

  if (!scan) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <p className="text-base font-medium">Scan não encontrado</p>
        <Link href="/history" className="mt-4">
          <Button variant="outline">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Voltar ao histórico
          </Button>
        </Link>
      </div>
    );
  }

  const severityCounts = findings?.reduce(
    (acc, f) => {
      acc[f.severity] = (acc[f.severity] || 0) + 1;
      return acc;
    },
    {} as Record<string, number>
  );

  return (
    <div className="space-y-6">
      {/* Header - Responsive */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="flex items-center gap-3">
          <Link href="/history">
            <Button variant="ghost" size="icon" aria-label="Voltar ao histórico">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent-primary-subtle">
            <Shield className="h-5 w-5 text-accent-primary" />
          </div>
          <div className="min-w-0 flex-1">
            <div className="flex items-center gap-2">
              <h1 className="truncate text-xl font-semibold tracking-tight sm:text-2xl">
                {scan.target}
              </h1>
              {scan.grade && <GradeBadge grade={scan.grade} score={scan.score} />}
            </div>
            <p className="text-xs text-muted-foreground sm:text-sm">
              {format(new Date(scan.createdDate), "dd/MM/yyyy, HH:mm")} • {scan.duration}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {/* View mode toggle */}
          <Button
            variant="outline"
            size="sm"
            onClick={() => setIsAdvancedView(!isAdvancedView)}
            className="gap-2"
          >
            {isAdvancedView ? (
              <ToggleRight className="h-4 w-4" />
            ) : (
              <ToggleLeft className="h-4 w-4" />
            )}
            {isAdvancedView ? "Avançado" : "Simples"}
          </Button>

          {/* Export PDF button */}
          <Button
            onClick={() => exportMutation.mutate(scanId)}
            disabled={exportMutation.isPending}
            variant="outline"
            size="sm"
          >
            <FileDown className="mr-2 h-4 w-4" />
            {exportMutation.isPending ? "Exportando..." : "Exportar PDF"}
          </Button>
        </div>
      </div>

      {/* Hero Score Block */}
      {(scan.score != null || severityCounts) && (
        <Card className="overflow-hidden rounded-2xl border shadow-sm">
          <CardContent className="p-6">
            <div className="flex flex-col items-center gap-6 sm:flex-row sm:items-center">
              {/* Score Gauge */}
              {scan.score != null && (
                <div className="shrink-0">
                  <ScoreGauge score={scan.score} grade={scan.grade} size={160} strokeWidth={10} />
                </div>
              )}

              {/* Divider (vertical on desktop) */}
              {scan.score != null && severityCounts && (
                <div className="hidden sm:block w-px self-stretch bg-border" />
              )}

              {/* Right side: Risk + Severity counts */}
              <div className="flex-1 space-y-4 w-full">
                {/* Risk level pill */}
                {scan.grade && (
                  <div className="flex items-center gap-2">
                    <RiskLevelPill grade={scan.grade} />
                    <span className="text-sm text-muted-foreground">Nível de Risco Geral</span>
                  </div>
                )}

                {/* Severity counts */}
                {severityCounts && (
                  <div className="grid gap-3 grid-cols-2 sm:grid-cols-5">
                    <SeverityCard label="Crítico" count={severityCounts.Critical || 0} colorClass="border-t-[3px] border-t-red-500" />
                    <SeverityCard label="Alto" count={severityCounts.High || 0} colorClass="border-t-[3px] border-t-orange-500" />
                    <SeverityCard label="Médio" count={severityCounts.Medium || 0} colorClass="border-t-[3px] border-t-amber-500" />
                    <SeverityCard label="Baixo" count={severityCounts.Low || 0} colorClass="border-t-[3px] border-t-emerald-500" />
                    <SeverityCard label="Info" count={severityCounts.Informational || 0} colorClass="border-t-[3px] border-t-blue-500" />
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Simple view: Scanner result cards */}
      {!isAdvancedView && scan.rawJsonResult && (
        <ScannerResultCards rawJson={scan.rawJsonResult} />
      )}

      {/* Advanced view: tabs */}
      {isAdvancedView && (
        <Tabs defaultValue="findings" className="space-y-4">
          <div>
            <TabsList className="grid w-full grid-cols-5 sm:inline-flex sm:w-auto">
              <TabsTrigger value="findings" className="sm:px-6">
                Vulnerabilidades
              </TabsTrigger>
              <TabsTrigger value="technologies" className="sm:px-6">
                Tecnologias
              </TabsTrigger>
              <TabsTrigger value="ai" className="sm:px-6">
                Análise de IA
              </TabsTrigger>
              <TabsTrigger value="timeline" className="sm:px-6">
                Evolução
              </TabsTrigger>
              <TabsTrigger value="json" className="sm:px-6">
                JSON
              </TabsTrigger>
            </TabsList>
          </div>

          <TabsContent value="findings">
            {findingsLoading ? (
              <Skeleton className="h-64 w-full" />
            ) : (
              <FindingsList findings={findings || []} />
            )}
          </TabsContent>

          <TabsContent value="technologies">
            {techLoading ? (
              <Skeleton className="h-64 w-full" />
            ) : (
              <TechnologiesList technologies={technologies || []} />
            )}
          </TabsContent>

          <TabsContent value="ai">
            <div className="py-8 text-center text-sm text-muted-foreground">
              Análise de IA não disponível para este scan.
            </div>
          </TabsContent>

          <TabsContent value="timeline">
            <ScoreTimeline currentScanId={scanId} target={scan.target} />
          </TabsContent>

          <TabsContent value="json">
            <JsonViewer json={scan.rawJsonResult ?? undefined} />
          </TabsContent>
        </Tabs>
      )}

      {/* Simple view: RiskCards grouped by severity */}
      {!isAdvancedView && (
        <div className="space-y-4">
          {findingsLoading ? (
            <Skeleton className="h-64 w-full" />
          ) : findings && findings.length > 0 ? (
            <div>
              <h3 className="mb-3 text-lg font-semibold">Vulnerabilidades Encontradas</h3>
              <RiskCards findings={findings} />
            </div>
          ) : null}
        </div>
      )}
    </div>
  );
}

function RiskLevelPill({ grade }: { grade: string | null | undefined }) {
  const riskMap: Record<string, { label: string; classes: string }> = {
    A: { label: "Baixo", classes: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400" },
    B: { label: "Baixo", classes: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400" },
    C: { label: "Médio", classes: "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400" },
    D: { label: "Alto", classes: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400" },
    F: { label: "Crítico", classes: "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400" },
  };

  const risk = riskMap[grade ?? "F"] ?? riskMap["F"];

  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ${risk.classes}`}>
      {risk.label}
    </span>
  );
}

function SeverityCard({ label, count, colorClass }: { label: string; count: number; colorClass: string }) {
  return (
    <Card className={`rounded-xl p-4 ${colorClass}`}>
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="mt-1 text-2xl font-bold">{count}</p>
    </Card>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Skeleton className="h-10 w-10 rounded-lg" />
        <div className="flex-1">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="mt-2 h-4 w-48" />
        </div>
      </div>
      <Skeleton className="h-[220px] w-full rounded-2xl" />
      <Skeleton className="h-96 w-full rounded-lg" />
    </div>
  );
}
