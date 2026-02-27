"use client";

import { useRouter } from "next/navigation";
import { format } from "date-fns";
import { ptBR } from "date-fns/locale";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { Skeleton } from "@/components/ui/skeleton";
import { useHistoryByTarget } from "@/lib/hooks/use-history";
import { routes } from "@/lib/constants/routes";
import type { TooltipProps } from "recharts";

interface ScoreTimelineProps {
  currentScanId: string;
  target: string;
}

function gradeColor(grade: string | null | undefined): string {
  switch (grade) {
    case "A":
      return "#059669"; // emerald-600
    case "B":
      return "#6366f1"; // indigo-500
    case "C":
      return "#eab308"; // yellow-500
    default:
      return "#ef4444"; // red-500
  }
}

interface ChartDataPoint {
  date: string;
  score: number;
  grade: string | null;
  id: string;
  fullDate: string;
}

interface CustomDotProps {
  cx?: number;
  cy?: number;
  payload?: ChartDataPoint;
  currentScanId: string;
  onDotClick: (id: string) => void;
}

function CustomDot({ cx, cy, payload, currentScanId, onDotClick }: CustomDotProps) {
  if (cx == null || cy == null || !payload) return null;

  const isCurrent = payload.id === currentScanId;
  const color = gradeColor(payload.grade);

  return (
    <circle
      cx={cx}
      cy={cy}
      r={isCurrent ? 8 : 5}
      fill={color}
      stroke={isCurrent ? "white" : "none"}
      strokeWidth={isCurrent ? 2 : 0}
      style={{ cursor: payload.id !== currentScanId ? "pointer" : "default" }}
      onClick={() => {
        if (payload.id !== currentScanId) {
          onDotClick(payload.id);
        }
      }}
      aria-label={`Score ${payload.score} - Grade ${payload.grade ?? "N/A"} - ${payload.fullDate}`}
      role="button"
      tabIndex={payload.id !== currentScanId ? 0 : undefined}
      onKeyDown={(e) => {
        if (e.key === "Enter" && payload.id !== currentScanId) {
          onDotClick(payload.id);
        }
      }}
    />
  );
}

interface CustomTooltipProps extends TooltipProps<number, string> {
  active?: boolean;
  payload?: Array<{ payload: ChartDataPoint }>;
  label?: string;
}

function CustomTooltip({ active, payload }: CustomTooltipProps) {
  if (!active || !payload || payload.length === 0) return null;

  const data = payload[0].payload;
  const color = gradeColor(data.grade);

  return (
    <div className="rounded-lg border border-border bg-background/95 backdrop-blur-sm p-3 shadow-lg text-sm">
      <p className="font-medium mb-1">{data.fullDate}</p>
      <div className="flex items-center gap-2">
        <span className="text-muted-foreground">Score:</span>
        <span className="font-bold" style={{ color }}>
          {data.score}
        </span>
        {data.grade && (
          <>
            <span className="text-muted-foreground">|</span>
            <span className="font-bold" style={{ color }}>
              {data.grade}
            </span>
          </>
        )}
      </div>
    </div>
  );
}

export function ScoreTimeline({ currentScanId, target }: ScoreTimelineProps) {
  const router = useRouter();
  const { data: histories, isLoading } = useHistoryByTarget(target);

  if (isLoading) {
    return <Skeleton className="h-[220px] w-full rounded-lg" />;
  }

  const validScans = (histories ?? [])
    .filter((s) => s.score != null)
    .sort(
      (a, b) =>
        new Date(a.createdDate).getTime() - new Date(b.createdDate).getTime()
    );

  if (validScans.length <= 1) {
    return (
      <div className="flex h-[220px] items-center justify-center rounded-lg border border-dashed">
        <p className="text-sm text-muted-foreground text-center px-4">
          Execute mais scans deste alvo para ver a evolução
        </p>
      </div>
    );
  }

  const rawChartData: ChartDataPoint[] = validScans.map((scan) => ({
    date: format(new Date(scan.createdDate), "dd/MM", { locale: ptBR }),
    fullDate: format(new Date(scan.createdDate), "dd/MM/yyyy, HH:mm", {
      locale: ptBR,
    }),
    score: scan.score as number,
    grade: scan.grade,
    id: scan.historyId,
  }));

  const hasSameDayDuplicates = rawChartData.some(
    (p, i) => rawChartData.findIndex((q) => q.date === p.date) !== i
  );

  const chartData: ChartDataPoint[] = hasSameDayDuplicates
    ? rawChartData.map((p, i) => ({
        ...p,
        date: format(new Date(validScans[i].createdDate), "dd/MM HH:mm", {
          locale: ptBR,
        }),
      }))
    : rawChartData;

  const handleDotClick = (id: string) => {
    router.push(`${routes.history}/${id}`);
  };

  return (
    <div className="rounded-lg border bg-card p-4">
      <p className="text-sm font-medium text-muted-foreground mb-4">
        Evolucao do score para:{" "}
        <span className="text-foreground font-semibold">{target}</span>
      </p>
      <ResponsiveContainer width="100%" height={220}>
        <LineChart
          data={chartData}
          margin={{ top: 8, right: 16, bottom: 0, left: -10 }}
        >
          <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
          <XAxis
            dataKey="date"
            tick={{ fontSize: 11 }}
            className="text-muted-foreground"
            tickLine={false}
            axisLine={false}
            {...(hasSameDayDuplicates
              ? { angle: -35, textAnchor: "end", height: 50 }
              : {})}
          />
          <YAxis
            domain={[0, 100]}
            tick={{ fontSize: 11 }}
            className="text-muted-foreground"
            tickLine={false}
            axisLine={false}
            width={32}
          />
          <Tooltip content={<CustomTooltip />} />
          <Line
            type="monotone"
            dataKey="score"
            stroke="#6366f1"
            strokeWidth={2}
            dot={(props: object) => {
              const dotProps = props as {
                cx: number;
                cy: number;
                payload: ChartDataPoint;
              };
              return (
                <CustomDot
                  key={`dot-${dotProps.payload.id}`}
                  cx={dotProps.cx}
                  cy={dotProps.cy}
                  payload={dotProps.payload}
                  currentScanId={currentScanId}
                  onDotClick={handleDotClick}
                />
              );
            }}
            activeDot={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
