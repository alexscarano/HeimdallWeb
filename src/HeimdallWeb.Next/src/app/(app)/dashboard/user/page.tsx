"use client";

import { BarChart3, TrendingUp, AlertTriangle, Activity } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { MetricCard } from "@/components/dashboard/metric-card";
import { ChartCard } from "@/components/dashboard/chart-card";
import { useUserDashboard } from "@/lib/hooks/use-dashboard";
import {
  AreaChart,
  Area,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";

export default function UserDashboardPage() {
  const { data: stats, isLoading } = useUserDashboard();

  if (isLoading) {
    return <DashboardSkeleton />;
  }

  if (!stats) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <p className="text-base font-medium">Erro ao carregar dashboard</p>
        <p className="mt-1 text-sm text-muted-foreground">
          Tente recarregar a página.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent-primary-subtle">
          <BarChart3 className="h-5 w-5 text-accent-primary" />
        </div>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Meu Dashboard</h1>
          <p className="text-sm text-muted-foreground">
            Estatísticas dos seus scans de segurança
          </p>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard
          title="Total de Scans"
          value={stats.totalScans}
          icon={Activity}
          borderColorClass="border-t-accent-primary"
        />
        <MetricCard
          title="Scans Completos"
          value={stats.completedScans}
          icon={TrendingUp}
          borderColorClass="border-t-success"
        />
        <MetricCard
          title="Vulnerabilidades"
          value={stats.totalFindings}
          icon={AlertTriangle}
          borderColorClass="border-t-warning"
        />
        <MetricCard
          title="Duração Média"
          value={stats.averageDuration ?? "—"}
          icon={Activity}
          borderColorClass="border-t-chart-2"
        />
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
        <SeverityMetricCard label="Critical" count={stats.criticalFindings} colorClass="border-t-severity-critical" />
        <SeverityMetricCard label="High" count={stats.highFindings} colorClass="border-t-severity-high" />
        <SeverityMetricCard label="Medium" count={stats.mediumFindings} colorClass="border-t-severity-medium" />
        <SeverityMetricCard label="Low" count={stats.lowFindings} colorClass="border-t-severity-low" />
        <SeverityMetricCard label="Info" count={stats.informationalFindings} colorClass="border-t-severity-info" />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        {stats.riskTrend && stats.riskTrend.length > 0 && (
          <ChartCard title="Tendência de Risco" subtitle="Vulnerabilidades encontradas ao longo do tempo">
            <ResponsiveContainer width="100%" height={250}>
              <AreaChart data={stats.riskTrend}>
                <defs>
                  <linearGradient id="colorFindings" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#059669" stopOpacity={0.1} />
                    <stop offset="95%" stopColor="#059669" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis dataKey="date" stroke="hsl(var(--muted-foreground))" fontSize={12} />
                <YAxis stroke="hsl(var(--muted-foreground))" fontSize={12} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(var(--popover))",
                    border: "1px solid hsl(var(--border))",
                    borderRadius: "0.5rem",
                  }}
                />
                <Area
                  type="monotone"
                  dataKey="findingsCount"
                  stroke="#059669"
                  fillOpacity={1}
                  fill="url(#colorFindings)"
                />
              </AreaChart>
            </ResponsiveContainer>
          </ChartCard>
        )}

        {stats.categoryBreakdown && stats.categoryBreakdown.length > 0 && (
          <ChartCard title="Vulnerabilidades por Categoria" subtitle="Distribuição de tipos de findings">
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={stats.categoryBreakdown} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis type="number" stroke="hsl(var(--muted-foreground))" fontSize={12} />
                <YAxis
                  dataKey="category"
                  type="category"
                  stroke="hsl(var(--muted-foreground))"
                  fontSize={12}
                  width={100}
                />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(var(--popover))",
                    border: "1px solid hsl(var(--border))",
                    borderRadius: "0.5rem",
                  }}
                />
                <Bar dataKey="count" fill="#059669" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>
        )}
      </div>
    </div>
  );
}

function SeverityMetricCard({ label, count, colorClass }: { label: string; count: number; colorClass: string }) {
  return (
    <div className={`rounded-xl border border-t-[3px] p-4 ${colorClass}`}>
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="mt-0.5 text-2xl font-bold">{count}</p>
    </div>
  );
}

function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-64" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-24 rounded-xl" />
        ))}
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-20 rounded-xl" />
        ))}
      </div>
      <div className="grid gap-4 lg:grid-cols-2">
        <Skeleton className="h-80 rounded-xl" />
        <Skeleton className="h-80 rounded-xl" />
      </div>
    </div>
  );
}
