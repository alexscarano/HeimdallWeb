"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Globe, ArrowRight, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormMessage,
} from "@/components/ui/form";
import { scanSchema, type ScanFormData } from "@/lib/validations/scan";

interface ScanFormProps {
  onSubmit: (target: string) => void;
  isScanning: boolean;
}

export function ScanForm({ onSubmit, isScanning }: ScanFormProps) {
  const form = useForm<ScanFormData>({
    resolver: zodResolver(scanSchema),
    defaultValues: { target: "" },
  });

  function handleSubmit(data: ScanFormData) {
    onSubmit(data.target);
  }

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(handleSubmit)}
        className="flex w-full max-w-2xl flex-col gap-3 sm:flex-row sm:gap-2"
      >
        <FormField
          control={form.control}
          name="target"
          render={({ field }) => (
            <FormItem className="flex-1">
              <FormControl>
                <div className="relative">
                  <Globe className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    {...field}
                    placeholder="https://exemplo.com"
                    className="h-12 pl-10 text-base"
                    disabled={isScanning}
                    autoComplete="url"
                    aria-label="URL do alvo para scan"
                  />
                </div>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button
          type="submit"
          size="lg"
          className="h-12 min-w-[140px] gap-2"
          disabled={isScanning}
        >
          {isScanning ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              Escaneando...
            </>
          ) : (
            <>
              Escanear
              <ArrowRight className="h-4 w-4" />
            </>
          )}
        </Button>
      </form>
    </Form>
  );
}
