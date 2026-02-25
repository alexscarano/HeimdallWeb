"use client";

import Link from "next/link";
import Script from "next/script";
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
import { isAxiosError } from "axios";

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: { credential: string }) => void;
          }) => void;
          prompt: () => void;
        };
      };
    };
  }
}

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

const GOOGLE_CLIENT_ID = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { login, loginWithGoogle } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [isGoogleLoading, setIsGoogleLoading] = useState(false);

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

  const handleGoogleLogin = () => {
    if (!GOOGLE_CLIENT_ID || !window.google) return;
    setIsGoogleLoading(true);
    try {
      window.google.accounts.id.initialize({
        client_id: GOOGLE_CLIENT_ID,
        callback: async ({ credential }) => {
          try {
            await loginWithGoogle(credential);
            toast.success("Login realizado com sucesso!");
            router.push(redirect);
          } catch {
            toast.error("Erro ao autenticar com Google");
          } finally {
            setIsGoogleLoading(false);
          }
        },
      });
      // Trigger the prompt without callback arguments since it's breaking TS
      window.google.accounts.id.prompt();
    } catch {
      toast.error("Erro ao inicializar Google Login");
      setIsGoogleLoading(false);
    }
  };

  return (
    <>
      {GOOGLE_CLIENT_ID && (
        <Script
          src="https://accounts.google.com/gsi/client"
          strategy="afterInteractive"
        />
      )}
      <Card className="border shadow-md">
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
                className="focus-ring-indigo"
                {...register("emailOrLogin")}
              />
              {errors.emailOrLogin && (
                <p className="text-xs text-destructive">{errors.emailOrLogin.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">Senha</Label>
              <PasswordInput
                id="password"
                placeholder="••••••••"
                autoComplete="current-password"
                disabled={isLoading}
                className="focus-ring-indigo"
                {...register("password")}
              />
              {errors.password && (
                <p className="text-xs text-destructive">{errors.password.message}</p>
              )}
              <div className="flex justify-end">
                <Link
                  href="/forgot-password"
                  className="text-sm text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
                >
                  Esqueci minha senha
                </Link>
              </div>
            </div>

            <Button type="submit" className="w-full" disabled={isLoading}>
              {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Entrar
            </Button>
          </form>

          {GOOGLE_CLIENT_ID && (
            <>
              <div className="relative my-4 flex items-center">
                <div className="flex-1 border-t border-border" />
                <span className="mx-3 text-xs text-muted-foreground">ou</span>
                <div className="flex-1 border-t border-border" />
              </div>

              <Button
                type="button"
                variant="outline"
                className="w-full"
                disabled={isGoogleLoading || isLoading}
                onClick={handleGoogleLogin}
              >
                {isGoogleLoading ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <svg
                    className="mr-2 h-4 w-4"
                    viewBox="0 0 24 24"
                    aria-hidden="true"
                  >
                    <path
                      d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                      fill="#4285F4"
                    />
                    <path
                      d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                      fill="#34A853"
                    />
                    <path
                      d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                      fill="#FBBC05"
                    />
                    <path
                      d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                      fill="#EA4335"
                    />
                  </svg>
                )}
                Continuar com Google
              </Button>
            </>
          )}

          <p className="mt-4 text-center text-sm text-muted-foreground">
            Não tem uma conta?{" "}
            <Link
              href={routes.register}
              className="font-medium text-accent-primary underline-offset-4 hover:underline"
            >
              Criar conta
            </Link>
          </p>
        </CardContent>
      </Card>
    </>
  );
}
