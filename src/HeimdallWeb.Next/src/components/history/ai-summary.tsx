"use client";

import { AISummary } from "@/lib/hooks/use-history";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Brain, AlertTriangle } from "lucide-react";

interface AISummaryCardProps {
  summary: AISummary;
}

export function AISummaryCard({ summary }: AISummaryCardProps) {
  const riskColor = getRiskColor(summary.overallRisk ?? "");

  return (
    <div className="space-y-4">
      <Card className={`border-t-4 p-6 ${riskColor}`}>
        <div className="flex items-start gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-accent-primary-subtle">
            <Brain className="h-6 w-6 text-accent-primary" />
          </div>
          <div className="flex-1">
            <div className="flex items-center gap-2">
              <h3 className="text-lg font-semibold">Análise de IA - {summary.mainCategory}</h3>
              <Badge className={riskBadgeClass(summary.overallRisk ?? "")}>{summary.overallRisk}</Badge>
            </div>
            <p className="mt-2 text-sm text-muted-foreground">{summary.summaryText}</p>
          </div>
        </div>
      </Card>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
        <SeverityCard label="Critical" count={summary.findingsCritical} colorClass="border-t-severity-critical" />
        <SeverityCard label="High" count={summary.findingsHigh} colorClass="border-t-severity-high" />
        <SeverityCard label="Medium" count={summary.findingsMedium} colorClass="border-t-severity-medium" />
        <SeverityCard label="Low" count={summary.findingsLow} colorClass="border-t-severity-low" />
        <SeverityCard label="Info" count={summary.findingsInformational} colorClass="border-t-severity-info" />
      </div>
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

function getRiskColor(risk: string): string {
  switch (risk.toLowerCase()) {
    case "critical":
    case "alto":
    case "high":
      return "border-t-severity-critical";
    case "medium":
    case "médio":
      return "border-t-severity-medium";
    case "low":
    case "baixo":
      return "border-t-severity-low";
    default:
      return "border-t-severity-info";
  }
}

function riskBadgeClass(risk: string): string {
  switch (risk.toLowerCase()) {
    case "critical":
    case "alto":
      return "bg-severity-critical-bg text-severity-critical border-severity-critical-border";
    case "high":
      return "bg-severity-high-bg text-severity-high border-severity-high-border";
    case "medium":
    case "médio":
      return "bg-severity-medium-bg text-severity-medium border-severity-medium-border";
    case "low":
    case "baixo":
      return "bg-severity-low-bg text-severity-low border-severity-low-border";
    default:
      return "bg-severity-info-bg text-severity-info border-severity-info-border";
  }
}
