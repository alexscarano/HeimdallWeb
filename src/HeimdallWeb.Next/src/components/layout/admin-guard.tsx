"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/stores/auth-store";
import { Shield } from "lucide-react";

export function AdminGuard({ children }: { children: React.ReactNode }) {
  const { isAdmin, isLoading, isAuthenticated } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && isAuthenticated && !isAdmin) {
      router.replace("/dashboard/user");
    }
  }, [isAdmin, isLoading, isAuthenticated, router]);

  if (isLoading) return null;

  if (!isAdmin) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <Shield className="mb-4 h-12 w-12 text-muted-foreground/40" />
        <p className="text-base font-medium">Acesso negado</p>
        <p className="mt-1 text-sm text-muted-foreground">
          Você não tem permissão para acessar esta página.
        </p>
      </div>
    );
  }

  return <>{children}</>;
}
