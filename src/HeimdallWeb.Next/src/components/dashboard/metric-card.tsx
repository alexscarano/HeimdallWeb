import { LucideIcon } from "lucide-react";
import { Card } from "@/components/ui/card";

interface MetricCardProps {
  title: string;
  value: string | number;
  icon: LucideIcon;
  borderColorClass: string;
}

export function MetricCard({ title, value, icon: Icon, borderColorClass }: MetricCardProps) {
  return (
    <Card className={`border-t-[3px] p-6 ${borderColorClass}`}>
      <div className="flex items-center gap-4">
        <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-accent-primary-subtle">
          <Icon className="h-6 w-6 text-accent-primary" />
        </div>
        <div className="flex-1">
          <p className="text-sm text-muted-foreground">{title}</p>
          <p className="mt-1 text-2xl font-bold">{value}</p>
        </div>
      </div>
    </Card>
  );
}
