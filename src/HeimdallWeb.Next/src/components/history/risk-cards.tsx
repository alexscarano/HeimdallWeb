"use client";

import { useState } from "react";
import {
  AlertCircle,
  AlertTriangle,
  AlertOctagon,
  Info,
  ChevronDown,
  ChevronUp,
  ShieldAlert,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { FindingResponse } from "@/types/scan";

interface RiskCardsProps {
  findings: FindingResponse[];
}

type Severity = "Critical" | "High" | "Medium" | "Low" | "Informational";

const SEVERITY_ORDER: Severity[] = [
  "Critical",
  "High",
  "Medium",
  "Low",
  "Informational",
];

const SEVERITY_CONFIG: Record<
  Severity,
  {
    label: string;
    cardClasses: string;
    badgeClasses: string;
    icon: React.ComponentType<{ className?: string }>;
  }
> = {
  Critical: {
    label: "Crítico",
    cardClasses: "border-l-4 border-l-red-500 bg-red-500/10",
    badgeClasses: "bg-red-100 text-red-700 border-red-200 dark:bg-red-900/30 dark:text-red-400 dark:border-red-800",
    icon: AlertOctagon,
  },
  High: {
    label: "Alto",
    cardClasses: "border-l-4 border-l-orange-500 bg-orange-500/10",
    badgeClasses: "bg-orange-100 text-orange-700 border-orange-200 dark:bg-orange-900/30 dark:text-orange-400 dark:border-orange-800",
    icon: AlertCircle,
  },
  Medium: {
    label: "Médio",
    cardClasses: "border-l-4 border-l-yellow-500 bg-yellow-500/10",
    badgeClasses: "bg-yellow-100 text-yellow-700 border-yellow-200 dark:bg-yellow-900/30 dark:text-yellow-400 dark:border-yellow-800",
    icon: AlertTriangle,
  },
  Low: {
    label: "Baixo",
    cardClasses: "border-l-4 border-l-blue-500 bg-blue-500/10",
    badgeClasses: "bg-blue-100 text-blue-700 border-blue-200 dark:bg-blue-900/30 dark:text-blue-400 dark:border-blue-800",
    icon: ShieldAlert,
  },
  Informational: {
    label: "Info",
    cardClasses: "border-l-4 border-l-zinc-400 bg-zinc-400/5",
    badgeClasses: "bg-zinc-100 text-zinc-600 border-zinc-200 dark:bg-zinc-800/50 dark:text-zinc-400 dark:border-zinc-700",
    icon: Info,
  },
};

interface FindingItemProps {
  finding: FindingResponse;
}

function FindingItem({ finding }: FindingItemProps) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="border-b border-border/50 last:border-0">
      <button
        type="button"
        className="flex w-full items-start justify-between gap-3 py-3 text-left hover:bg-background/50 transition-colors rounded px-2 -mx-2"
        onClick={() => setExpanded((prev) => !prev)}
        aria-expanded={expanded}
      >
        <div className="min-w-0 flex-1">
          <p className="font-medium text-sm leading-snug">{finding.type}</p>
          {!expanded && finding.recommendation && (
            <p className="mt-0.5 text-xs text-muted-foreground line-clamp-2">
              {finding.recommendation}
            </p>
          )}
        </div>
        <span className="mt-0.5 shrink-0 text-muted-foreground">
          {expanded ? (
            <ChevronUp className="h-4 w-4" />
          ) : (
            <ChevronDown className="h-4 w-4" />
          )}
        </span>
      </button>

      {expanded && (
        <div className="space-y-3 pb-3 px-2 -mx-2">
          {finding.description && (
            <div>
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-1">
                Descrição
              </p>
              <p className="text-sm text-foreground/80">{finding.description}</p>
            </div>
          )}

          {finding.evidence && (
            <div>
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-1">
                Evidência
              </p>
              <pre className="overflow-x-auto rounded-md bg-muted p-2.5 text-xs whitespace-pre-wrap break-words">
                {finding.evidence}
              </pre>
            </div>
          )}

          {finding.recommendation && (
            <div>
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-1">
                Recomendação
              </p>
              <p className="text-sm text-muted-foreground">{finding.recommendation}</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export function RiskCards({ findings }: RiskCardsProps) {
  if (findings.length === 0) {
    return (
      <div className="py-8 text-center text-sm text-muted-foreground">
        Nenhuma vulnerabilidade encontrada.
      </div>
    );
  }

  const grouped = findings.reduce<Record<Severity, FindingResponse[]>>(
    (acc, finding) => {
      const sev = finding.severity as Severity;
      if (!acc[sev]) acc[sev] = [];
      acc[sev].push(finding);
      return acc;
    },
    {} as Record<Severity, FindingResponse[]>
  );

  const visibleGroups = SEVERITY_ORDER.filter(
    (sev) => grouped[sev] && grouped[sev].length > 0
  );

  return (
    <div className="space-y-4">
      {visibleGroups.map((severity) => {
        const config = SEVERITY_CONFIG[severity];
        const Icon = config.icon;
        const items = grouped[severity];

        return (
          <div
            key={severity}
            className={`rounded-lg p-4 ${config.cardClasses}`}
            role="region"
            aria-label={`Vulnerabilidades ${config.label}`}
          >
            {/* Card header */}
            <div className="flex items-center gap-2 mb-3">
              <Icon className="h-4 w-4 shrink-0" />
              <span className="font-semibold text-sm">{config.label}</span>
              <Badge
                className={`ml-auto text-xs ${config.badgeClasses}`}
                variant="outline"
              >
                {items.length}
              </Badge>
            </div>

            {/* Findings list */}
            <div className="divide-y divide-border/40">
              {items.map((finding) => (
                <FindingItem key={finding.findingId} finding={finding} />
              ))}
            </div>
          </div>
        );
      })}
    </div>
  );
}
