"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Globe,
  ArrowRight,
  Loader2,
  Zap,
  Shield,
  Search,
} from "lucide-react";
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
import { fetchScanProfiles } from "@/lib/api/scan.api";
import type { ScanProfile } from "@/types/scan";
import { cn } from "@/lib/utils";

interface ScanFormProps {
  onSubmit: (target: string, profileId?: number | null) => void;
  isScanning: boolean;
}



// Profile icon mapping
const PROFILE_ICONS: Record<string, React.ElementType> = {
  Quick: Zap,
  Rápido: Zap,
  Standard: Shield,
  Padrão: Shield,
  Deep: Search,
  Profundo: Search,
};

export function ScanForm({ onSubmit, isScanning }: ScanFormProps) {
  const [profiles, setProfiles] = useState<ScanProfile[]>([]); // Start empty
  const [isLoadingProfiles, setIsLoadingProfiles] = useState(true);
  const [selectedProfileId, setSelectedProfileId] = useState<number | null>(null);

  const form = useForm<ScanFormData>({
    resolver: zodResolver(scanSchema),
    defaultValues: { target: "" },
  });

  // Fetch profiles on mount
  useEffect(() => {
    let cancelled = false;
    fetchScanProfiles()
      .then((data) => {
        if (!cancelled) {
          if (data.length > 0) {
            setProfiles(data);
            // Auto-select "Standard" or the first profile if no selection yet
            const defaultProfile = data.find((p) => p.name === "Standard") || data[0];
            if (defaultProfile) {
              setSelectedProfileId(defaultProfile.id);
            }
          }
          setIsLoadingProfiles(false);
        }
      })
      .catch(() => {
        // Handle error if needed, but for now just stop loading
        if (!cancelled) setIsLoadingProfiles(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  function handleSubmit(data: ScanFormData) {
    // id=0 means "default" (no profileId sent to backend)
    const profileId = selectedProfileId && selectedProfileId > 0 ? selectedProfileId : null;
    onSubmit(data.target, profileId);
  }

  // Translation map for profile descriptions
  const PROFILE_DESCRIPTIONS: Record<string, string> = {
    "Quick": "Scan rápido cobrindo verificações de segurança essenciais. Adequado para verificações frequentes.",
    "Standard": "Scan balanceado incluindo as vulnerabilidades mais comuns e relevantes.",
    "Deep": "Scan abrangente que executa todos os scanners disponíveis. Análise completa de segurança.",
  };

  return (
    <div className="w-full max-w-3xl space-y-8">
      {/* URL Form */}
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(handleSubmit)}
          className="flex w-full flex-col gap-3 sm:flex-row sm:gap-4"
        >
          <FormField
            control={form.control}
            name="target"
            render={({ field }) => (
              <FormItem className="flex-1">
                <FormControl>
                  <div className="url-input-glow relative rounded-2xl bg-background transition-all duration-200">
                    <Globe className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-muted-foreground/70" />
                    <Input
                      {...field}
                      placeholder="https://exemplo.com"
                      className="h-14 rounded-2xl border-2 pl-12 font-mono text-base shadow-sm focus-visible:border-accent-primary focus-visible:ring-0 focus-visible:ring-offset-0"
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
            className="h-14 min-w-[160px] rounded-2xl px-8 text-base font-semibold shadow-lg transition-all hover:scale-105 hover:shadow-xl active:scale-95"
            disabled={isScanning || isLoadingProfiles}
          >
            {isScanning ? (
              <>
                <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                Escaneando
              </>
            ) : (
              <>
                Escanear
                <ArrowRight className="ml-2 h-5 w-5" />
              </>
            )}
          </Button>
        </form>
      </Form>

      {/* Profile selector — moved below URL input */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        {isLoadingProfiles ? (
          // Skeleton Loading State
          Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="flex h-[200px] flex-col items-center gap-4 rounded-2xl border-2 border-muted bg-card/50 p-6">
              <div className="h-14 w-14 animate-pulse rounded-2xl bg-muted" />
              <div className="space-y-2 w-full flex flex-col items-center">
                <div className="h-6 w-24 animate-pulse rounded bg-muted" />
                <div className="h-12 w-full animate-pulse rounded bg-muted/50" />
              </div>
            </div>
          ))
        ) : (
          profiles.map((profile) => {
            const isSelected = (selectedProfileId ?? 0) === profile.id;
            const IconComponent = PROFILE_ICONS[profile.name] ?? Shield;
            const description = PROFILE_DESCRIPTIONS[profile.name] || profile.description;

            return (
              <button
                key={profile.id}
                type="button"
                disabled={isScanning}
                onClick={() => setSelectedProfileId(profile.id)}
                className={cn(
                  "relative flex flex-col items-center gap-4 rounded-2xl border-2 p-6 text-center transition-all duration-300",
                  "hover:border-accent-primary/50 hover:bg-accent-primary-subtle/30 hover:-translate-y-1 hover:shadow-lg",
                  isSelected
                    ? "border-accent-primary bg-accent-primary-subtle/40 shadow-md ring-1 ring-accent-primary/20"
                    : "border-muted bg-card/50"
                )}
              >
                <div
                  className={cn(
                    "flex h-14 w-14 items-center justify-center rounded-2xl transition-all duration-300 shadow-sm",
                    isSelected
                      ? "bg-accent-primary text-accent-primary-foreground scale-110"
                      : "bg-muted text-foreground/70 group-hover:bg-accent-primary/20 group-hover:text-accent-primary group-hover:scale-110"
                  )}
                >
                  <IconComponent className="h-7 w-7" />
                </div>

                <div className="space-y-2">
                  <h4 className={cn(
                    "text-lg font-bold tracking-tight transition-colors",
                    isSelected ? "text-accent-primary" : "text-foreground"
                  )}>
                    {profile.name}
                  </h4>
                  <p className="text-sm leading-relaxed text-muted-foreground">
                    {description}
                  </p>
                </div>

                {isSelected && (
                  <div className="absolute inset-0 rounded-2xl ring-2 ring-accent-primary/20 pointer-events-none" />
                )}
              </button>
            );
          })
        )}
      </div>
    </div>
  );
}
