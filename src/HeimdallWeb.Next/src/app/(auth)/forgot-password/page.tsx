"use client";

import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCircle, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { isAxiosError } from "axios";
import { forgotPassword } from "@/lib/api/auth.api";
import { forgotPasswordSchema, type ForgotPasswordFormData } from "@/lib/validations/auth";
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

export default function ForgotPasswordPage() {
  const [isLoading, setIsLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const onSubmit = async (data: ForgotPasswordFormData) => {
    setIsLoading(true);
    try {
      await forgotPassword(data.email);
      setSubmitted(true);
    } catch (error) {
      if (isAxiosError(error)) {
        toast.error("Erro ao processar solicitação. Tente novamente.");
      } else {
        toast.error("Erro inesperado. Tente novamente.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  if (submitted) {
    return (
      <Card className="border shadow-md">
        <CardContent className="pt-6">
          <div className="flex flex-col items-center gap-4 py-4 text-center">
            <CheckCircle className="h-12 w-12 text-accent-primary" />
            <div className="space-y-1">
              <p className="font-medium">Solicitação enviada</p>
              <p className="text-sm text-muted-foreground">
                Se o email estiver cadastrado, você receberá um link em breve.
              </p>
            </div>
            <Link
              href={routes.login}
              className="text-sm text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
            >
              ← Voltar ao login
            </Link>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="border shadow-md">
      <CardHeader className="space-y-1 text-center">
        <CardTitle className="text-xl">Esqueci minha senha</CardTitle>
        <CardDescription>
          Informe seu email para receber um link de recuperação
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="seu@email.com"
              autoComplete="email"
              disabled={isLoading}
              className="focus-ring-indigo"
              {...register("email")}
            />
            {errors.email && (
              <p className="text-xs text-destructive">{errors.email.message}</p>
            )}
          </div>

          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Enviar link de recuperação
          </Button>
        </form>

        <p className="mt-4 text-center text-sm">
          <Link
            href={routes.login}
            className="text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
          >
            ← Voltar ao login
          </Link>
        </p>
      </CardContent>
    </Card>
  );
}
