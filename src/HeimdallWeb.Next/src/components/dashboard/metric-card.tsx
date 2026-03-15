"use client";

import { LucideIcon } from "lucide-react";
import { Card } from "@/components/ui/card";
import { CountUp } from "@/components/ui/count-up";

interface MetricCardProps {
  title: string;
  value: string | number;
  icon: LucideIcon;
  borderColorClass: string;
  animateValue?: boolean;
}

export function MetricCard({ title, value, icon: Icon, borderColorClass, animateValue }: MetricCardProps) {
  const isNumber = typeof value === "number";

  return (
    <Card className={`border-t-[3px] p-6 ${borderColorClass}`}>
      <div className="flex items-center gap-4">
        <div className="icon-box flex h-12 w-12 items-center justify-center rounded-lg bg-muted text-muted-foreground">
          <Icon className="h-6 w-6" />
        </div>
        <div className="flex-1">
          <p className="text-sm text-muted-foreground">{title}</p>
          <p className="mt-1 text-2xl font-bold">
            {animateValue && isNumber ? (
              <CountUp end={value as number} duration={1.5} />
            ) : (
              value
            )}
          </p>
        </div>
      </div>
    </Card>
  );
}
