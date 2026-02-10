import { SeverityLevel } from "@/types/common";

export const severityConfig = {
  [SeverityLevel.Critical]: {
    label: "Critical",
    className: "bg-severity-critical-bg text-severity-critical border-severity-critical-border",
  },
  [SeverityLevel.High]: {
    label: "High",
    className: "bg-severity-high-bg text-severity-high border-severity-high-border",
  },
  [SeverityLevel.Medium]: {
    label: "Medium",
    className: "bg-severity-medium-bg text-severity-medium border-severity-medium-border",
  },
  [SeverityLevel.Low]: {
    label: "Low",
    className: "bg-severity-low-bg text-severity-low border-severity-low-border",
  },
  [SeverityLevel.Informational]: {
    label: "Info",
    className: "bg-severity-info-bg text-severity-info border-severity-info-border",
  },
} as const;

export function getSeverityLabel(severity: SeverityLevel): string {
  return severityConfig[severity]?.label ?? "Unknown";
}

export function getSeverityClassName(severity: SeverityLevel): string {
  return severityConfig[severity]?.className ?? "";
}
