"use client";

import { Shield, ShieldCheck, Radar, FileSearch } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
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
    description: "Verifica headers HTTP essenciais como HSTS, CSP e X-Frame-Options.",
  },
  {
    icon: Radar,
    title: "Análise de Portas",
    description: "Identifica portas abertas e serviços expostos na infraestrutura.",
  },
  {
    icon: FileSearch,
    title: "Caminhos Sensíveis",
    description: "Detecta arquivos e diretórios que não deveriam estar expostos.",
  },
];

export default function HomePage() {
  const scan = useScan();

  function handleSubmit(target: string) {
    toast.info(`Iniciando scan em ${target}...`);
    scan.submit(target);
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
            segurança com 7 scanners especializados.
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
        <div className="mt-16 grid w-full max-w-3xl grid-cols-1 gap-4 sm:grid-cols-3">
          {features.map((feature) => (
            <Card key={feature.title} className="border bg-background transition-colors hover:border-accent-primary-border">
              <CardContent className="flex flex-col items-center gap-3 py-6 text-center">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-accent-primary-subtle">
                  <feature.icon className="h-5 w-5 text-accent-primary" />
                </div>
                <h3 className="text-sm font-medium">{feature.title}</h3>
                <p className="text-xs text-muted-foreground">
                  {feature.description}
                </p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
      </div>
    </>
  );
}

