"use client";

import { Shield, ShieldCheck, Radar, FileSearch, Lock, Globe, Network, Zap, Search } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { ScanForm } from "@/components/scan/scan-form";
import { ScanLoading } from "@/components/scan/scan-loading";
import { ScanResultSummary } from "@/components/scan/scan-result-summary";
import { ParticleBackground } from "@/components/ui/particle-background";
import { useScan } from "@/lib/hooks/use-scan";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const scanProfilesDetail = [
  {
    icon: Zap,
    title: "Rápido",
    description: "Verificações essenciais de segurança. Ideal para monitoramento contínuo.",
    estimatedTime: "~15s",
    scans: [
      { name: "Headers de Segurança", speed: "fast" },
      { name: "Redirecionamento HTTPS", speed: "fast" },
      { name: "Certificado SSL", speed: "fast" },
    ],
  },
  {
    icon: Shield,
    title: "Padrão",
    description: "Scan balanceado com as vulnerabilidades mais comuns e relevantes.",
    estimatedTime: "~45s",
    scans: [
      { name: "Todos do perfil Rápido", speed: "fast" },
      { name: "Robots.txt & Sitemap", speed: "medium" },
      { name: "Scanner de Portas", speed: "slow" },
    ],
  },
  {
    icon: Search,
    title: "Profundo",
    description: "Análise com todos os scanners de segurança na infraestrutura.",
    estimatedTime: "~1m30s",
    scans: [
      { name: "Todos do perfil Padrão", speed: "medium" },
      { name: "Caminhos Sensíveis", speed: "slow" },
    ],
  },
];

const SPEED_COLORS: Record<string, string> = {
  fast: "bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]",
  medium: "bg-amber-500 shadow-[0_0_8px_rgba(245,158,11,0.5)]",
  slow: "bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.5)]",
};

export default function ScanPage() {
  const scan = useScan();

  function handleSubmit(target: string, profileId?: number | null, enabledScanners?: string[] | null) {
    toast.info(`Iniciando scan em ${target}...`);
    scan.submit(target, profileId, enabledScanners);
  }

  return (
    <>
      {/* Particle background - visible in both light and dark mode */}
      <ParticleBackground />

      <div className="relative z-10 flex flex-1 flex-col items-center justify-center px-4">
        {/* Hero section */}
        <div className="flex flex-col items-center gap-6 text-center mt-12 mb-8">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-accent-primary-subtle">
            <Shield className="h-8 w-8 text-accent-primary" />
          </div>
          <div className="space-y-2">
            <h1 className="text-4xl font-bold tracking-tight">
              Análise de Segurança Web
            </h1>
            <p className="mx-auto max-w-md text-muted-foreground">
              Insira a URL do seu site para executar uma análise completa de
              segurança com 13 scanners especializados.
            </p>
          </div>
        </div>

        {/* Scan form / loading / result */}
        <div className="flex w-full justify-center">
          <div className="w-full max-w-5xl">
            {scan.result ? (
              <ScanResultSummary result={scan.result} onNewScan={scan.reset} />
            ) : scan.isScanning ? (
              <ScanLoading
                elapsedSeconds={scan.elapsedSeconds}
                timeoutSeconds={scan.timeoutSeconds}
              />
            ) : (
              <ScanForm onSubmit={handleSubmit} isScanning={scan.isScanning} />
            )}
          </div>
        </div>

        {/* Error toast is handled by useScan + sonner, but show inline too */}
        {scan.error && !scan.isScanning && !scan.result && (
          <p className="mt-4 text-sm text-destructive">{scan.error}</p>
        )}

        {/* Detailed Scan Profiles Section */}
        {!scan.isScanning && !scan.result && (
          <div className="mt-16 mb-20 w-full max-w-5xl space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700 delay-150 fill-mode-both">
            <div className="flex items-center gap-4">
              <Separator className="flex-1 opacity-20" />
              <h2 className="text-sm font-medium uppercase tracking-widest text-muted-foreground">
                Perfis de Scan e Desempenho
              </h2>
              <Separator className="flex-1 opacity-20" />
            </div>

            <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
              {scanProfilesDetail.map((profile) => (
                <Card key={profile.title} className="flex flex-col justify-between border bg-background/80 backdrop-blur-sm transition-colors hover:border-accent-primary-border overflow-hidden">
                  <div className="p-6">
                    <div className="flex flex-col items-center justify-center gap-2 mb-4">
                      <div className="flex flex-col items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-accent-primary-subtle">
                          <profile.icon className="h-5 w-5 text-accent-primary" />
                        </div>
                        <h3 className="font-bold text-lg">{profile.title}</h3>
                      </div>
                      <span className="text-xs font-semibold px-2 py-1 bg-muted rounded-md text-muted-foreground w-fit">
                        {profile.estimatedTime}
                      </span>
                    </div>

                    <p className="text-sm text-center text-muted-foreground mb-8 min-h-[60px]">
                      {profile.description}
                    </p>

                    <div className="space-y-3 flex flex-col items-center">
                      <h4 className="text-xs font-semibold uppercase text-center text-muted-foreground/70 tracking-wider mb-2">Scanners Inclusos</h4>
                      <ul className="space-y-2.5 inline-block text-left">
                        {profile.scans.map((subscan, idx) => (
                          <li key={idx} className="flex items-center gap-2.5 text-sm w-fit">
                            <span
                              className={cn(
                                "block h-2 w-2 shrink-0 rounded-full",
                                SPEED_COLORS[subscan.speed]
                              )}
                              title={subscan.speed === 'fast' ? "Rápido" : subscan.speed === 'medium' ? "Tempo Médio" : "Demorado"}
                            />
                            <span className="text-foreground/90 font-medium">{subscan.name}</span>
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>
                </Card>
              ))}
            </div>

            <div className="flex items-center justify-center gap-6 mt-4 opacity-70">
              <div className="flex items-center gap-2 text-xs font-medium text-muted-foreground">
                <div className="h-2 w-2 rounded-full bg-emerald-500" /> Rápido
              </div>
              <div className="flex items-center gap-2 text-xs font-medium text-muted-foreground">
                <div className="h-2 w-2 rounded-full bg-amber-500" /> Tempo Médio
              </div>
              <div className="flex items-center gap-2 text-xs font-medium text-muted-foreground">
                <div className="h-2 w-2 rounded-full bg-rose-500" /> Demorado
              </div>
            </div>
          </div>
        )}
      </div>
    </>
  );
}
