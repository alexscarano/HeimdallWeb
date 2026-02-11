"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { AlertCircle, ArrowLeft, ShieldAlert } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";

export default function HistoryDetailError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  const router = useRouter();

  useEffect(() => {
    // Log do erro para debugging (não expor detalhes ao usuário)
    console.error("History detail error:", error);
  }, [error]);

  const isPermissionError = 
    error.message.includes("permissão") || 
    error.message.includes("não encontrado");

  return (
    <div className="flex min-h-[60vh] items-center justify-center p-4">
      <Card className="w-full max-w-md p-8">
        <div className="flex flex-col items-center text-center">
          {/* Icon */}
          <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-destructive/10">
            {isPermissionError ? (
              <ShieldAlert className="h-8 w-8 text-destructive" />
            ) : (
              <AlertCircle className="h-8 w-8 text-destructive" />
            )}
          </div>

          {/* Title */}
          <h2 className="mb-2 text-xl font-semibold">
            {isPermissionError ? "Acesso Negado" : "Erro ao Carregar Scan"}
          </h2>

          {/* Message */}
          <p className="mb-6 text-sm text-muted-foreground">
            {isPermissionError
              ? "Este scan não existe ou você não tem permissão para acessá-lo."
              : "Ocorreu um erro ao tentar carregar os detalhes do scan. Tente novamente."}
          </p>

          {/* Actions */}
          <div className="flex w-full flex-col gap-3 sm:flex-row">
            <Button
              variant="outline"
              className="flex-1"
              onClick={() => router.push("/history")}
            >
              <ArrowLeft className="mr-2 h-4 w-4" />
              Voltar ao Histórico
            </Button>
            {!isPermissionError && (
              <Button className="flex-1" onClick={() => reset()}>
                Tentar Novamente
              </Button>
            )}
          </div>
        </div>
      </Card>
    </div>
  );
}
