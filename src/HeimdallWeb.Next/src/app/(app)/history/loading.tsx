import { Skeleton } from "@/components/ui/skeleton";

export default function HistoryLoading() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-64" />
      <Skeleton className="h-14 w-full rounded-xl" />
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-16 w-full rounded-xl" />
        ))}
      </div>
    </div>
  );
}
