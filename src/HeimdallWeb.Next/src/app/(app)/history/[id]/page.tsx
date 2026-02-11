"use client";

import { use } from "react";
import { format } from "date-fns";
import { ArrowLeft, FileDown, Shield } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
  useScanHistoryDetail,
  useScanFindings,
  useScanTechnologies,
  useAISummary,
  useExportPdf,
} from "@/lib/hooks/use-history";
import { FindingsList } from "@/components/history/findings-list";
import { JsonViewer } from "@/components/history/json-viewer";
import { TechnologiesList } from "@/components/history/technologies-list";
import { AISummaryCard } from "@/components/history/ai-summary";

interface Props {
  params: Promise<{ id: string }>;
}

export default function HistoryDetailPage({ params }: Props) {
  const resolvedParams = use(params);
  const scanId = resolvedParams.id;

  const { data: scan, isLoading: scanLoading } = useScanHistoryDetail(scanId);
  const { data: findings, isLoading: findingsLoading } = useScanFindings(scanId);
  const { data: technologies, isLoading: techLoading } = useScanTechnologies(scanId);
  const { data: aiSummary, isLoading: aiLoading } = useAISummary(scanId);
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
            <h1 className="truncate text-xl font-semibold tracking-tight sm:text-2xl">
              {scan.target}
            </h1>
            <p className="text-xs text-muted-foreground sm:text-sm">
              {format(new Date(scan.createdDate), "dd/MM/yyyy, HH:mm")} • {scan.duration}
            </p>
          </div>
        </div>

        {/* Export PDF button - Ao lado em desktop */}
        <Button
          onClick={() => exportMutation.mutate(scanId)}
          disabled={exportMutation.isPending}
          variant="outline"
          className="w-full shrink-0 sm:w-auto sm:ml-4"
        >
          <FileDown className="mr-2 h-4 w-4" />
          {exportMutation.isPending ? "Exportando..." : "Exportar PDF"}
        </Button>
      </div>

      {severityCounts && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
          <SeverityCard label="Critical" count={severityCounts.Critical || 0} colorClass="border-t-severity-critical" />
          <SeverityCard label="High" count={severityCounts.High || 0} colorClass="border-t-severity-high" />
          <SeverityCard label="Medium" count={severityCounts.Medium || 0} colorClass="border-t-severity-medium" />
          <SeverityCard label="Low" count={severityCounts.Low || 0} colorClass="border-t-severity-low" />
          <SeverityCard label="Info" count={severityCounts.Informational || 0} colorClass="border-t-severity-info" />
        </div>
      )}

      <Tabs defaultValue="findings" className="space-y-4">
        {/* Tabs - Scrollable on mobile */}
        <div className="overflow-x-auto">
          <TabsList className="inline-flex w-full min-w-max sm:w-auto">
            <TabsTrigger value="findings" className="flex-1 sm:flex-none sm:px-6">
              Vulnerabilidades
            </TabsTrigger>
            <TabsTrigger value="technologies" className="flex-1 sm:flex-none sm:px-6">
              Tecnologias
            </TabsTrigger>
            <TabsTrigger value="ai" className="flex-1 sm:flex-none sm:px-6">
              Análise de IA
            </TabsTrigger>
            <TabsTrigger value="json" className="flex-1 sm:flex-none sm:px-6">
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
          {aiLoading ? (
            <Skeleton className="h-64 w-full" />
          ) : aiSummary ? (
            <AISummaryCard summary={aiSummary} />
          ) : (
            <div className="py-8 text-center text-sm text-muted-foreground">
              Análise de IA não disponível para este scan.
            </div>
          )}
        </TabsContent>

        <TabsContent value="json">
          <JsonViewer json={scan.rawJsonResult} />
        </TabsContent>
      </Tabs>
    </div>
  );
}

function SeverityCard({ label, count, colorClass }: { label: string; count: number; colorClass: string }) {
  return (
    <Card className={`border-t-[3px] p-4 ${colorClass}`}>
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
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-20 rounded-xl" />
        ))}
      </div>
      <Skeleton className="h-96 w-full rounded-lg" />
    </div>
  );
}
