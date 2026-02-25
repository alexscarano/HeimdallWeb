"use client";

import Link from "next/link";
import { Suspense, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter, useSearchParams } from "next/navigation";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { isAxiosError } from "axios";
import { resetPassword } from "@/lib/api/auth.api";
import { resetPasswordSchema, type ResetPasswordFormData } from "@/lib/validations/auth";
import { routes } from "@/lib/constants/routes";
import { Button } from "@/components/ui/button";
import { PasswordInput } from "@/components/ui/password-input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<ResetPasswordSkeleton />}>
      <ResetPasswordForm />
    </Suspense>
  );
}

function ResetPasswordSkeleton() {
  return (
    <Card className="border shadow-sm">
      <CardHeader className="space-y-1 text-center">
        <Skeleton className="mx-auto h-6 w-40" />
        <Skeleton className="mx-auto h-4 w-56" />
      </CardHeader>
      <CardContent className="space-y-4">
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-10 w-full" />
      </CardContent>
    </Card>
  );
}

function ResetPasswordForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [isLoading, setIsLoading] = useState(false);
  const [inlineError, setInlineError] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  });

  if (!token) {
    return (
      <Card className="border shadow-md">
        <CardContent className="pt-6">
          <div className="flex flex-col items-center gap-4 py-4 text-center">
            <p className="font-medium">Link inválido ou expirado</p>
            <p className="text-sm text-muted-foreground">
              O link de redefinição de senha é inválido ou expirou.
            </p>
            <Button asChild variant="outline" className="w-full">
              <Link href="/forgot-password">Solicitar novo link</Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    );
  }

  const onSubmit = async (data: ResetPasswordFormData) => {
    setIsLoading(true);
    setInlineError(false);
    try {
      await resetPassword(token, data.newPassword);
      toast.success("Senha alterada com sucesso!");
      setTimeout(() => {
        router.push(routes.login);
      }, 1500);
    } catch (error) {
      if (isAxiosError(error) && error.response && error.response.status >= 400 && error.response.status < 500) {
        setInlineError(true);
      } else {
        toast.error("Erro ao redefinir senha. Tente novamente.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Card className="border shadow-md">
      <CardHeader className="space-y-1 text-center">
        <CardTitle className="text-xl">Redefinir senha</CardTitle>
        <CardDescription>
          Crie uma nova senha para sua conta
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="newPassword">Nova senha</Label>
            <PasswordInput
              id="newPassword"
              placeholder="••••••••"
              autoComplete="new-password"
              disabled={isLoading}
              className="focus-ring-indigo"
              {...register("newPassword")}
            />
            {errors.newPassword && (
              <p className="text-xs text-destructive">{errors.newPassword.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Confirmar senha</Label>
            <PasswordInput
              id="confirmPassword"
              placeholder="••••••••"
              autoComplete="new-password"
              disabled={isLoading}
              className="focus-ring-indigo"
              {...register("confirmPassword")}
            />
            {errors.confirmPassword && (
              <p className="text-xs text-destructive">{errors.confirmPassword.message}</p>
            )}
          </div>

          {inlineError && (
            <p className="text-sm text-destructive">
              Link inválido ou expirado. Solicite um novo.{" "}
              <Link
                href="/forgot-password"
                className="underline underline-offset-4 hover:opacity-80"
              >
                Solicitar novo link
              </Link>
            </p>
          )}

          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Redefinir senha
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
