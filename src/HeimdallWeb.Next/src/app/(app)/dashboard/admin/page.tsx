"use client";

import { useState, useRef, useEffect } from "react";
import { useTheme } from "next-themes";
import { format } from "date-fns";
import {
  Users,
  Shield,
  Activity,
  AlertTriangle,
  TrendingUp,
  BarChart3,
} from "lucide-react";
import { AdminGuard } from "@/components/layout/admin-guard";
import { useAdminDashboard } from "@/lib/hooks/use-admin";
import { MetricCard } from "@/components/dashboard/metric-card";
import { ChartCard } from "@/components/dashboard/chart-card";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
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
  PieChart,
  Pie,
  Cell,
  Legend,
} from "recharts";

const SEVERITY_COLORS = ["#ef4444", "#f97316", "#eab308", "#3b82f6"];

export default function AdminDashboardPage() {
  return (
    <AdminGuard>
      <AdminDashboardContent />
    </AdminGuard>
  );
}

function AdminDashboardContent() {
  const [logPage, setLogPage] = useState(1);
  const [logLevel, setLogLevel] = useState<string>("all");
  const scrollPosRef = useRef<number>(0);
  const { resolvedTheme } = useTheme();
  const chartColor = resolvedTheme === "dark" ? "#6366f1" : "#059669";
  const tickColor = resolvedTheme === "dark" ? "#cbd5e1" : "#64748b";

  const { data, isLoading } = useAdminDashboard({
    logPage,
    logPageSize: 10,
    logLevel: logLevel === "all" ? undefined : logLevel,
  });

  // Restore scroll position after log page change
  useEffect(() => {
    if (scrollPosRef.current > 0) {
      const main = document.querySelector("main");
      if (main) main.scrollTop = scrollPosRef.current;
    }
  }, [data?.logs.items]);

  if (isLoading) return <AdminDashboardSkeleton />;
  if (!data) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <p className="text-base font-medium">Erro ao carregar dashboard</p>
        <p className="mt-1 text-sm text-muted-foreground">
          Verifique se você tem permissões de administrador.
        </p>
      </div>
    );
  }

  const severityData = [
    { name: "Critical", value: data.scanStats.criticalFindings },
    { name: "High", value: data.scanStats.highFindings },
    { name: "Medium", value: data.scanStats.mediumFindings },
    { name: "Low", value: data.scanStats.lowFindings },
  ].filter((d) => d.value > 0);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent-primary-subtle">
          <Shield className="h-5 w-5 text-accent-primary" />
        </div>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            Admin Dashboard
          </h1>
          <p className="text-sm text-muted-foreground">
            Visão geral do sistema
          </p>
        </div>
      </div>

      {/* User & Scan KPIs */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard
          title="Total de Usuários"
          value={data.userStats.totalUsers}
          icon={Users}
          borderColorClass="border-t-accent-primary"
        />
        <MetricCard
          title="Total de Scans"
          value={data.scanStats.totalScans}
          icon={Activity}
          borderColorClass="border-t-chart-2"
        />
        <MetricCard
          title="Scans Completos"
          value={data.scanStats.completedScans}
          icon={TrendingUp}
          borderColorClass="border-t-success"
        />
        <MetricCard
          title="Vulnerabilidades"
          value={data.scanStats.totalFindings}
          icon={AlertTriangle}
          borderColorClass="border-t-warning"
        />
      </div>

      {/* User Stats Row */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatMini label="Ativos" value={data.userStats.activeUsers} />
        <StatMini label="Bloqueados" value={data.userStats.blockedUsers} />
        <StatMini label="Admins" value={data.userStats.adminUsers} />
        <StatMini label="Regulares" value={data.userStats.regularUsers} />
      </div>

      {/* Charts */}
      <div className="grid gap-4 lg:grid-cols-2">
        {/* Severity Distribution Pie */}
        {severityData.length > 0 && (
          <ChartCard
            title="Distribuição de Severidade"
            subtitle="Findings por nível de severidade"
          >
            <ResponsiveContainer width="100%" height={250}>
              <PieChart>
                <Pie
                  data={severityData}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={100}
                  dataKey="value"
                >
                  {severityData.map((_, index) => (
                    <Cell
                      key={index}
                      fill={SEVERITY_COLORS[index % SEVERITY_COLORS.length]}
                    />
                  ))}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </ChartCard>
        )}

        {/* Scan Trend */}
        {data.scanTrend.length > 0 && (
          <ChartCard
            title="Tendência de Scans"
            subtitle="Scans realizados ao longo do tempo"
          >
            <ResponsiveContainer width="100%" height={250}>
              <AreaChart data={data.scanTrend}>
                <defs>
                  <linearGradient
                    id="colorScans"
                    x1="0"
                    y1="0"
                    x2="0"
                    y2="1"
                  >
                    <stop offset="5%" stopColor={chartColor} stopOpacity={0.1} />
                    <stop offset="95%" stopColor={chartColor} stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid
                  strokeDasharray="3 3"
                  stroke="hsl(var(--border))"
                />
                <XAxis
                  dataKey="date"
                  stroke={tickColor}
                  fontSize={12}
                  tick={{ fill: tickColor }}
                />
                <YAxis
                  stroke={tickColor}
                  fontSize={12}
                  tick={{ fill: tickColor }}
                />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(var(--popover))",
                    border: "1px solid hsl(var(--border))",
                    borderRadius: "0.5rem",
                  }}
                />
                <Area
                  type="monotone"
                  dataKey="count"
                  stroke={chartColor}
                  fillOpacity={1}
                  fill="url(#colorScans)"
                />
              </AreaChart>
            </ResponsiveContainer>
          </ChartCard>
        )}
      </div>

      {/* Recent Activity Table */}
      {data.recentActivity.length > 0 && (
        <Card className="p-6">
          <h3 className="mb-4 font-semibold">Atividade Recente</h3>
          <div className="rounded-lg border">
            <Table>
              <TableHeader>
                <TableRow className="bg-muted/50">
                  <TableHead>Usuário</TableHead>
                  <TableHead>Alvo</TableHead>
                  <TableHead>Data</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Findings</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.recentActivity.map((activity) => (
                  <TableRow key={activity.historyId} className="hover:bg-muted/50">
                    <TableCell className="font-medium">
                      {activity.username}
                    </TableCell>
                    <TableCell className="text-sm">{activity.target}</TableCell>
                    <TableCell className="text-sm text-muted-foreground">
                      {format(
                        new Date(activity.createdDate),
                        "dd/MM/yyyy, HH:mm"
                      )}
                    </TableCell>
                    <TableCell>
                      <Badge
                        className={
                          activity.hasCompleted
                            ? "bg-success-bg text-success border-success-border"
                            : "bg-destructive text-white"
                        }
                      >
                        {activity.hasCompleted ? "OK" : "Falha"}
                      </Badge>
                    </TableCell>
                    <TableCell className="font-medium">
                      {activity.findingsCount}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </Card>
      )}

      {/* Audit Logs */}
      <Card className="p-6">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-semibold">Logs de Auditoria</h3>
          <Select
            value={logLevel}
            onValueChange={(val) => {
              setLogLevel(val);
              setLogPage(1);
            }}
          >
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="Nível" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos</SelectItem>
              <SelectItem value="Info">Info</SelectItem>
              <SelectItem value="Warning">Warning</SelectItem>
              <SelectItem value="Error">Error</SelectItem>
              <SelectItem value="Critical">Critical</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div className="rounded-lg border">
          <Table>
            <TableHeader>
              <TableRow className="bg-muted/50">
                <TableHead>Data</TableHead>
                <TableHead>Nível</TableHead>
                <TableHead>Origem</TableHead>
                <TableHead>Mensagem</TableHead>
                <TableHead>Usuário</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.logs.items.map((log) => (
                <TableRow key={log.logId} className="hover:bg-muted/50">
                  <TableCell className="whitespace-nowrap text-sm text-muted-foreground">
                    {format(new Date(log.timestamp), "dd/MM HH:mm")}
                  </TableCell>
                  <TableCell>
                    <Badge className={logLevelClass(log.level)}>
                      {log.level}
                    </Badge>
                  </TableCell>
                  <TableCell className="max-w-[200px] truncate text-sm">
                    {log.source}
                  </TableCell>
                  <TableCell className="max-w-[300px] truncate text-sm">
                    {log.message}
                  </TableCell>
                  <TableCell className="text-sm">
                    {log.username ?? "—"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        {data.logs.totalPages > 1 && (
          <div className="mt-4 flex items-center justify-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={logPage <= 1}
              onClick={() => {
                scrollPosRef.current = document.querySelector("main")?.scrollTop ?? 0;
                setLogPage((p) => p - 1);
              }}
            >
              Anterior
            </Button>
            <span className="text-sm text-muted-foreground">
              {logPage} / {data.logs.totalPages}
            </span>
            <Button
              variant="outline"
              size="sm"
              disabled={logPage >= data.logs.totalPages}
              onClick={() => {
                scrollPosRef.current = document.querySelector("main")?.scrollTop ?? 0;
                setLogPage((p) => p + 1);
              }}
            >
              Próximo
            </Button>
          </div>
        )}
      </Card>
    </div>
  );
}

function StatMini({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border p-4">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="mt-0.5 text-2xl font-bold">{value}</p>
    </div>
  );
}

function logLevelClass(level: string): string {
  switch (level) {
    case "Critical":
      return "bg-severity-critical-bg text-severity-critical";
    case "Error":
      return "bg-destructive/10 text-destructive";
    case "Warning":
      return "bg-severity-medium-bg text-severity-medium";
    default:
      return "bg-muted text-muted-foreground";
  }
}

function AdminDashboardSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-64" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-24 rounded-xl" />
        ))}
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-16 rounded-xl" />
        ))}
      </div>
      <div className="grid gap-4 lg:grid-cols-2">
        <Skeleton className="h-80 rounded-xl" />
        <Skeleton className="h-80 rounded-xl" />
      </div>
      <Skeleton className="h-64 rounded-xl" />
    </div>
  );
}
