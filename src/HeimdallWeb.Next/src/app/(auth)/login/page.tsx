"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { useAuth } from "@/stores/auth-store";
import { loginSchema, type LoginFormData } from "@/lib/validations/auth";
import { routes } from "@/lib/constants/routes";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { isAxiosError } from "axios";

export default function LoginPage() {
  return (
    <Suspense fallback={<LoginSkeleton />}>
      <LoginForm />
    </Suspense>
  );
}

function LoginSkeleton() {
  return (
    <Card className="border shadow-sm">
      <CardHeader className="space-y-1 text-center">
        <Skeleton className="mx-auto h-6 w-24" />
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

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { login } = useAuth();
  const [isLoading, setIsLoading] = useState(false);

  const redirect = searchParams.get("redirect") ?? routes.home;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    try {
      await login(data);
      toast.success("Login realizado com sucesso!");
      router.push(redirect);
    } catch (error) {
      if (isAxiosError(error)) {
        const message = error.response?.data?.message ?? "Credenciais inválidas";
        toast.error(message);
      } else {
        toast.error("Erro ao realizar login");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Card className="border shadow-sm">
      <CardHeader className="space-y-1 text-center">
        <CardTitle className="text-xl">Entrar</CardTitle>
        <CardDescription>
          Informe suas credenciais para acessar o sistema
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="emailOrLogin">Email ou Usuário</Label>
            <Input
              id="emailOrLogin"
              type="text"
              placeholder="seu@email.com"
              autoComplete="username"
              disabled={isLoading}
              {...register("emailOrLogin")}
            />
            {errors.emailOrLogin && (
              <p className="text-xs text-destructive">{errors.emailOrLogin.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="password">Senha</Label>
            <Input
              id="password"
              type="password"
              placeholder="••••••••"
              autoComplete="current-password"
              disabled={isLoading}
              {...register("password")}
            />
            {errors.password && (
              <p className="text-xs text-destructive">{errors.password.message}</p>
            )}
          </div>

          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Entrar
          </Button>
        </form>

        <p className="mt-4 text-center text-sm text-muted-foreground">
          Não tem uma conta?{" "}
          <Link
            href={routes.register}
            className="font-medium text-foreground underline-offset-4 hover:underline"
          >
            Criar conta
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}
