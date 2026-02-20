"use client";

import { Shield, ShieldCheck, Radar, FileSearch, Lock, Globe, Network } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { ScanForm } from "@/components/scan/scan-form";
import { ScanLoading } from "@/components/scan/scan-loading";
import { ScanResultSummary } from "@/components/scan/scan-result-summary";
import { ParticleBackground } from "@/components/ui/particle-background";
import { useScan } from "@/lib/hooks/use-scan";
import { toast } from "sonner";

const features = [
  {
    icon: ShieldCheck,
    title: "Headers de Segurança",
    description: "HSTS, CSP, X-Frame-Options e mais.",
  },
  {
    icon: Lock,
    title: "SSL & TLS",
    description: "Certificados, protocolos e ciphers.",
  },
  {
    icon: Radar,
    title: "Análise de Portas",
    description: "Portas abertas e serviços expostos.",
  },
  {
    icon: FileSearch,
    title: "Caminhos Sensíveis",
    description: "Arquivos e diretórios expostos.",
  },
  {
    icon: Globe,
    title: "DNS & Domínio",
    description: "Resolução IP, CDN e idade do domínio.",
  },
  {
    icon: Network,
    title: "Subdomínios",
    description: "Descoberta automática de subdomínios.",
  },
];

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
        <div className="flex flex-col items-center gap-6 text-center">
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
        <div className="mt-8 flex w-full justify-center">
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

        {/* Error toast is handled by useScan + sonner, but show inline too */}
        {scan.error && !scan.isScanning && !scan.result && (
          <p className="mt-4 text-sm text-destructive">{scan.error}</p>
        )}

        {/* Features grid — only visible when not scanning */}
        {!scan.isScanning && !scan.result && (
          <div className="mt-12 w-full max-w-2xl space-y-6">

            <div className="flex items-center gap-4">
              <Separator className="flex-1 opacity-20" />
              <h2 className="text-sm font-medium uppercase tracking-widest text-muted-foreground">
                Scans nos quais trabalhamos
              </h2>
              <Separator className="flex-1 opacity-20" />
            </div>

            {/* Row 1 */}
            <div className="grid grid-cols-3 gap-4">
              {features.slice(0, 3).map((feature) => (
                <Card key={feature.title} className="border bg-background/80 backdrop-blur-sm transition-colors hover:border-accent-primary-border">
                  <CardContent className="flex flex-col items-center gap-3 py-6 text-center">
                    <div className="flex h-11 w-11 items-center justify-center rounded-full bg-accent-primary-subtle">
                      <feature.icon className="h-5 w-5 text-accent-primary" />
                    </div>
                    <h3 className="text-sm font-semibold leading-tight">{feature.title}</h3>
                    <p className="text-xs leading-relaxed text-muted-foreground">
                      {feature.description}
                    </p>
                  </CardContent>
                </Card>
              ))}
            </div>

            {/* Row 2 */}
            <div className="grid grid-cols-3 gap-4">
              {features.slice(3, 6).map((feature) => (
                <Card key={feature.title} className="border bg-background/80 backdrop-blur-sm transition-colors hover:border-accent-primary-border">
                  <CardContent className="flex flex-col items-center gap-3 py-6 text-center">
                    <div className="flex h-11 w-11 items-center justify-center rounded-full bg-accent-primary-subtle">
                      <feature.icon className="h-5 w-5 text-accent-primary" />
                    </div>
                    <h3 className="text-sm font-semibold leading-tight">{feature.title}</h3>
                    <p className="text-xs leading-relaxed text-muted-foreground">
                      {feature.description}
                    </p>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>
        )}
      </div>
    </>
  );
}
