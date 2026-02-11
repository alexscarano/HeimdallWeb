"use client";

import { Finding } from "@/lib/hooks/use-history";
import { Badge } from "@/components/ui/badge";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";

interface FindingsListProps {
  findings: Finding[];
}

export function FindingsList({ findings }: FindingsListProps) {
  if (findings.length === 0) {
    return (
      <div className="py-8 text-center text-sm text-muted-foreground">
        Nenhuma vulnerabilidade encontrada.
      </div>
    );
  }

  const groupedFindings = findings.reduce((acc, finding) => {
    if (!acc[finding.severity]) {
      acc[finding.severity] = [];
    }
    acc[finding.severity].push(finding);
    return acc;
  }, {} as Record<string, Finding[]>);

  return (
    <div className="space-y-4">
      {Object.entries(groupedFindings).map(([severity, items]) => (
        <div key={severity}>
          <Accordion type="single" collapsible className="space-y-2">
            {items.map((finding) => (
              <AccordionItem
                key={finding.findingId}
                value={finding.findingId.toString()}
                className={`rounded-lg border ${severityBarColor(finding.severity)}`}
              >
                <AccordionTrigger className="px-4 hover:no-underline">
                  <div className="flex items-center gap-3 text-left">
                    <Badge className={severityBadgeClass(finding.severity)}>
                      {finding.severity}
                    </Badge>
                    <span className="font-medium">{finding.type}</span>
                  </div>
                </AccordionTrigger>
                <AccordionContent className="px-4 pb-4">
                  <div className="space-y-3 pt-2">
                    <div>
                      <p className="text-sm font-medium">Descrição</p>
                      <p className="mt-1 text-sm text-muted-foreground">
                        {finding.description}
                      </p>
                    </div>

                    {finding.evidence && (
                      <div>
                        <p className="text-sm font-medium">Evidência</p>
                        <pre className="mt-1 rounded-md bg-muted p-3 text-xs">
                          {finding.evidence}
                        </pre>
                      </div>
                    )}

                    {finding.recommendation && (
                      <div>
                        <p className="text-sm font-medium">Recomendação</p>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {finding.recommendation}
                        </p>
                      </div>
                    )}
                  </div>
                </AccordionContent>
              </AccordionItem>
            ))}
          </Accordion>
        </div>
      ))}
    </div>
  );
}

function severityBadgeClass(severity: string): string {
  switch (severity) {
    case "Critical":
      return "bg-severity-critical-bg text-severity-critical border-severity-critical-border";
    case "High":
      return "bg-severity-high-bg text-severity-high border-severity-high-border";
    case "Medium":
      return "bg-severity-medium-bg text-severity-medium border-severity-medium-border";
    case "Low":
      return "bg-severity-low-bg text-severity-low border-severity-low-border";
    default:
      return "bg-severity-info-bg text-severity-info border-severity-info-border";
  }
}

function severityBarColor(severity: string): string {
  switch (severity) {
    case "Critical":
      return "border-l-4 border-l-severity-critical";
    case "High":
      return "border-l-4 border-l-severity-high";
    case "Medium":
      return "border-l-4 border-l-severity-medium";
    case "Low":
      return "border-l-4 border-l-severity-low";
    default:
      return "border-l-4 border-l-severity-info";
  }
}
