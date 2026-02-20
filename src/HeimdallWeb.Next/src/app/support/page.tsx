"use client";

import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod/v4";
import { toast } from "sonner";
import { ArrowLeft } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { apiClient } from "@/lib/api/client";

// ─── Zod schema ───────────────────────────────────────────────────────────────

const supportSchema = z.object({
  name: z.string().min(2, "Nome deve ter no mínimo 2 caracteres"),
  email: z.email("Insira um e-mail válido"),
  subject: z.enum(["Dúvida técnica", "Bug / Problema", "Sugestão", "Outro"]),
  message: z
    .string()
    .min(20, "Mensagem deve ter no mínimo 20 caracteres")
    .max(1000, "Mensagem deve ter no máximo 1000 caracteres"),
});

type SupportForm = z.infer<typeof supportSchema>;

// ─── Support Page ─────────────────────────────────────────────────────────────

export default function SupportPage() {
  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SupportForm>({
    resolver: zodResolver(supportSchema),
  });

  const onSubmit = async (values: SupportForm) => {
    try {
      await apiClient.post("/support/contact", values);
      toast.success("Mensagem enviada!");
      reset();
    } catch {
      toast.error("Erro ao enviar. Tente novamente.");
    }
  };

  return (
    <div className="min-h-screen bg-background">
      {/* Minimal header */}
      <header className="sticky top-0 z-50 border-b border-border bg-background/80 backdrop-blur-sm">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center gap-4">
            <Link
              href="/"
              className="flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-foreground"
            >
              <ArrowLeft className="h-4 w-4" />
              Início
            </Link>
            <span className="text-muted-foreground">/</span>
            <span className="text-sm font-medium">Suporte</span>
          </div>
        </div>
      </header>

      {/* Content */}
      <main className="mx-auto max-w-lg px-4 py-16 sm:px-6">
        <Card>
          <CardHeader>
            <CardTitle>Fale conosco</CardTitle>
            <CardDescription>
              Tem alguma dúvida ou sugestão? Estamos aqui para ajudar.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              {/* Name */}
              <div className="space-y-1.5">
                <Label htmlFor="support-name">Nome</Label>
                <Input
                  id="support-name"
                  placeholder="Seu nome"
                  {...register("name")}
                />
                {errors.name && (
                  <p className="text-xs text-destructive">{errors.name.message}</p>
                )}
              </div>

              {/* Email */}
              <div className="space-y-1.5">
                <Label htmlFor="support-email">E-mail</Label>
                <Input
                  id="support-email"
                  type="email"
                  placeholder="seu@email.com"
                  {...register("email")}
                />
                {errors.email && (
                  <p className="text-xs text-destructive">{errors.email.message}</p>
                )}
              </div>

              {/* Subject */}
              <div className="space-y-1.5">
                <Label htmlFor="support-subject">Assunto</Label>
                <Select
                  onValueChange={(val) =>
                    setValue("subject", val as SupportForm["subject"])
                  }
                >
                  <SelectTrigger id="support-subject" className="w-full">
                    <SelectValue placeholder="Selecione o assunto" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Dúvida técnica">Dúvida técnica</SelectItem>
                    <SelectItem value="Bug / Problema">Bug / Problema</SelectItem>
                    <SelectItem value="Sugestão">Sugestão</SelectItem>
                    <SelectItem value="Outro">Outro</SelectItem>
                  </SelectContent>
                </Select>
                {errors.subject && (
                  <p className="text-xs text-destructive">{errors.subject.message}</p>
                )}
              </div>

              {/* Message */}
              <div className="space-y-1.5">
                <Label htmlFor="support-message">Mensagem</Label>
                <Textarea
                  id="support-message"
                  placeholder="Descreva sua dúvida ou sugestão..."
                  rows={5}
                  {...register("message")}
                />
                {errors.message && (
                  <p className="text-xs text-destructive">{errors.message.message}</p>
                )}
              </div>

              <Button type="submit" className="w-full" disabled={isSubmitting}>
                {isSubmitting ? "Enviando..." : "Enviar mensagem"}
              </Button>
            </form>
          </CardContent>
        </Card>
      </main>
    </div>
  );
}
