"use client";

import { format } from "date-fns";
import { MoreVertical, Eye, FileDown, Trash2, FileText } from "lucide-react";
import Link from "next/link";
import { ScanHistory } from "@/lib/hooks/use-history";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";

interface ScanTableProps {
  scans: ScanHistory[];
  isLoading: boolean;
  onDelete: (id: string) => void;
  onExportPdf: (id: string) => void;
}

export function ScanTable({ scans, isLoading, onDelete, onExportPdf }: ScanTableProps) {
  if (isLoading) {
    return <ScanTableSkeleton />;
  }

  if (scans.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center rounded-lg border border-dashed py-16 text-center">
        <FileText className="mb-3 h-12 w-12 text-muted-foreground/50" />
        <p className="text-base font-medium">Nenhum scan encontrado</p>
        <p className="mt-1 text-sm text-muted-foreground">
          Seus scans de segurança aparecerão aqui.
        </p>
      </div>
    );
  }

  return (
    <div className="rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow className="bg-muted/50">
            <TableHead>URL Alvo</TableHead>
            <TableHead>Data</TableHead>
            <TableHead>Duração</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="w-[70px]"></TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {scans.map((scan) => (
            <TableRow key={scan.historyId} className="hover:bg-muted/50">
              <TableCell className="font-medium">
                <Link
                  href={`/history/${scan.historyId}`}
                  className="hover:text-accent-primary hover:underline"
                >
                  {scan.target}
                </Link>
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {format(new Date(scan.createdDate), "dd/MM/yyyy, HH:mm")}
              </TableCell>
              <TableCell className="text-sm">{scan.duration}</TableCell>
              <TableCell>
                <Badge
                  className={
                    scan.hasCompleted
                      ? "bg-success-bg text-success border-success-border"
                      : "bg-destructive text-white"
                  }
                >
                  {scan.hasCompleted ? "Completo" : "Falhado"}
                </Badge>
              </TableCell>
              <TableCell>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="sm">
                      <MoreVertical className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem asChild>
                      <Link href={`/history/${scan.historyId}`}>
                        <Eye className="mr-2 h-4 w-4" />
                        Ver detalhes
                      </Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => onExportPdf(scan.historyId)}>
                      <FileDown className="mr-2 h-4 w-4" />
                      Exportar PDF
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => onDelete(scan.historyId)}
                      className="text-destructive focus:text-destructive"
                    >
                      <Trash2 className="mr-2 h-4 w-4" />
                      Deletar
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

function ScanTableSkeleton() {
  return (
    <div className="rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow className="bg-muted/50">
            <TableHead>URL Alvo</TableHead>
            <TableHead>Data</TableHead>
            <TableHead>Duração</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="w-[70px]"></TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell>
                <Skeleton className="h-5 w-48" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-5 w-32" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-5 w-16" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-6 w-20" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-8 w-8" />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
